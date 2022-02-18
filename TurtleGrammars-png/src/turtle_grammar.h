#pragma once
#include <string>
#include <functional>
#include <map>
#include "ppm_image.h"
#include "log.h"
#include "glm/gtc/constants.hpp"

struct Turtle
{
	double rotation;
	glm::vec2 position;
};

struct TurtleGrammarRule
{
	char match;
	std::string replace;
	std::function<void(Turtle&, Image&)> produce = [](Turtle& t, Image& i) {}; // move the turtle
};

class TurtleGrammar
{
public:
	TurtleGrammar() = default;
	~TurtleGrammar();
	
	std::string setStart(const std::string& start) 
	{ 
		m_Start = start; 
		m_Current = start; 
		return start; 
	}
	std::string getStart() { return m_Start; }
	std::string getCurrent() { return m_Current; }

	void setTurtle(const Turtle& t) { m_Turtle = t; }

	bool addRule(TurtleGrammarRule* rule);
	TurtleGrammarRule* getRule(char match);

	std::string generate(int n); // generate complex grammar
	void produce(Image& img); // write grammar to an image
private:
	std::map<char, TurtleGrammarRule*> m_Rules;
	Turtle m_Turtle;
	std::string m_Start;
	std::string m_Current;

	float m_Theta = glm::pi<float>() / 2.0f;
	float m_Scale = 6.0f;
};