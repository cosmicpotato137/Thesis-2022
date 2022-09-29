# pragma once

# include <string>
# include <map>
# include "parser.tab.hpp"

// Then comes the declaration of the scanning function.Flex expects the signature of yylex to be defined in the macro YY_DECL, and the C++ parser expects it to be declared.We can factor both as follows.
// Give Flex the prototype of yylex we want ...
# define YY_DECL \
  yy::parser::symbol_type yylex (driver& drv)
// ... and declare it for the parser's sake.
YY_DECL;

// The driver class is then declared with its most obvious members.
// Conducting the whole scanning and parsing of Calc++.
class driver
{
public:
	driver()
		: trace_parsing(false), trace_scanning(false), result(0)
	{
		variables["one"] = 1;
		variables["two"] = 2;
	}

	std::map<std::string, int> variables;

	int result;

	// The main routine is of course calling the parser.
	// Run the parser on file F.  Return 0 on success.
	int parse(const std::string& f, const std::string& l = "");
	// The name of the file being parsed.
	std::string file;
	// Whether to generate parser debug traces.
	bool trace_parsing;

	// To encapsulate the coordination with the Flex scanner, it is useful to have member functions to openand close the scanning phase.
	// Handling the scanner.
	void scan_begin();
	void scan_end();
	// Whether to generate scanner debug traces.
	bool trace_scanning;
	// The token's location used by the scanner.
	yy::location location;
};