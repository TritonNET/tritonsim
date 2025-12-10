#pragma once

#include <iostream>
#include <fstream>
#include <filesystem>
#include <vector>
#include <map>
#include <regex>

#define NOMINMAX
#include <windows.h>

#include <nlohmann/json.hpp>

#include <bx/commandline.h>
#include "bgfx/bgfx.h"
#include <bx/bx.h>
#include <bx/file.h>
#include <bx/string.h>

#include "utils.h"

#include "shaderc.h"

#include <shader_def.h>

#include <shader_types.h>
#include <shader_packer.h>

#include "shader_compiler.h"

