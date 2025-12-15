#!/bin/bash

echo "Generating Enums..."

# Try python3 first, fallback to python
if command -v python3 &> /dev/null; then
    python3 gen_enums.py
else
    python gen_enums.py
fi

# Check exit code
if [ $? -eq 0 ]; then
    echo "[SUCCESS] Code generation complete."
else
    echo "[ERROR] Generation failed."
    exit 1
fi