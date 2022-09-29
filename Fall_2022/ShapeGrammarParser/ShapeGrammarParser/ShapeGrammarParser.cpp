#include "ShapeGrammarParser.h"
#include "ParseDriver.h"

DLLExport int Parse(const std::string& file, const std::string& logFile)
{
	driver drv;
	//drv.trace_scanning = true;
	return drv.parse(file, logFile);
}

int asdf(int a, int b) {
	return a + b;
}

DLLExport int add(int a, int b) 
{
	return asdf(a, b);
}

DLLExport int str(const std::string& str)
{
	return 1;
}
