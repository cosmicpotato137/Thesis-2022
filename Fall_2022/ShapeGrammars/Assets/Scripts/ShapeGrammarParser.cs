﻿using CSharpParserGenerator;
using System;
using System.Collections.Generic;
using UnityEngine;
using cosmicpotato.DataStructures;

public class ShapeGrammarParser
{
    private Dictionary<string, SGGeneratorBase>     generators;
    private Dictionary<string, SGProducer>          producers;
    private Dictionary<string, SGVar>               variables;
    private Dictionary<string, SGVar>               prepVars;
    private Dictionary<string, Type>                sgTypes;

    private SGProdGen opTree;

    private LinkedList<SGRule> opQueue;

    private Parser<ELang> parser;


    public ShapeGrammarParser()
    {
        generators = new Dictionary<string, SGGeneratorBase>();
        producers = new Dictionary<string, SGProducer>();
        variables = new Dictionary<string, SGVar>();
        prepVars = new Dictionary<string, SGVar>();
        opQueue = new LinkedList<SGRule>();

        // types for sg language
        sgTypes = new Dictionary<string, Type>();
        sgTypes.Add("string", typeof(string));
        sgTypes.Add("int", typeof(int));
        sgTypes.Add("sgrule", typeof(SGRule));
        sgTypes.Add("sgprod", typeof(SGProdGen));
        sgTypes.Add("sggen", typeof(SGProdGen));

        SGVar depth = new SGVar("maxDepth", -1);
        prepVars.Add(depth.token, depth);
        SGVar oper = new SGVar("maxOper", -1);
        prepVars.Add(oper.token, oper);
    }

    public ParseResult<string> Parse(TextAsset text)
    {
        producers.Clear();
        variables.Clear();
        opQueue.Clear();
        return parser.Parse<string>(text.text);
    }

    public void RunShapeGrammar(int maxDepth, int maxOper = 100000)
    {
        if (prepVars.ContainsKey("maxDepth") && prepVars["maxDepth"].Get<int>() > 0)
            maxDepth = prepVars["maxDepth"].Get<int>();
        if (prepVars.ContainsKey("maxOper") && prepVars["maxOper"].Get<int>() > 0)
            maxOper = prepVars["maxOper"].Get<int>();

        SGRule.maxDepth = maxDepth;
        opQueue.AddLast(opTree);

        int i = 0;
        while (opQueue.Count > 0 && i < maxOper)
        {
            opQueue.First.Value.Call();
            opQueue.RemoveFirst();
            i++;
        }
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
            variables.ContainsKey(token) || prepVars.ContainsKey(token) ||
            sgTypes.ContainsKey(token);
    }

    private enum ELang
    {
        // production rules
        START, Rule, GenRuleList, ExpList, Exp, ProdRule, ProdRuleList, 
        Var, VarList, Array, GenRuleLists,
        // symbols
        Ignore, LParen, RParen, Number, Name, RArrow, Colon, Comma, Break,
        Pound, Equals, LCBrac, RCBrac, LBrac, RBrac, String, Star, Quote
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
            [ELang.LCBrac] = "{",
            [ELang.RCBrac] = "}",
            [ELang.LBrac] = "\\[",
            [ELang.RBrac] = "\\]",
            [ELang.Star] = "\\*",
        });

        var grammarRules = new GrammarRules<ELang>(new Dictionary<ELang, Token[][]>()
        {
            [ELang.START] = new Token[][]
            {
                new Token[] { ELang.ProdRuleList, new Op(o => o[0] = "Shape Grammar") },
                new Token[] { ELang.VarList, ELang.Break, ELang.ProdRuleList, new Op(o => o[0] = "Shape Grammar") }
            },
            // list of production rules
            [ELang.ProdRuleList] = new Token[][]
            {
                new Token[] { ELang.ProdRule,
                    new Op(o =>
                    {
                        SGProducer.opQueue = this.opQueue;
                        SGProducer p = o[0];
                        SGProdGen pg = new SGProdGen("__BEGIN__", () => p);
                        opTree = pg;
                        opTree.scope = Matrix4x4.identity;
                        opTree.depth = 0;
                    })
                },
                new Token[] { ELang.ProdRuleList, ELang.ProdRule }
            },
            [ELang.ProdRule] = new Token[][]
            {
                new Token[] { ELang.Name, ELang.Colon, ELang.LCBrac, ELang.GenRuleList, ELang.RCBrac,
                    new Op(o =>
                    {
                        if (NameExsists(o[0]))
                            throw new ArgumentException($"Name already exists: {o[0]}");

                        // make a new producer and add it's children
                        var p = new SGProducer(o[0]);
                        AddProducer(p);
                        Node<dynamic> genNode = o[3];
                        while (genNode != null)
                        {
                            try
                            {
                                p.rules.Add((SGRule)genNode.Value);
                                genNode = genNode.Next;
                            }
                            catch (Exception)
                            {
		                        throw new Exception("Non-rule found in a rule list");
                            }
                        }
                        o[0] = p;
                    })
                },
                new Token[] { ELang.Name, ELang.Colon, ELang.GenRuleLists, 
                    new Op(o => 
                    {
                        var p = new SGProducer(o[0]);
                        AddProducer(p);
                        Node<dynamic> genNode = o[2].Value;
                        while (genNode != null)
                        {
                            try
                            {
                                p.rules.Add((SGRule)genNode.Value);
                                genNode = genNode.Next;
                            }
                            catch (Exception)
                            {
                                throw new Exception("Non-rule found in a rule list");
                            }
                        }
                        o[0] = p;
                    })
                },
            },
            [ELang.GenRuleLists] = new Token[][]
            {
                new Token[] { ELang.LParen, ELang.Number, ELang.RParen, 
                    ELang.LCBrac, ELang.GenRuleList, ELang.RCBrac, 
                    new Op(o => o[0] = new Node<Node<dynamic>>(o[4])) },
                new Token[] { ELang.LParen, ELang.Number, ELang.RParen, 
                    ELang.LCBrac, ELang.GenRuleList, ELang.RCBrac, ELang.GenRuleLists,
                    new Op(o => 
                    {
                        var n = new Node<Node<dynamic>>(o[4]);
                        n.Next = o[6];
                        o[0] = n;
                    })
                }
            },
            // list of generator rules
            [ELang.GenRuleList] = new Token[][]
            {
                new Token[] { ELang.Exp, new Op(o => o[0] = new Node<dynamic>(o[0])) },
                new Token[] { ELang.Exp, ELang.GenRuleList,
                    new Op(o =>
                    {
                        o[0] = new Node<dynamic>(o[0]);
                        o[0].Next = o[1];
                    })
                }
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
            // single parameter
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
                        else if (prepVars.ContainsKey(o[0]))
                            o[0] = prepVars[o[0]].Get<dynamic>();
                        else if (sgTypes.ContainsKey(o[0]))
                            o[0] = sgTypes[o[0]];
                        else
                            throw new AccessViolationException($"name not found: {o[0]}");
                    }) 
                },
                //new Token[] { ELang.ProdRule, ELang.LParen, ELang.RParen, 
                //    new Op(o =>
                //    {
                //        // find the producer corresponding to SGProdGen
                //        string name = o[1];
                //        Func<SGProducer> findProd = () =>
                //        {
                //            if (producers.ContainsKey(name))
                //                return producers[name];
                //            else
                //                throw new MissingMethodException($"Shape grammar rule: {name} does not exist");
                //        };
                //        o[0] = new SGProdGen(name, findProd);
                //        o[0] = generators[o[1]].Get<SGRule>();
                //    })
                //},
                //new Token[] { ELang.Name, ELang.Star,
                //    new Op(o =>
                //    {
                //        string name = o[0];
                //        Func<SGProducer> findProd = () =>
                //        {
                //            if (producers.ContainsKey(name))
                //                return producers[name];
                //            else
                //                throw new MissingMethodException($"Shape grammar rule: {name} does not exist");
                //        };
                //        o[0] = new SGProdGen(name, findProd, depthFirst:true);
                //    })
                //},
                // function with no parameters
                new Token[] { ELang.Name, ELang.LParen, ELang.RParen,
                    new Op(o =>
                    {
                        if (generators.ContainsKey(o[0]))
                            o[0] = generators[o[0]].Copy();
                        else
                        {
                            //throw new MethodAccessException($"Function: {o[0]} does not exist");
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
                            //o[0] = generators[o[0]].Get<SGRule>();
                        }
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
                            else if (i < g.parameters.Length)
                                throw new ArgumentException($"Too few arguments in function: {o[0]}");
                            else
                                o[0] = g;
                        }
                        else
                        {
                            throw new MethodAccessException($"Function: {o[0]} does not exist");
                        }
                    })
                },

                // typed list
                new Token[] { ELang.Name, ELang.LCBrac, ELang.ExpList, ELang.RCBrac,
                    new Op(o =>
                    {
                        //o[0] = (SGProdGen)o[1].Value;
                        if (!sgTypes.ContainsKey(o[0]))
                            throw new TypeUnloadedException($"Type does not exist: {o[0]}");
                        Type arrType = sgTypes[o[0]];

                        Node<dynamic> rules = o[2];
                        List<dynamic> gens = new List<dynamic>();
                        while (rules != null)
                        {
                            var typ = rules.Value.GetType().BaseType;
                            if (typ == typeof(SGRule) || typ == typeof(SGGeneratorBase))
                            {
                                var r = rules.Value;
                                gens.Add(r);
                                rules = rules.Next;
                            }
                            else
                                throw new ArrayTypeMismatchException($"Unsupported list type: {rules.Value.GetType()}");
                        }
                        o[0] = new SGRule[gens.Count];
                        gens.CopyTo(o[0]);
                    })
                }
            },

            // variables
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