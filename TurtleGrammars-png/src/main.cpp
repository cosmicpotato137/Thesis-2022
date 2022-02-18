#include "turtle_grammar.h"
#include "ppm_image.h"
#include "glm/glm.hpp"
#include "glm/gtc/constants.hpp"
#include <iostream>

typedef Pixel Pixel;
typedef Image Image;
using namespace glm;

int main() 
{
	//image testing
	int imgSize = 800;
	Image i = Image(imgSize, imgSize);
	//std::vector<glm::vec2> v = { vec2(0, 0), vec2(25, 25), vec2(0,  50) };
	//draw_lines(v, i);

	// start production rule
	std::string start = "I";
	TurtleGrammarRule* in = new TurtleGrammarRule{ 'I', "F-F-F-F" };
	// production rule for Koch island!
	//TurtleGrammarRule* F = new TurtleGrammarRule{ 'F', "F-F+F+FF-F-F+F" };
	// different variation
	TurtleGrammarRule* F = new TurtleGrammarRule{ 'F', "FF-F--F-F" };
	F->produce = [](Turtle& t, Image& i) {
		std::vector<vec2> verts;
		verts.push_back(t.position);
		t.position += vec2(cos(t.rotation), sin(t.rotation)) * 3.0f;
		verts.push_back(t.position);
		draw_lines(verts, i);
	};
	TurtleGrammarRule* f = new TurtleGrammarRule{ 'f', "f" };
	f->produce = [](Turtle& t, Image& i) {
		t.position += vec2(cos(t.rotation), sin(t.rotation)) * 3.0f;
	};
	TurtleGrammarRule* p = new TurtleGrammarRule{ '+', "+" };
	p->produce = [](Turtle& t, Image& i) {
		t.rotation += pi<double>() / 2.0;
	};
	TurtleGrammarRule* m = new TurtleGrammarRule{ '-', "-" };
	m->produce = [](Turtle& t, Image& i) {
		t.rotation -= pi<double>() / 2.0;
	};
	
	// setting up the grammar
	TurtleGrammar tg;
	tg.setStart(start);
	tg.setTurtle({ 0.0f, vec2(10, imgSize-10) });
	tg.addRule(in);
	tg.addRule(F);
	tg.addRule(f);
	tg.addRule(p);
	tg.addRule(m);
	LOG(tg.generate(6)); // generate the turtle movement rules
	tg.produce(i); // produce the final image

	i.save("C:/Users/msunde/Desktop/NerdThings/UnrealProjects/Thesis/TurtleGrammars-png/res/test.jpg");
}