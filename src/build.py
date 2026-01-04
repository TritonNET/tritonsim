import os
import sys
import json
import argparse
import shutil

from build_utils import log, run_command, load_env_from_bat
import build_web
import build_win

def create_build_context():
    parser = argparse.ArgumentParser(description="Build Script")
    parser.add_argument("-Config", default="build.json", help="Path to JSON configuration file")
    parser.add_argument("-Configuration", default="Release", help="Build Configuration (Debug/Release)")
    parser.add_argument("-Platform", default="win_x64", help="Target Platform (win_x64/web)")
    parser.add_argument("-Target", default="all", help="Build Targets \n all or \n\t bgfx, sim, gui, host, docker \n or combination of them in any order seperated by underscore (ie: gui_docker). They will be executed in the correct order that they should be executed.")

    args = parser.parse_args()
    
    src_root = os.path.dirname(os.path.abspath(__file__))
    config_file_path = args.Config
    
    if not os.path.isabs(config_file_path):
        config_file_path = os.path.join(src_root, config_file_path)

    if not os.path.exists(config_file_path):
        log(f"Configuration file not found: {config_file_path}", "ERROR")
        sys.exit(1)

    try:
        with open(config_file_path, 'r') as f:
            config_data = json.load(f)
    except json.JSONDecodeError as e:
        log(f"Error parsing JSON config: {e}", "ERROR")
        sys.exit(1)

    try:
        thirdparty_dir = os.path.join(src_root, config_data["directories"]["thirdparty"])
        bx_dir = os.path.join(thirdparty_dir, "bx")
        
        ctx = {
            "platform": args.Platform,
            "target": args.Target,
            "configuration": args.Configuration,
            "src_root": src_root,
            "src_paths": {
                "thirdparty": thirdparty_dir,
                "renderer": os.path.join(src_root, config_data["directories"]["renderer"]),
                "bgfx": os.path.join(thirdparty_dir, "bgfx"),
                "bx": bx_dir,
                "emsdk": os.path.join(thirdparty_dir, "emsdk"),
                "gui": os.path.join(src_root, "TritonSim.GUI"),
                "gui_browser": os.path.join(src_root, "TritonSim.GUI.Browser"),
                "gui_browser_host": os.path.join(src_root, "TritonSim.GUI.Browser.Host"),
            },
            "output_paths":{
                "win": config_data["directories"]["output"][f"win_{args.Configuration.lower()}"],
			    "linux": config_data["directories"]["output"][f"linux_{args.Configuration.lower()}"],
			    "web": config_data["directories"]["output"][f"web_{args.Configuration.lower()}"],
                "android": config_data["directories"]["output"][f"android_{args.Configuration.lower()}"],
            },
            "tools": {
                "genie": os.path.join(bx_dir, "tools", "bin", "windows", "genie.exe"),
                "em_version": config_data["tools"]["em_version"],
                "vs_vars_path": config_data["tools"]["vs_vars_path"],
                "vs_version": config_data["tools"]["vs_version"],
            },
            "docker": {
                "image_name": config_data["docker"]["image_name"],
                "tag": config_data["docker"][f"tag_{args.Configuration.lower()}"],
            }
        }
    except KeyError as e:
        log(f"Missing key in configuration file: {e}", "ERROR")
        sys.exit(1)

    log(f"Platform: {ctx['platform']}")
    log(f"Target:        {ctx['target']}")
    log(f"Configuration:   {ctx['configuration']}")
    log(f"Loaded Config:   {config_file_path}")

    return ctx

def main():
    ctx = create_build_context()
    
    platform = ctx["platform"].lower()
    
    if platform == "web":
        build_web.run_build_web(ctx)     
    elif platform in ["win_x64", "win_x86"]:
        build_win.run_build_win(ctx)
    else:
        log(f"Unknown Platform: {platform}", "ERROR")
        sys.exit(1)

if __name__ == "__main__":
    main()