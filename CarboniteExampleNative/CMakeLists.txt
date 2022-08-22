cmake_minimum_required(VERSION 3.10)

project(CarboniteExampleNative)

add_executable(CarboniteExampleNative Main.cpp)

target_include_directories(CarboniteExampleNative PRIVATE ../Include)

target_compile_definitions(CarboniteExampleNative PRIVATE _CRT_SECURE_NO_WARNINGS)

# For Visual Studio to pick up the natvis file, it need to be in the CMakeLists.txt directory or a subdirectory
# (our gitignore ignores the copy of the file)
file(COPY ../carbonite.natvis DESTINATION ${CMAKE_CURRENT_SOURCE_DIR})
