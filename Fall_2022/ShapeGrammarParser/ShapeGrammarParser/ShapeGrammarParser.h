#pragma once
#include <string>

#define DLLExport __declspec(dllexport)

extern "C"
{
	DLLExport int Parse(const std::string& file, const std::string& logFile);
	DLLExport int add(int a, int b);
	DLLExport int str(const std::string& str);
}