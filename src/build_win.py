import os
import sys
import shutil
import multiprocessing
from build_utils import log, run_command, load_env_from_bat

def run_build_win(ctx):
    log(f"\nStarting Windows Build ({ctx['platform']})...")
    log("-" * 75)

    if shutil.which("nmake") is None:
        log("MSVC tools not found in PATH. Attempting to load vcvars64.bat...")
        if not load_env_from_bat(ctx["tools"]["vs_vars_path"]):
            log("Could not find vcvars64.bat and tools are not in PATH.", "ERROR")
            sys.exit(1)
        else:
            log("VS Environment loaded.")

    bgfx_dir = ctx["src_paths"]["bgfx"]
    genie = ctx["tools"]["genie"]
    config = ctx["configuration"]
    
    vs_version = ctx["tools"]["vs_version"]
    
    log(f"Generating Visual Studio {vs_version} Solution...")
    run_command(f'"{genie}" --with-tools vs{vs_version}', cwd=bgfx_dir)

    sln_ext = "slnx" if vs_version == "2026" else "sln"
    sln_file = os.path.join(bgfx_dir, ".build", "projects", f"vs{vs_version}", f"bgfx.{sln_ext}")
    
    msbuild_platform = "x64"
    if ctx["platform"].lower() == "win_x86":
        msbuild_platform = "x86"

    log(f"Building {config} Configuration...")
    cmd = (
        f'msbuild "{sln_file}" '
        f'/p:Configuration={config} '
        f'/p:Platform={msbuild_platform} '
        '/t:Build /v:minimal /maxcpucount'
    )
    run_command(cmd)

    log("\n[SUCCESS] Windows Build Complete.")
    bin_dir = os.path.join(bgfx_dir, ".build", f"win64_vs{vs_version}", "bin")
    log(f"            Binaries in: {bin_dir}")