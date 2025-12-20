cmake_minimum_required(VERSION 3.15)
project(TritonSimRenderer)

# --- 0. PRE-CONFIGURATION (Find Python) ---------------------------------------
find_package(Python3 REQUIRED COMPONENTS Interpreter)

# --- 1. CONFIGURATION ---------------------------------------------------------

# Set C++ Standard
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# --- CODE GENERATION STEP ---
set(COMMON_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../Common")
set(GEN_SCRIPT "${COMMON_DIR}/enum_compile.py")
set(ENUMS_JSON "${COMMON_DIR}/enums.json")
set(GENERATED_HEADER "${CMAKE_CURRENT_SOURCE_DIR}/ResponseCode.h")

add_custom_command(
    OUTPUT "${GENERATED_HEADER}"
    COMMAND "${Python3_EXECUTABLE}" "enum_compile.py"
    WORKING_DIRECTORY "${COMMON_DIR}"
    DEPENDS "${GEN_SCRIPT}" "${ENUMS_JSON}"
    COMMENT "Auto-generating Response Codes from JSON..."
    VERBATIM
)

# Define Source Files 
file(GLOB SOURCES 
    "*.cpp" "*.h" 
    "../Common/*.cpp" "../Common/*.h"
)
list(APPEND SOURCES "${GENERATED_HEADER}")

# Define Include Directories
set(INCLUDE_DIRS
    .
    ../Common
    ../3rdparty/bgfx/include
    ../3rdparty/bx/include
    ../3rdparty/bimg/include
    ../3rdparty/glm
    ../3rdparty/json/include
)

# --- 2. PLATFORM LOGIC --------------------------------------------------------

if(EMSCRIPTEN)
    # --- WEB (WebAssembly) ---
    message(STATUS "Configuring TritonSimRenderer for WebAssembly (Threading & Exceptions Enabled)")
    
    # Web usually prefers Static libraries to link into the final .wasm
    add_library(TritonSimRenderer STATIC ${SOURCES})

    # Emscripten specific defines
    target_compile_definitions(TritonSimRenderer PRIVATE 
        __EMSCRIPTEN__ 
        BX_CONFIG_DEBUG=0
    )

    # B. Compile Options
    # FIX: Added -fwasm-exceptions and ensured -pthread is present
    target_compile_options(TritonSimRenderer PRIVATE 
        -gsource-map
        -Os 
        -Wall 
        -Wextra 
        -Werror=return-type
        -pthread
        -fwasm-exceptions
    )

    # C. Link Options
    # Ensure static library objects are created with these flags too
    target_link_options(TritonSimRenderer PUBLIC
                -pthread
        -fwasm-exceptions
        
        # Canvas and display settings
        "SHELL:-s DISABLE_DEPRECATED_FIND_EVENT_TARGET_BEHAVIOR=0"
        "SHELL:-s USE_WEBGL2=1"
        "SHELL:-s FULL_ES3=1"
        "SHELL:-s MIN_WEBGL_VERSION=2"
        
        # Allow BGFX to find canvas
        "SHELL:-s DEFAULT_LIBRARY_FUNCS_TO_INCLUDE=['$Browser','$HTML5']"
        
        # Export functions for testing
        "SHELL:-s EXPORTED_FUNCTIONS=['_main','_test_webgl_availability','_get_canvas_width']"
        "SHELL:-s EXPORTED_RUNTIME_METHODS=['ccall','cwrap','UTF8ToString']"
        
        # Memory settings
        "SHELL:-s ALLOW_MEMORY_GROWTH=1"
        "SHELL:-s INITIAL_MEMORY=67108864"  # 64MB initial
        
        # Debugging
        "SHELL:-s GL_DEBUG=1"
        "SHELL:-s ASSERTIONS=2"
    )

    # Libraries to link
    set(PLATFORM_LIBS 
        "${BGFX_LIB_PATH}/libbgfxRelease.a"
        "${BGFX_LIB_PATH}/libbxRelease.a"
        "${BGFX_LIB_PATH}/libbimgRelease.a"
    )

elseif(ANDROID)
    # --- ANDROID ---
    message(STATUS "Configuring for Android")
    
    add_library(TritonSimRenderer SHARED ${SOURCES})
    
    target_compile_definitions(TritonSimRenderer PRIVATE 
        ANDROID 
        __ANDROID__
    )
    
    set(BGFX_LIB_PATH "${CMAKE_SOURCE_DIR}/../3rdparty/bgfx/.build/android-arm64/bin")

    set(PLATFORM_LIBS
        "${BGFX_LIB_PATH}/libbgfxRelease.a"
        "${BGFX_LIB_PATH}/libbxRelease.a"
        "${BGFX_LIB_PATH}/libbimgRelease.a"
        EGL GLESv2 android log
    )

elseif(WIN32)
    # --- WINDOWS ---
    message(STATUS "Configuring for Windows")
    
    add_library(TritonSimRenderer SHARED ${SOURCES} "TritonSimRenderer.rc")
    
    target_compile_definitions(TritonSimRenderer PRIVATE 
        WINDOWS 
        WIN32 
        _WINDOWS 
        TRITONSIMRENDERER_EXPORTS 
        NOMINMAX
        BX_CONFIG_DEBUG=0
    )
    
    target_compile_options(TritonSimRenderer PRIVATE "/Zc:__cplusplus" "/Zc:preprocessor")

    set_property(TARGET TritonSimRenderer PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

    set(BGFX_LIB_PATH "${CMAKE_SOURCE_DIR}/../3rdparty/bgfx/.build/win64_vs2022/bin")
    
    set(PLATFORM_LIBS
        debug "${BGFX_LIB_PATH}/bgfxDebug.lib"
        optimized "${BGFX_LIB_PATH}/bgfxRelease.lib"
        
        debug "${BGFX_LIB_PATH}/bxDebug.lib"
        optimized "${BGFX_LIB_PATH}/bxRelease.lib"
        
        debug "${BGFX_LIB_PATH}/bimgDebug.lib"
        optimized "${BGFX_LIB_PATH}/bimgRelease.lib"
        
        d3d11.lib dxgi.lib user32.lib gdi32.lib psapi.lib
    )

elseif(UNIX AND NOT APPLE)
    # --- LINUX ---
    message(STATUS "Configuring for Linux")
    
    add_library(TritonSimRenderer SHARED ${SOURCES})
    
    target_compile_definitions(TritonSimRenderer PRIVATE LINUX __LINUX__)
    
    set(BGFX_LIB_PATH "${CMAKE_SOURCE_DIR}/../3rdparty/bgfx/.build/linux64_gcc/bin")
    
    set(PLATFORM_LIBS
        "${BGFX_LIB_PATH}/libbgfxRelease.a"
        "${BGFX_LIB_PATH}/libbxRelease.a"
        "${BGFX_LIB_PATH}/libbimgRelease.a"
        GL X11 dl pthread
    )
endif()

target_include_directories(TritonSimRenderer PRIVATE ${INCLUDE_DIRS})
target_link_libraries(TritonSimRenderer PRIVATE ${PLATFORM_LIBS})

# Output directory settings
set_target_properties(TritonSimRenderer PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)