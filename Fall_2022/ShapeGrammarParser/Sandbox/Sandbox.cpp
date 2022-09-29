// Sandbox.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <string>
#include <fstream>
#include "ShapeGrammarParser.h"

int main(int argc, char* argv[])
{
    std::cout << "starting" << std::endl;
    Parse("C:/NerdThings/thesis/Fall_2022/ShapeGrammarParser/Sandbox/bin/x64/Release/math.txt",
        "parseLog.txt");

    //for (int i = 1; i < argc; ++i)
    //    if (argv[i] == std::string("-p"))
    //        drv.trace_parsing = true;
    //    else if (argv[i] == std::string("-s"))
    //        drv.trace_scanning = true;
    //    else if (!drv.parse(argv[i]))
    //        std::cout << drv.result << '\n';
    //    else
    //        res = 1;
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
