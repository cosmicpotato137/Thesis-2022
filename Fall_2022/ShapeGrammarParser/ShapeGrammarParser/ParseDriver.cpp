#include "ParseDriver.h"
#include "parser.tab.hpp"
#include <fstream>

//The parse member function deserves some attention.
int driver::parse(const std::string& f, const std::string& l)
{

	file = f;
	location.initialize(&file);
	scan_begin();
	
	yy::parser parse(*this);

	std::ofstream debug_stream;
	debug_stream.open(l);
	parse.set_debug_stream(debug_stream);
	parse.set_debug_level(trace_parsing);
	int res = parse();
	scan_end();

	return res;
}