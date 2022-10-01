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

	std::vector<Turtle> turtleStack;
	double angle = 22.5 * pi<double>() / 180.0;
	float len = 8;

	// start production rule
	std::string start = "I";
	TurtleGrammarRule* in = new TurtleGrammarRule{ 'I', "F" };
	// production rule for Koch island!
	//TurtleGrammarRule* F = new TurtleGrammarRule{ 'F', "F-F+F+FF-F-F+F" };
	// different variation
	TurtleGrammarRule* F = new TurtleGrammarRule{ 'F', "FF-[-F+F+F]+[+F-F-F]" };
	F->produce = [len](Turtle& t, Image& i) {
		std::vector<vec2> verts;
		verts.push_back(t.position);
		t.position += vec2(cos(t.rotation), sin(t.rotation)) * len;
		verts.push_back(t.position);
		draw_lines(verts, i);
	};
	TurtleGrammarRule* f = new TurtleGrammarRule{ 'f', "f" };
	f->produce = [len](Turtle& t, Image& i) {
		t.position += vec2(cos(t.rotation), sin(t.rotation)) * len;
	};
	TurtleGrammarRule* p = new TurtleGrammarRule{ '+', "+" };
	p->produce = [angle](Turtle& t, Image& i) {
		t.rotation += angle;
	};
	TurtleGrammarRule* m = new TurtleGrammarRule{ '-', "-" };
	m->produce = [angle](Turtle& t, Image& i) {
		t.rotation -= angle;
	};
	TurtleGrammarRule* push = new TurtleGrammarRule{ '[', "[" };
	push->produce = [&turtleStack](Turtle& t, Image& i) {
		Turtle nt;
		nt.position = t.position;
		nt.rotation = t.rotation;
		turtleStack.push_back(nt);
	};
	TurtleGrammarRule* pop = new TurtleGrammarRule{ ']', "]" };
	pop->produce = [&turtleStack](Turtle& t, Image& i) {
		t = turtleStack.back();
		turtleStack.pop_back();
	};

	
	// setting up the grammar
	TurtleGrammar tg;
	tg.setStart(start);
	tg.setTurtle({ 0.0f, vec2(imgSize/2, imgSize/2) });
	tg.addRule(in);
	tg.addRule(F);
	tg.addRule(f);
	tg.addRule(p);
	tg.addRule(m);
	tg.addRule(push);
	tg.addRule(pop);
	LOG(tg.generate(4)); // generate the turtle movement rules
	tg.produce(i); // produce the final image

	i.save("C:/NerdThings/thesis/TurtleGrammars-png/res/test.jpg");
}