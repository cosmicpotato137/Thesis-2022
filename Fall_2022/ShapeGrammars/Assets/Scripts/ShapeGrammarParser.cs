using CSharpParserGenerator;
using System;
using System.Collections.Generic;
using UnityEngine;
using cosmicpotato.Datastructures;

public class ShapeGrammarParser
{
    private Dictionary<string, SGGeneratorBase>     generators;
    private Dictionary<string, SGProducer>          producers;
    private Dictionary<string, SGVar>      variables;
    private Dictionary<string, SGVar>      prepVars;

    private SGRule opTree;

    private Parser<ELang> parser;

    public ShapeGrammarParser()
    {
        generators = new Dictionary<string, SGGeneratorBase>();
        producers = new Dictionary<string, SGProducer>();
        variables = new Dictionary<string, SGVar>();
        prepVars = new Dictionary<string, SGVar>();

        SGVar depth = new SGVar("maxDepth", -1);
        prepVars.Add(depth.token, depth);
        SGVar oper = new SGVar("maxOper", -1);
        prepVars.Add(oper.token, oper);
    }

    public ParseResult<string> Parse(TextAsset text)
    {
        producers.Clear();
        variables.Clear();
        return parser.Parse<string>(text.text);
    }

    public void RunShapeGrammar(int maxDepth, int maxOper = 100000)
    {
        if (prepVars.ContainsKey("maxDepth") && prepVars["maxDepth"].Get<int>() > 0)
            maxDepth = prepVars["maxDepth"].Get<int>();
        if (prepVars.ContainsKey("maxOper") && prepVars["maxOper"].Get<int>() > 0)
            maxOper = prepVars["maxOper"].Get<int>();

        SGRule.maxOper = maxOper;
        opTree.Call(maxDepth);
    }

    public void AddGenerator(SGGeneratorBase rule)
    {
        generators.Add(rule.token, rule);
    }

    public void AddProducer(SGProducer producer)
    {
        producers.Add(producer.token, producer);
    }

    public bool NameExsists(string token)
    {
        return producers.ContainsKey(token) || generators.ContainsKey(token) || 
            variables.ContainsKey(token) || prepVars.ContainsKey(token);
    }

    private enum ELang
    {
        // production rules
        START, Rule, RuleList, ExpList, Exp, ProdRule, ProdRuleList, 
        Var, VarList,
        // symbols
        Ignore, LParen, RParen, Number, Name, RArrow, Colon, Comma, Break,
        Pound, Equals, LBrac, RBrac, String
    }

    public void CompileParser()
    {
        var tokens = new LexerDefinition<ELang>(new Dictionary<ELang, TokenRegex>
        {
            [ELang.Ignore] = "[\\s\\n]+",
            [ELang.Name] = @"[A-Za-z_][a-zA-Z0-9_]*",
            [ELang.String] = "\"[A-Za-z_][a-zA-Z0-9_]*\"",// (?:[^\"\\]|\\.)*
            [ELang.LParen] = "\\(",
            [ELang.RParen] = "\\)",
            [ELang.Number] = "[-+]?\\d*(\\.\\d+)?",
            [ELang.RArrow] = @"->",
            [ELang.Colon] = @":",
            [ELang.Comma] = @",",
            [ELang.Break] = @"%%",
            [ELang.Pound] = "\\#",
            [ELang.Equals] = @"=",
            [ELang.LBrac] = "{",
            [ELang.RBrac] = "}"
        });


        var grammarRules = new GrammarRules<ELang>(new Dictionary<ELang, Token[][]>()
        {
            [ELang.START] = new Token[][]
            {
                new Token[] { ELang.ProdRuleList, new Op(o => o[0] = "Shape Grammar") },
                new Token[] { ELang.VarList, ELang.Break, ELang.ProdRuleList, new Op(o => o[0] = "Shape Grammar")}
            },
            // list of production rules
            [ELang.ProdRuleList] = new Token[][]
            {
                new Token[] { ELang.ProdRule, new Op(o => opTree = o[0]) },
                new Token[] { ELang.ProdRuleList, ELang.ProdRule }
            },
            [ELang.ProdRule] = new Token[][]
            {
                new Token[] { ELang.Name, ELang.Colon, ELang.LBrac, ELang.RuleList, ELang.RBrac,
                    new Op(o =>
                    {
                        if (NameExsists(o[0]))
                            throw new ArgumentException($"Name already exists: {o[0]}");

                        // make a new producer and add it's children
                        var p = new SGProducer(o[0]);
                        AddProducer(p);
                        Node<SGRule> genNode = o[3];
                        while (genNode != null)
                        {
                            genNode.Value.parent = p;
                            p.rules.Add(genNode.Value);
                            genNode = genNode.Next;
                        }
                        o[0] = p;
                    })
                }
            },
            // list of generator rules
            [ELang.RuleList] = new Token[][]
            {
                new Token[] { ELang.Rule, new Op(o => o[0] = new Node<SGRule>(o[0])) },
                new Token[] { ELang.Rule, ELang.RuleList,
                    new Op(o => 
                    {
                        o[0] = new Node<SGRule>(o[0]);
                        o[0].Next = o[1];
                    }) 
                }
            },
            [ELang.Rule] = new Token[][]
            {
                // SGProdGen: a placeholder for SGProducer, handled at 'runtime'
                new Token[] { ELang.Name,
                    new Op(o =>
                    {
                        // find the producer corresponding to SGProdGen
                        string name = o[0];
                        Func<SGProducer> findProd = () =>
                        {
                            if (producers.ContainsKey(name)) 
                                return producers[name];
                            else
                                throw new MissingMethodException($"Shape grammar rule: {name} does not exist");
                        };
                        o[0] = new SGProdGen(name, findProd);
                    })
                },
                // function with no parameters
                new Token[] { ELang.Name, ELang.LParen, ELang.RParen,
                    new Op(o =>
                    {
                        if (generators.ContainsKey(o[0]))
                            o[0] = generators[o[0]].Copy();
                        else
                            throw new MethodAccessException($"Function: {o[0]} does not exist");
                    })
                },
                // function with parameters
                new Token[] { ELang.Name, ELang.LParen, ELang.ExpList, ELang.RParen,
                    new Op(o =>
                    {
                        if (generators.ContainsKey(o[0]))
                        {
                            // add all parameters to the generator
                            Node<dynamic> expList = o[2];
                            int i = 0;
                            var g = generators[o[0]].Copy();
                            while (expList != null && i < g.parameters.Length)
                            {
                                g.parameters[i] = expList.Value;
                                expList = expList.Next;
                                i++;
                            }
                            // make sure the function has the right number of params
                            if (expList != null)
                                throw new ArgumentException($"Too many arguments in function: {o[0]}");
                            else
                                o[0] = g;
                        }
                        else
                        {
                            throw new MethodAccessException($"Function: {o[0]} does not exist");
                        }
                    })
                },
            },
            // list of parameters
            [ELang.ExpList] = new Token[][]
            {
                new Token[] { ELang.Exp, new Op(o => o[0] = new Node<dynamic>(o[0])) },
                new Token[] { ELang.Exp, ELang.Comma, ELang.ExpList,
                    new Op(o => 
                    {
                        o[0] = new Node<dynamic>(o[0]);
                        o[0].Next = o[2];
                    })
                }
            },
            // single argument
            [ELang.Exp] = new Token[][]
            {
                new Token[] { ELang.Number, new Op(o => o[0] = Convert.ToDouble(o[0])) },
                new Token[] { ELang.String, 
                    new Op(o => 
                    { 
                        string s = Convert.ToString(o[0]);
                        o[0] = s.Substring(1, s.Length - 2);
                    }) 
                },
                new Token[] { ELang.Name, 
                    new Op(o => 
                    {
                        if (variables.ContainsKey(o[0]))
                            o[0] = variables[o[0]].Get<dynamic>();
                    
                    }) 
                }
            },

            [ELang.VarList] = new Token[][]
            {
                new Token[] { ELang.Var },
                new Token[] { ELang.VarList, ELang.Var }
            },
            [ELang.Var] = new Token[][]
            {
                new Token[] { ELang.Pound, "\\s*var\\s+", ELang.Name, ELang.Exp,
                    new Op(o =>
                    {
                        if (NameExsists(o[2]))
                        {
                            throw new Exception($"Name already defined: {o[2]}");
                        }
                        var v = new SGVar(o[2], o[3]);
                        variables.Add(v.token, v);
                    })
                },
                new Token[] { ELang.Pound, "\\s*define\\s+", ELang.Name, ELang.Exp,
                    new Op(o =>
                    {
                        if (prepVars.ContainsKey(o[2]))
                            prepVars[o[2]].Set(o[3]);
                        else
                            Debug.LogWarning($"Preprocessing var not found: {o[2]}");
                    })
                }
            }
        });

        var lexer = new Lexer<ELang>(tokens, ELang.Ignore);
        parser = new ParserGenerator<ELang>(lexer, grammarRules).CompileParser();
    }
}