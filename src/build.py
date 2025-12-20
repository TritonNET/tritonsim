import os
import sys
import json
import subprocess
import argparse
import shutil
import multiprocessing

# ============================================================================
# 1. UTILITY FUNCTIONS (Stateless)
# ============================================================================

def log(msg, level="INFO"):
    print(f"[{level}] {msg}")

def run_command(cmd, cwd=None, env=None, shell=True):
    """Runs a shell command and exits if it fails."""
    if cwd:
        log(f"Running in {cwd}: {cmd}")
    else:
        log(f"Running: {cmd}")
        
    try:
        # On Windows, shell=True is generally needed for built-ins and bat files
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
    
    # Run the bat file, then run 'set' to dump environment variables
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

# ============================================================================
# 2. CONFIGURATION & CONTEXT SETUP
# ============================================================================

def create_build_context():
    """
    Parses arguments, loads config, validates paths, and returns a context dictionary.
    """
    # 1. Parse Arguments
    parser = argparse.ArgumentParser(description="Build Script")
    parser.add_argument("-Config", default="build.config", help="Path to JSON configuration file")
    parser.add_argument("-Configuration", default="Release", help="Build Configuration (Debug/Release)")
    parser.add_argument("-Platform", default="win_x64", help="Target Platform (win_x64/web/web_bgfx/web_sim/web_gui)")
    
    args = parser.parse_args()
    
    # 2. Determine Root Paths
    src_root = os.path.dirname(os.path.abspath(__file__))
    config_file_path = args.Config
    
    # Handle config path if it's relative
    if not os.path.isabs(config_file_path):
        config_file_path = os.path.join(src_root, config_file_path)

    if not os.path.exists(config_file_path):
        log(f"Configuration file not found: {config_file_path}", "ERROR")
        sys.exit(1)

    # 3. Load JSON
    try:
        with open(config_file_path, 'r') as f:
            config_data = json.load(f)
    except json.JSONDecodeError as e:
        log(f"Error parsing JSON config: {e}", "ERROR")
        sys.exit(1)

    # 4. Resolve Paths and Build Context Dict
    try:
        thirdparty_dir = os.path.join(src_root, config_data["directories"]["thirdparty"])
        bx_dir = os.path.join(thirdparty_dir, "bx")
        
        ctx = {
            "platform": args.Platform,
            "configuration": args.Configuration,
            "src_root": src_root,
            "paths": {
                "thirdparty": thirdparty_dir,
                "renderer": os.path.join(src_root, config_data["directories"]["renderer"]),
                "bgfx": os.path.join(thirdparty_dir, "bgfx"),
                "bx": bx_dir,
                "emsdk": os.path.join(thirdparty_dir, "emsdk"),
                # GUI Paths
                "gui": os.path.join(src_root, "TritonSim.GUI"),
                "gui_browser": os.path.join(src_root, "TritonSim.GUI.Browser"),
            },
            "tools": {
                "genie": os.path.join(bx_dir, "tools", "bin", "windows", "genie.exe"),
                "em_version": config_data["tools"]["em_version"],
                "vs_vars_path": config_data["tools"]["vs_vars_path"]
            }
        }
    except KeyError as e:
        log(f"Missing key in configuration file: {e}", "ERROR")
        sys.exit(1)

    log(f"Target Platform: {ctx['platform']}")
    log(f"Configuration:   {ctx['configuration']}")
    log(f"Loaded Config:   {config_file_path}")

    # 5. Basic Validation
    if not os.path.exists(ctx["paths"]["thirdparty"]):
        log(f"Could not find 3rdparty directory at: {ctx['paths']['thirdparty']}", "ERROR")
        sys.exit(1)

    if not os.path.exists(ctx["tools"]["genie"]):
        log(f"Could not find GENie build tool at: {ctx['tools']['genie']}", "ERROR")
        sys.exit(1)

    return ctx

# ============================================================================
# 3. WINDOWS BUILD ROUTINE
# ============================================================================

def build_win(ctx):
    log(f"\nStarting Windows Build ({ctx['platform']})...")
    log("-" * 75)

    # Load VS Env if NMAKE is missing (common check for Windows dev environment)
    if shutil.which("nmake") is None:
        log("MSVC tools not found in PATH. Attempting to load vcvars64.bat...")
        if not load_env_from_bat(ctx["tools"]["vs_vars_path"]):
            log("Could not find vcvars64.bat and tools are not in PATH.", "ERROR")
            sys.exit(1)
        else:
            log("VS Environment loaded.")

    bgfx_dir = ctx["paths"]["bgfx"]
    genie = ctx["tools"]["genie"]
    config = ctx["configuration"]
    
    # 1. Generate Solution
    log("Generating Visual Studio 2022 Solution...")
    run_command(f'"{genie}" --with-tools vs2022', cwd=bgfx_dir)

    sln_file = os.path.join(bgfx_dir, ".build", "projects", "vs2022", "bgfx.sln")
    
    msbuild_platform = "x64"
    if ctx["platform"].lower() == "win_x86":
        msbuild_platform = "x86"

    # 2. Build
    log(f"Building {config} Configuration...")
    cmd = (
        f'msbuild "{sln_file}" '
        f'/p:Configuration={config} '
        f'/p:Platform={msbuild_platform} '
        '/t:Build /v:minimal /maxcpucount'
    )
    run_command(cmd)

    log("\n[SUCCESS] Windows Build Complete.")
    bin_dir = os.path.join(bgfx_dir, ".build", "win64_vs2022", "bin")
    log(f"            Binaries in: {bin_dir}")

# ============================================================================
# 4. WEB (WASM) BUILD ROUTINES
# ============================================================================

def build_web_bgfx(ctx):
    log("\n>>> Building BGFX for Web...")
    log("-" * 50)
    
    bgfx_dir = ctx["paths"]["bgfx"]
    genie = ctx["tools"]["genie"]
    config = ctx["configuration"]

    gmake_wasm_dir = os.path.join(bgfx_dir, ".build", "projects", "gmake-wasm")
    wasm_build_dir = os.path.join(bgfx_dir, ".build", "wasm")
    
    # 1. Clean old build
    log("Generating BGFX Projects...")
    if os.path.exists(gmake_wasm_dir):
        shutil.rmtree(gmake_wasm_dir)
    if os.path.exists(wasm_build_dir):
        shutil.rmtree(wasm_build_dir)

    run_command(f'"{genie}" --gcc=wasm gmake', cwd=bgfx_dir)

    if not os.path.exists(gmake_wasm_dir):
        log(f"Project directory not found: {gmake_wasm_dir}", "ERROR")
        sys.exit(1)

    # 2. Patch Makefiles
    log("Patching BGFX Makefiles to enable WASM exceptions...")
    for root, dirs, files in os.walk(gmake_wasm_dir):
        for filename in files:
            if filename.endswith(".make"):
                filepath = os.path.join(root, filename)
                try:
                    with open(filepath, 'r', encoding='utf-8') as f:
                        content = f.read()
                    
                    if '-fno-exceptions' in content:
                        # FIX: Replace no-exceptions with wasm-exceptions
                        content = content.replace('-fno-exceptions', '-fwasm-exceptions')
                        with open(filepath, 'w', encoding='utf-8') as f:
                            f.write(content)
                except Exception as e:
                    log(f"Error patching {filepath}: {e}", "WARNING")

    # 3. Compile
    gmake_config = config.lower()
    log("Compiling BGFX with Threading & WASM Exceptions...")
    
    # Ensure -fwasm-exceptions is passed to both compiler and linker
    os.environ["CXXFLAGS"] = "-pthread -fwasm-exceptions"
    os.environ["CFLAGS"] = "-pthread -fwasm-exceptions"
    os.environ["LDFLAGS"] = "-pthread -fwasm-exceptions"

    num_cpus = multiprocessing.cpu_count()
    run_command(f"emmake make config={gmake_config} -j{num_cpus}", cwd=gmake_wasm_dir)

    # Cleanup Env Vars so they don't leak implicitly
    del os.environ["CXXFLAGS"]
    del os.environ["CFLAGS"]
    del os.environ["LDFLAGS"]
    
    log(">>> BGFX Build Complete.")

def build_web_tritonsim(ctx):
    log("\n>>> Building TritonSimRenderer for Web...")
    log("-" * 50)

    renderer_dir = ctx["paths"]["renderer"]
    config = ctx["configuration"]
    renderer_wasm_build_dir = os.path.join(renderer_dir, "build_wasm")

    # Clean and Recreate build dir
    if os.path.exists(renderer_wasm_build_dir):
        shutil.rmtree(renderer_wasm_build_dir)
    os.makedirs(renderer_wasm_build_dir)

    # Configure CMake
    # FIX: Explicitly passing -fwasm-exceptions to CMake
    cmake_flags = (
        f'-DCMAKE_BUILD_TYPE={config} '
        '-DCMAKE_CXX_FLAGS="-pthread -fwasm-exceptions" '
        '-DCMAKE_C_FLAGS="-pthread -fwasm-exceptions" '
        # For libraries, EXE_LINKER_FLAGS might be ignored, but good to have for consistency
        '-DCMAKE_EXE_LINKER_FLAGS="-pthread -fwasm-exceptions"'
    )

    # Use Unix Makefiles (standard for Emscripten)
    run_command(f'emcmake cmake .. -G "Unix Makefiles" {cmake_flags}', cwd=renderer_wasm_build_dir)
    
    # Build
    run_command("emmake make", cwd=renderer_wasm_build_dir)
    
    log(">>> TritonSimRenderer Build Complete.")

def build_web_tritonsimgui(ctx):
    log("\n>>> Publishing TritonSim.GUI.Browser...")
    log("-" * 50)

    # Check for dotnet
    if shutil.which("dotnet") is None:
        log("Dotnet SDK not found in PATH.", "ERROR")
        sys.exit(1)

    # 1. Define paths
    gui_dir = ctx["paths"]["gui"]
    browser_dir = ctx["paths"]["gui_browser"]
    config = ctx["configuration"]
    
    # 2. Cleanup old binaries
    dirs_to_clean = [
        os.path.join(gui_dir, "bin"),
        os.path.join(gui_dir, "obj"),
        os.path.join(browser_dir, "bin"),
        os.path.join(browser_dir, "obj"),
    ]

    log("Cleaning old binaries...")
    for d in dirs_to_clean:
        if os.path.exists(d):
            log(f"Deleting {d}")
            try:
                shutil.rmtree(d)
            except Exception as e:
                log(f"Failed to delete {d}: {e}", "WARNING")

    # 3. Locate Project
    csproj_path = os.path.join(browser_dir, "TritonSim.GUI.Browser.csproj")
    if not os.path.exists(csproj_path):
        log(f"Project file not found: {csproj_path}", "ERROR")
        sys.exit(1)

    log(f"Publishing {csproj_path} [{config}]...")
    
    # 4. Prepare SolutionDir logic to fix MSB4019
    solution_dir = ctx["src_root"]
    
    # Ensure it ends with a separator
    if not solution_dir.endswith(os.sep):
        solution_dir += os.sep

    # Handle Windows quoting quirk
    solution_dir_arg = solution_dir
    if os.name == 'nt' and solution_dir.endswith('\\'):
        solution_dir_arg += '\\'

    # 5. Run Publish (Using 'publish' instead of 'build')
    run_command(f'dotnet publish "{csproj_path}" -c {config} -p:SolutionDir="{solution_dir_arg}"')

    log(">>> TritonSim.GUI.Browser Publish Complete.")

def build_web(ctx, target="all"):
    """
    Orchestrates the WebAssembly build.
    target: 'all', 'bgfx', 'sim', 'gui'
    """
    log(f"\nStarting Web Assembly Build Sequence (Target: {target})...")
    log("=" * 75)

    if shutil.which("make") is None:
        log("GNU 'make' not found in PATH.", "ERROR")
        sys.exit(1)

    # 1. Setup Emscripten Environment (Always Required)
    emsdk_dir = ctx["paths"]["emsdk"]
    em_version = ctx["tools"]["em_version"]

    log(f"Activating Emscripten {em_version}...")
    
    emsdk_bat = os.path.join(emsdk_dir, "emsdk.bat")
    env_bat = os.path.join(emsdk_dir, "emsdk_env.bat")

    if not os.path.exists(emsdk_bat):
        log(f"EMSDK not found at {emsdk_bat}", "ERROR")
        sys.exit(1)

    run_command(f'"{emsdk_bat}" install {em_version}', cwd=emsdk_dir)
    run_command(f'"{emsdk_bat}" activate {em_version}', cwd=emsdk_dir)
    
    if not load_env_from_bat(env_bat):
        log("Failed to load Emscripten environment.", "ERROR")
        sys.exit(1)

    # Force check EMSCRIPTEN var
    if "EMSCRIPTEN" not in os.environ:
        forced_path = os.path.join(emsdk_dir, "upstream", "emscripten")
        os.environ["EMSCRIPTEN"] = forced_path
        log(f"Force-setting EMSCRIPTEN={forced_path}")

    # 2. Dispatch based on sub-target
    if target == "all":
        build_web_bgfx(ctx)
        build_web_tritonsim(ctx)
        build_web_tritonsimgui(ctx)
    elif target == "bgfx":
        build_web_bgfx(ctx)
    elif target == "sim":
        build_web_tritonsim(ctx)
    elif target == "gui":
        build_web_tritonsimgui(ctx)
    else:
        log(f"Unknown web sub-target: {target}", "ERROR")
        sys.exit(1)

    log("\n[SUCCESS] Web Build Complete.")

# ============================================================================
# 5. MAIN ENTRY POINT
# ============================================================================
def main():
    # 1. Create Context (Parses args and loads config)
    ctx = create_build_context()
    
    # 2. Dispatch based on platform
    platform = ctx["platform"].lower()
    
    # Map friendly names to internal target logic
    if platform == "web":
        build_web(ctx, target="all")
    elif platform == "web_bgfx":
        build_web(ctx, target="bgfx")
    elif platform == "build_web_tritonsim": # Strict match as requested
        build_web(ctx, target="sim")
    elif platform == "build_web_tritonsimgui": # Strict match as requested
        build_web(ctx, target="gui")
        
    elif platform in ["win_x64", "win_x86"]:
        build_win(ctx)
    else:
        log(f"Unknown Platform: {platform}", "ERROR")
        sys.exit(1)

if __name__ == "__main__":
    main()