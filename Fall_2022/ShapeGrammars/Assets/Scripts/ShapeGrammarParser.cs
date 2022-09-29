using CSharpParserGenerator;
using System;
using System.Collections.Generic;
using UnityEngine;


//static void Main(string[] args)
//{
//    var parser = CompileParser();

//    Console.Write("Write math expression: ");
//    string expression = Console.ReadLine();

//    var result = parser.Parse<double>(expression);

//    if (result.Success)
//    {
//        Console.WriteLine($"Result: {result.Value}");
//    }
//    else
//    {
//        Console.WriteLine("Some errors have been detected:");
//        foreach (var error in result.Errors)
//        {
//            Console.WriteLine(error.Description);
//        }
//    }
//}

// 
// Grammar rule classes
// 

public class SGRule 
{
    public string token;

    public SGRule(string token)
    {
        this.token = token;
    }

    public SGRule(SGRule other)
    {
        this.token = other.token;
    }

    public virtual SGRule Copy()
    {
        throw new NotImplementedException();
    }

    public virtual void Call()
    {
        throw new NotImplementedException();
    }
}

public class SGProducer : SGRule
{
    public List<SGRule> rules;
    public Action<SGRule[]> callback;

    public SGProducer(string token, Action<SGRule[]> callback) : base(token)
    {
        this.callback = callback;
        rules = new List<SGRule>();
    }

    public SGProducer(SGProducer other) : base(other)
    {
        this.callback = other.callback;
        this.rules = other.rules;
    }

    public override SGRule Copy()
    {
        return new SGProducer(this.token, this.callback);
    }

    public override void Call()
    {
        callback(rules.ToArray());
    }
}

public class SGGeneratorBase : SGRule
{
    public dynamic[] parameters;

    public SGGeneratorBase(string token) : base(token)
    {
    }

    public SGGeneratorBase(SGGeneratorBase other) : base(other)
    {
        this.parameters = other.parameters;
    }
}

public class SGGenerator : SGGeneratorBase
{
    private Action callback;

    public SGGenerator(string token, Action callback) : base(token)
    {
        this.callback = callback;
        parameters = new dynamic[0];
    }

    public SGGenerator(SGGenerator other) : base(other)
    {
        this.callback = other.callback;
    }
    public override SGRule Copy()
    {
        return new SGGenerator(token, callback);
    }


    public override void Call()
    {
        callback();
    }
}

public class SGGenerator<T1> : SGGeneratorBase
{
    private Action<T1> callback;

    public SGGenerator(string token, Action<T1> callback) : base(token)
    {
        this.callback = callback;
        parameters = new dynamic[1];
    }

    public SGGenerator(SGGenerator<T1> other) : base(other)
    {
        this.callback = other.callback;
    }

    public override SGRule Copy()
    {
        return new SGGenerator<T1>(token, callback);
    }

    public override void Call()
    {
        callback((T1)parameters[0]);
    }
}

public class SGGenerator<T1, T2> : SGGeneratorBase
{
    private Action<T1, T2> callback;

    public SGGenerator(string token, Action<T1, T2> callback) : base(token)
    {
        this.callback = callback;
        parameters = new dynamic[2];
    }

    public SGGenerator(SGGenerator<T1, T2> other) : base(other)
    {
        this.callback = other.callback;
    }

    public override SGRule Copy()
    {
        return new SGGenerator<T1, T2>(token, callback);
    }

    public override void Call()
    {
        callback((T1)parameters[0], (T2)parameters[1]);
    }
}

public class SGGenerator<T1, T2, T3> : SGGeneratorBase
{
    private Action<T1, T2, T3> callback;

    public SGGenerator(string token, Action<T1, T2, T3> callback) : base(token)
    {
        this.callback = callback;
        parameters = new dynamic[3];
    }

    public SGGenerator(SGGenerator<T1, T2, T3> other) : base(other)
    {
        this.callback = other.callback;
    }

    public override SGRule Copy()
    {
        return new SGGenerator<T1, T2, T3>(token, callback);
    }

    public override void Call()
    {
        callback((T1)parameters[0], (T2)parameters[1], (T3)parameters[2]);
    }
}

public class SGLinkedList<T>
{
    public T Value;
    public SGLinkedList<T> Prev;
    public SGLinkedList<T> Next;

    public SGLinkedList(T value)
    {
        Value = value;
    }
}

public class ShapeGrammarParser
{
    public Dictionary<string, SGGeneratorBase> generators { get; private set; }
    public Dictionary<string, SGProducer> producers { get; private set; }
    public LinkedList<SGRule> opQueue;

    private Parser<ELang> parser;

    public ShapeGrammarParser()
    {
        generators = new Dictionary<string, SGGeneratorBase>();
        producers = new Dictionary<string, SGProducer>();
        opQueue = new LinkedList<SGRule>();
    }

    public ParseResult<string> Parse(TextAsset text)
    {
        opQueue.Clear();
        producers.Clear();
        return parser.Parse<string>(text.text);
    }

    public void AddGenerator(SGGeneratorBase rule)
    {
        generators.Add(rule.token, rule);
    }

    public void AddProducer(string token)
    {
        var p = new SGProducer(token, AddRules);
        producers.Add(token, p);
    }

    public void AddProducer(SGProducer p)
    {
        producers.Add(p.token, p);
    }

    public void AddRules(SGRule[] rules)
    {
        for (int i = rules.Length-1; i >= 0; i--)
            opQueue.AddFirst(rules[i]);
    }

    public void PushRule(SGRule rule)
    {
        opQueue.AddLast(rule);
    }

    private enum ELang
    {
        START, Rule, RuleList, ExpList, Exp, ProdRule, ProdRuleList, //E, M, T,

        //  Pow, Mul, Sub, Plus, Div,
        Ignore, LParen, RParen, Number, Name, RArrow, Colon, Comma, Break
    }

    public void CompileParser()
    {
        SGProducer p = new SGProducer("hi", AddRules);
        var tokens = new LexerDefinition<ELang>(new Dictionary<ELang, TokenRegex>
        {
            [ELang.Ignore]  = "[\\s\\n]+",
            [ELang.Name]    = @"[A-Za-z_][a-zA-Z0-9_]*",
            [ELang.LParen]  = "\\(",
            [ELang.RParen]  = "\\)",
            [ELang.Number]  = "[-+]?\\d*(\\.\\d+)?",
            [ELang.RArrow]  = @"->",
            [ELang.Colon]   = @":",
            [ELang.Comma]   = @",",
            [ELang.Break]   = @"%%"
        });


        var grammarRules = new GrammarRules<ELang>(new Dictionary<ELang, Token[][]>()
        {
            [ELang.START] = new Token[][]
            {
                new Token[] { ELang.ProdRuleList, new Op(o => o[0] = "Production Rule List") },
                new Token[] { ELang.RuleList, 
                    new Op(o => 
                    {
                        SGLinkedList<SGGeneratorBase> genNode = o[0];
                        while (genNode != null)
                        {
                            opQueue.AddLast(genNode.Value);
                            genNode = genNode.Next;
                        }
                        o[0] = "Generator List";
                    })
                }
            },
            [ELang.ProdRuleList] = new Token[][]
            {
                new Token[] { ELang.ProdRule, ELang.Break, 
                    new Op(o =>
                    {
                        foreach (SGRule r in o[0].rules)
                            opQueue.AddLast(r);
                    })
                },
                new Token[] { ELang.ProdRuleList, ELang.ProdRule, ELang.Break }
            },
            [ELang.ProdRule] = new Token[][]
            {
                new Token[] { ELang.Name, ELang.Colon, ELang.RuleList,
                    new Op(o =>
                    {
                        if (producers.ContainsKey(o[0]))
                            throw new ArgumentException("Cannot have duplicate producer labels");

                        SGProducer p = new SGProducer(o[0], (Action<SGRule[]>)AddRules);
                        AddProducer(p);
                        SGLinkedList<SGGeneratorBase> genNode = o[2];
                        while (genNode != null)
                        {
                            p.rules.Add(genNode.Value);
                            genNode = genNode.Next;
                        }
                        o[0] = p;
                    })
                }
            },
            [ELang.RuleList] = new Token[][]
            {
                new Token[] { ELang.Rule, new Op(o => o[0] = new SGLinkedList<SGGeneratorBase>(o[0])) },
                new Token[] { ELang.Rule, ELang.RuleList,
                    new Op(o => 
                    {
                        o[0] = new SGLinkedList<SGGeneratorBase>(o[0]);
                        o[0].Next = o[1];
                    }) 
                }
            },
            [ELang.Rule] = new Token[][]
            {
                new Token[] { ELang.Name,
                    new Op(o =>
                    {
                        Action<string> a = (token) => {
                            if (producers.ContainsKey(token))
                                producers[token].Call();
                        };
                        o[0] = new SGGenerator<string>(o[0], a);
                        o[0].parameters = new dynamic[] { o[0].token };
                    })
                },
                new Token[] { ELang.Name, ELang.LParen, ELang.RParen,
                    new Op(o =>
                    {
                        if (generators.ContainsKey(o[0]))
                            o[0] = generators[o[0]].Copy();
                        else
                            throw new MethodAccessException($"Function: {o[0]} does not exist");
                    })
                },
                new Token[] { ELang.Name, ELang.LParen, ELang.ExpList, ELang.RParen,
                    new Op(o =>
                    {
                        if (generators.ContainsKey(o[0]))
                        {
                            SGLinkedList<dynamic> expList = o[2];
                            int i = 0;
                            var g = generators[o[0]].Copy();
                            while (expList != null && i < g.parameters.Length)
                            {
                                g.parameters[i] = expList.Value;
                                expList = expList.Next;
                                i++;
                            }
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
            [ELang.ExpList] = new Token[][]
            {
                new Token[] { ELang.Exp, new Op(o => { 
                    o[0] = new SGLinkedList<dynamic>(o[0]); 
                }) },
                new Token[] { ELang.Exp, ELang.Comma, ELang.ExpList,
                    new Op(o => 
                    {
                        o[0] = new SGLinkedList<dynamic>(o[0]);
                        o[0].Next = o[2];
                    })
                }
            },
            [ELang.Exp] = new Token[][]
            {
                new Token[] { ELang.Number, new Op(o => {
                    o[0] = Convert.ToDouble(o[0]); 
                }) },
                new Token[] { ELang.Name, new Op(o => o[0] = Convert.ToString(o[0])) }
            }
        });

        var lexer = new Lexer<ELang>(tokens, ELang.Ignore);
        parser = new ParserGenerator<ELang>(lexer, grammarRules).CompileParser();
    }
}