project(turtle-grammars)
cmake_minimum_required(VERSION 2.8.11)

if (WIN32) # Include win64 platforms

  set(INCLUDE_DIRS
	external/include
	src)

  set(CMAKE_CXX_STANDARD 11)
  set(CMAKE_CXX_FLAGS 
     "/wd4018 /wd4244 /wd4305 
     /D_CRT_SECURE_NO_WARNINGS 
     /D_CRT_NONSTDC_NO_DEPRECATE 
     /D NOMINMAX /DGLEW_STATIC
     /EHsc")

  set(VS_DEBUGGER_WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/bin)
  set(CMAKE_EXE_LINKER_FLAGS "/NODEFAULTLIB:\"MSVCRT\" /NODEFAULTLIB:\"LIBCMT\"")

  set(EXECUTABLE_OUTPUT_PATH ${CMAKE_SOURCE_DIR}/bin)

elseif (APPLE)

  set(CMAKE_MACOSX_RPATH 1)
  set(CMAKE_CXX_FLAGS "-Wall -Wno-reorder -Wno-deprecated-declarations -g -stdlib=libc++ -std=c++14")
  set(EXECUTABLE_OUTPUT_PATH ${CMAKE_SOURCE_DIR}/bin)
  add_definitions(-DAPPLE)

  set(INCLUDE_DIRS
	/System/Library/Frameworks
	external/include
	/usr/local/include
	src)

elseif (UNIX)

  set(CMAKE_CXX_FLAGS "-Wall -g -std=c++11 -Wno-comment")
  set(EXECUTABLE_OUTPUT_PATH ${CMAKE_SOURCE_DIR}/bin)

  set(INCLUDE_DIRS
	external/include
	src)

  add_definitions(-DUNIX)

endif()

include_directories(${INCLUDE_DIRS})
link_directories(${LIBRARY_DIRS})


set(SOURCES
	src/ppm_image.cpp
	src/ppm_image.h
	src/turtle_grammar.cpp
	src/turtle_grammar.h)

add_executable(turtle-grammars src/main.cpp ${SOURCES})

