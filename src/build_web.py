import os
import sys
import shutil
import multiprocessing
from build_utils import log, run_command, load_env_from_bat

def build_web_bgfx(ctx):
    log("\n>>> Building BGFX for Web...")
    log("-" * 50)
    
    bgfx_dir = ctx["src_paths"]["bgfx"]
    genie = ctx["tools"]["genie"]
    config = ctx["configuration"]

    gmake_wasm_dir = os.path.join(bgfx_dir, ".build", "projects", "gmake-wasm")
    wasm_build_dir = os.path.join(bgfx_dir, ".build", "wasm")
    
    log("Generating BGFX Projects...")
    if os.path.exists(gmake_wasm_dir):
        shutil.rmtree(gmake_wasm_dir)
    if os.path.exists(wasm_build_dir):
        shutil.rmtree(wasm_build_dir)

    run_command(f'"{genie}" --gcc=wasm gmake', cwd=bgfx_dir)

    if not os.path.exists(gmake_wasm_dir):
        log(f"Project directory not found: {gmake_wasm_dir}", "ERROR")
        sys.exit(1)

    gmake_config = config.lower()
    log("Compiling BGFX with Threading & WASM Exceptions...")
    
    os.environ["CXXFLAGS"] = "-pthread"
    os.environ["CFLAGS"] = "-pthread"
    os.environ["LDFLAGS"] = "-pthread"

    num_cpus = multiprocessing.cpu_count()
    run_command(f"emmake make config={gmake_config} -j{num_cpus}", cwd=gmake_wasm_dir)

    del os.environ["CXXFLAGS"]
    del os.environ["CFLAGS"]
    del os.environ["LDFLAGS"]
    
    log(">>> BGFX Build Complete.")

def build_web_tritonsim(ctx):
    log("\n>>> Building TritonSimRenderer for Web...")
    log("-" * 50)

    renderer_dir = ctx["src_paths"]["renderer"]
    config = ctx["configuration"]
    renderer_wasm_build_dir = os.path.join(renderer_dir, "build_wasm")

    if os.path.exists(renderer_wasm_build_dir):
        shutil.rmtree(renderer_wasm_build_dir)
    os.makedirs(renderer_wasm_build_dir)

    cmake_flags = (
        f'-DCMAKE_BUILD_TYPE={config} '
        '-DCMAKE_CXX_FLAGS="-pthread" '
        '-DCMAKE_C_FLAGS="-pthread" '
        '-DCMAKE_EXE_LINKER_FLAGS="-pthread"'
    )

    run_command(f'emcmake cmake .. -G "Unix Makefiles" {cmake_flags}', cwd=renderer_wasm_build_dir)
    run_command("emmake make", cwd=renderer_wasm_build_dir)
    
    log(">>> TritonSimRenderer Build Complete.")

def build_web_tritonsimgui(ctx):
    log("\n>>> Publishing TritonSim.GUI.Browser...")
    log("-" * 50)

    if shutil.which("dotnet") is None:
        log("Dotnet SDK not found in PATH.", "ERROR")
        sys.exit(1)

    gui_dir = ctx["src_paths"]["gui"]
    browser_dir = ctx["src_paths"]["gui_browser"]
    config = ctx["configuration"]
    output_dir = os.path.join(ctx["src_root"], ctx["output_paths"]["web"])

    dirs_to_clean = [
        os.path.join(gui_dir, "bin"),
        os.path.join(gui_dir, "obj"),
        os.path.join(browser_dir, "bin"),
        os.path.join(browser_dir, "obj"),
        output_dir,
    ]

    log("Cleaning old binaries...")
    for d in dirs_to_clean:
        if os.path.exists(d):
            log(f"Deleting {d}")
            try:
                shutil.rmtree(d)
            except Exception as e:
                log(f"Failed to delete {d}: {e}", "WARNING")

    csproj_path = os.path.join(browser_dir, "TritonSim.GUI.Browser.csproj")
    if not os.path.exists(csproj_path):
        log(f"Project file not found: {csproj_path}", "ERROR")
        sys.exit(1)

    log(f"Publishing {csproj_path} [{config}]...")
    
    solution_dir = ctx["src_root"]
    if not solution_dir.endswith(os.sep):
        solution_dir += os.sep

    solution_dir_arg = solution_dir
    if os.name == 'nt' and solution_dir.endswith('\\'):
        solution_dir_arg += '\\'

    run_command(f'dotnet publish "{csproj_path}" -c {config} -p:SolutionDir="{solution_dir_arg}" -o "{output_dir}"')
    
    host_project_dir = os.path.join(browser_dir, "..", "TritonSim.GUI.Browser.Host")
    host_csproj = os.path.join(host_project_dir, "TritonSim.GUI.Browser.Host.csproj")
    
    if os.path.exists(host_csproj):
        log(f"Publishing Host: {host_csproj}...")
        run_command(f'dotnet publish "{host_csproj}" -c {config}')
    else:
        log(f"Host project not found at {host_csproj}. Docker build may fail.", "WARNING")

    log(">>> TritonSim.GUI.Browser Publish Complete.")

def build_web_tritonsimguihost(ctx):
    log("\n>>> Building TritonSim.GUI.Browser.Host...")
    log("-" * 50)
    if shutil.which("dotnet") is None:
        log("Dotnet SDK not found in PATH.", "ERROR")
        sys.exit(1)

    host_project_dir = ctx["src_paths"]["gui_browser_host"]

    host_csproj = os.path.join(host_project_dir, "TritonSim.GUI.Browser.Host.csproj")

    output_dir = os.path.join(ctx["src_root"], ctx["output_paths"]["web"])

    config = ctx["configuration"]
    if not os.path.exists(host_csproj):
        log(f"Host project not found at {host_csproj}.", "ERROR")
        sys.exit(1)

    log(f"Building Host: {host_csproj} [{config}]...")

    run_command(f'dotnet publish "{host_csproj}" -c {config} -o {output_dir}')
    
    log(">>> TritonSim.GUI.Browser.Host Build Complete.")

def build_web_docker(ctx):
    log("\n>>> Building Docker Image...")
    log("-" * 50)

    if shutil.which("docker") is None:
        log("Docker not found in PATH. Skipping docker build.", "WARNING")
        return

    config = ctx["configuration"]
    src_root = ctx["src_root"]
    
    artifact_path = os.path.join(src_root, ctx["output_paths"]["web"])
    
    if not os.path.exists(artifact_path):
        log(f"Publish directory not found: {artifact_path}", "ERROR")
        log("Did the build step fail?", "ERROR")
        sys.exit(1)

    dockerfile_path = os.path.join(src_root, "Dockerfile")

    shutil.copy2(dockerfile_path, artifact_path)

    image_tag = ctx["docker"]["image_name"] + ":" + ctx["docker"]["tag"]
    log(f"Building Docker Image: {image_tag}")
    
    run_command(f"docker build -t {image_tag} .", cwd=artifact_path)
    
    log(f"[SUCCESS] Docker Image '{image_tag}' built successfully.")

def run_build_web(ctx):
    """
    Orchestrates the WebAssembly build.
    target: 'all', 'bgfx', 'sim', 'gui'
    """
    target = ctx["target"]
    log(f"\nStarting Web Assembly Build Sequence (Target: {target})...")
    log("=" * 75)

    if shutil.which("make") is None:
        log("GNU 'make' not found in PATH.", "ERROR")
        sys.exit(1)

    emsdk_dir = ctx["src_paths"]["emsdk"]
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

    if "EMSCRIPTEN" not in os.environ:
        forced_path = os.path.join(emsdk_dir, "upstream", "emscripten")
        os.environ["EMSCRIPTEN"] = forced_path
        log(f"Force-setting EMSCRIPTEN={forced_path}")

    build_steps = [
        ("bgfx",    build_web_bgfx),
        ("sim",     build_web_tritonsim),
        ("gui",     build_web_tritonsimgui),
        ("host",    build_web_tritonsimguihost),
        ("docker",  build_web_docker),
    ]

    requested_targets = set(target.split('_'))

    for name, func in build_steps:
        if target == "all" or name in requested_targets:
            print(f"Executing build step: {name}")
            func(ctx)

    log("\n[SUCCESS] Web Build Complete.")