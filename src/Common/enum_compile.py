import json
import os
import datetime
from types import SimpleNamespace
from collections import defaultdict

JSON_FILE = 'enums.json'

def to_pascal_case(screaming_snake):
    clean = screaming_snake[3:] if screaming_snake.startswith("RC_") else screaming_snake
    parts = clean.split('_')
    return "".join(word.capitalize() for word in parts)

def calculate_hresult(severity_str, facility_id, code):
    # Severity: "Fail" = 1, "Success" = 0
    is_fail = 1 if severity_str.lower() == "fail" else 0
    
    val = 0
    if is_fail:
        val |= (1 << 31) # Bit 31: Severity
    
    val |= (facility_id << 16) # Bit 16-26: Facility
    val |= (code & 0xFFFF)     # Bit 0-15: Code (Max 65535)
    
    return val & 0xFFFFFFFF

def format_hex(int_val):
    return f"0x{int_val:08X}"

def generate():
    if not os.path.exists(JSON_FILE):
        print(f"Error: {JSON_FILE} not found.")
        return

    gen_time = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    with open(JSON_FILE, 'r') as f:
        # Load raw dict for facilities map, SimpleNamespace for recursive access
        raw_data = json.load(f)
        # Re-load for object access to enums list (convenience)
        f.seek(0)
        data = json.load(f, object_hook=lambda d: SimpleNamespace(**d))

    facilities = raw_data['facilities']
    
    # Counter dictionary: stores the next available code for each "Type"
    # e.g., type_counters["IO_Fail"] = 0
    type_counters = defaultdict(int)

    # Prepare lists for output
    cpp_lines = []
    cs_lines = []

    for item in data.enums:
        # 1. Parse the Type string "Facility_Severity"
        try:
            fac_name, sev_str = item.type.split('_')
        except ValueError:
            print(f"Error: Type '{item.type}' must be in format 'Facility_Severity' (e.g. IO_Fail)")
            continue

        if fac_name not in facilities:
            print(f"Error: Facility '{fac_name}' not defined in facilities list.")
            continue

        # 2. Get IDs and Code
        fac_id = facilities[fac_name]
        
        # Get the next code for this specific type and increment it
        current_code = type_counters[item.type]
        type_counters[item.type] += 1

        # 3. Calculate Value
        hresult_val = calculate_hresult(sev_str, fac_id, current_code)
        hex_str = format_hex(hresult_val)
        
        # 4. Generate C++ Line
        if sev_str.lower() == "fail":
            cpp_lines.append(f"    {item.name} = static_cast<int32_t>({hex_str}), // Code: {current_code}")
        else:
            cpp_lines.append(f"    {item.name} = {hex_str}, // Code: {current_code}")

        # 5. Generate C# Line
        cs_name = to_pascal_case(item.name)
        cs_lines.append(f"    [Description(\"{item.desc}\")]")
        if sev_str.lower() == "fail":
            cs_lines.append(f"    {cs_name} = unchecked((int){hex_str}),\n")
        else:
            cs_lines.append(f"    {cs_name} = {hex_str},\n")

    # --- Write C++ ---
    with open(data.output_cpp, 'w') as f:
        f.write("#pragma once\n#include <cstdint>\n\n")
        f.write(f"// AUTO-GENERATED FROM enums.json at {gen_time}\n")
        f.write("// HRESULT: [Sev:1][Res:5][Fac:11][Code:15]\n")
        f.write("enum ResponseCode : int32_t\n{\n")
        f.write("\n".join(cpp_lines))
        f.write("\n};\n\n")
        f.write("#define RC_IS_SUCCESS(x) ((x) >= 0)\n")
        f.write("#define RC_IS_FAILURE(x) ((x) < 0)\n")
        print(f"Generated {data.output_cpp}")

    # --- Write C# ---
    with open(data.output_cs, 'w') as f:
        f.write("using System.ComponentModel;\n\n")
        f.write("namespace TritonSim.GUI.Infrastructure;\n\n")
        f.write(f"// AUTO-GENERATED FROM enums.json at {gen_time}\n")
        f.write("public enum ResponseCode : int\n{\n")
        f.write("\n".join(cs_lines))
        f.write("}\n")
    print(f"Generated {data.output_cs}")

if __name__ == "__main__":
    generate()