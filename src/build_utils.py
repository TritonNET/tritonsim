import os
import sys
import subprocess

def log(msg, level="INFO"):
    print(f"[{level}] {msg}")

def run_command(cmd, cwd=None, env=None, shell=True):
    """Runs a shell command and exits if it fails."""
    if cwd:
        log(f"Running in {cwd}: {cmd}")
    else:
        log(f"Running: {cmd}")
        
    try:
        result = subprocess.run(cmd, cwd=cwd, env=env, shell=shell, check=True)
    except subprocess.CalledProcessError as e:
        log(f"Command failed with exit code {e.returncode}", "ERROR")
        sys.exit(1)

def load_env_from_bat(bat_path):
    """
    Runs a batch file and captures the environment variables it sets,
    updating the current Python process's os.environ.
    """
    if not os.path.exists(bat_path):
        return False
    
    log(f"Loading environment from: {bat_path}")
    
    cmd = f'"{bat_path}" >nul && set'
    
    try:
        output = subprocess.check_output(cmd, shell=True, text=True)
        for line in output.splitlines():
            if '=' in line:
                key, _, value = line.partition('=')
                os.environ[key.upper()] = value 
                os.environ[key] = value
        return True
    except subprocess.CalledProcessError:
        return False