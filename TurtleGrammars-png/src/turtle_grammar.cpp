#include "turtle_grammar.h"
#include <stdexcept>


TurtleGrammar::~TurtleGrammar()
{
	auto iter = m_Rules.begin();
	while (iter != m_Rules.end())
	{
		delete iter->second;
		++iter;
	}
}

bool TurtleGrammar::addRule(TurtleGrammarRule* rule)
{
	if (m_Rules.find(rule->match) != m_Rules.end())
	{
		LOG("Rule with the match: " << rule->match << " already exists");
		return false;
	}
	m_Rules[rule->match] = rule;
	return true;
}

TurtleGrammarRule* TurtleGrammar::getRule(char match)
{
	if (m_Rules.find(match) == m_Rules.end())
		return nullptr;
	return m_Rules[match];
}

std::string TurtleGrammar::generate(int n)
{
	for (int i = 0; i < n; i++)
	{
		std::string result;
	
		for (int j = 0; j < m_Current.size(); j++)
		{
			char r = m_Current[j];
			if (m_Rules.find(r) == m_Rules.end())
			{
				LOG("rule not found: " + r);
				throw std::exception();
			}
			result += m_Rules[r]->replace;
		}
		m_Current = result;
	}

	return m_Current;
}

void TurtleGrammar::produce(Image& img)
{
	for (int i = 0; i < m_Current.size(); i++)
	{
		m_Rules[m_Current[i]]->produce(m_Turtle, img);
	}
}