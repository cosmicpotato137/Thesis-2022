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

public class Argument<T>
{
    public Type type;
    public T value;
    Argument(Type type, T value)
    {
        this.type = type;
        this.value = value;
    }
}

public class GeneratorRuleBase
{
    public string token;
    public string[] parameters;
    public List<Type> types;

    public GeneratorRuleBase(string token)
    {
        this.token = token;
    }

    public virtual void Call()
    {
        throw new NotImplementedException();
    }
}

public class GeneratorRule : GeneratorRuleBase
{
    private Action callback;

    public GeneratorRule(string token, Action callback) : base(token)
    {
        this.callback = callback;
        types = new List<Type>();
        parameters = new string[] { };
    }

    public override void Call()
    {
        if (types.Count != parameters.Length)
            Debug.LogError("invalid number of parameters");

        callback();
    }
}

public class GeneratorRule<T1> : GeneratorRuleBase
{
    private Action<T1> callback;
    T1 p1;

    public GeneratorRule(string token, Action<T1> callback) : base(token)
    {
        this.callback = callback;
    }

    public override void Call()
    {
        //callback(fields[0]);
    }
}

public class GeneratorRule<T1, T2> : GeneratorRuleBase
{
    private Action<T1, T2> callback;
    private T1 p1;
    private T2 p2;

    public GeneratorRule(string token, Action<T1, T2> callback, T1 p1, T2 p2) : base(token)
    {
        this.callback = callback;
        this.p1 = p1;
        this.p2 = p2;
    }

    public override void Call()
    {
        //callback(p1, p2);
    }
}

public class GeneratorRule<T1, T2, T3> : GeneratorRuleBase
{
    private Action<T1, T2, T3> callback;

    public GeneratorRule(string token, Action<T1, T2, T3> callback) : base(token)
    {
        this.callback = callback;
    }

    public override void Call()
    {
        //callback(p1, p2, p3);
    }
}

// 
// 
// 

//public static class Tokens
//{
//    //public static string Generator = @"[A-Za-z_][a-zA-Z0-9_]*\((([0-9]+\s*,\s*)*[0-9]+|)\)";
//    public static string Name = @"[A-Za-z_][a-zA-Z0-9_]*";
//    public static string Number = @"[0-9]+";
//    public static string Newline = @"\n|\r|\r\n";
//    public static string LParen = @"(";
//    public static string RParen = @")";
//    public static string RArrow = @"->";
//    public static string Colon = @":";

//    public static float MakeFloat(string token)
//    {
//        return float.Parse(token);
//    }
//}

public class ShapeGrammarParser
{
    public Dictionary<string, GeneratorRuleBase> rules = 
        new Dictionary<string, GeneratorRuleBase>();

    private Parser<ELang> parser;

    public ParseResult<string> Parse(TextAsset text)
    {
        return parser.Parse<string>(text.text);
    }
    //public ShapeGrammarParser()
    //{
    //    this.rules = new Dictionary<string, GeneratorRuleBase>();
    //}

    private enum ELang
    {
        A, ExpList, Exp, //E, M, T,

        //  Pow, Mul, Sub, Plus, Div,
        Ignore, LParen, RParen, Number, Name, RArrow, Colon, Comma
    }

    public void CompileParser()
    {
        var tokens = new LexerDefinition<ELang>(new Dictionary<ELang, TokenRegex>
        {
            [ELang.Ignore] = "[ \\n]+",
            [ELang.Name] = @"[A-Za-z_][a-zA-Z0-9_]*",
            [ELang.LParen] = "\\(",
            [ELang.RParen] = "\\)",
            [ELang.Number] = "[-+]?\\d*(\\.\\d+)?",
            [ELang.RArrow] = @"->",
            [ELang.Colon] = @":",
            [ELang.Comma] = @","
        });


        var grammarRules = new GrammarRules<ELang>(new Dictionary<ELang, Token[][]>()
        {

            [ELang.A] = new Token[][]
            {
                new Token[] { ELang.Name, ELang.LParen, ELang.RParen,
                    new Op(o =>
                    {
                        string name = o[0];
                        if (rules.ContainsKey(name))
                            rules[name].Call();
                    })
                },
                new Token[] { ELang.Name, ELang.LParen, ELang.Exp, ELang.RParen,
                    new Op(o =>
                    {
                        if (rules.ContainsKey(o[0]))
                        {
                            rules[o[0]].parameters = new string[] { o[0] };
                            rules[o[0]].Call();
                        }
                    })
                }
            },
            [ELang.Exp] = new Token[][]
            {
                new Token[] { ELang.Number }
            }

            // A' -> A
            // A -> A + M
            // A -> A - M
            // A -> M
            // 
            // M -> M * E
            // M -> M / E
            // M -> E
            // 
            // E -> E ^ T
            // E -> T
            // 
            // T -> ( A )
            // T -> number

            //[ELang.A] = new Token[][]
            //    {
            //        new Token[] { ELang.A, ELang.Plus, ELang.M, new Op(o => { o[0] += o[2]; }) },
            //        new Token[] { ELang.A, ELang.Sub, ELang.M, new Op(o => { o[0] -= o[2]; }) },
            //        new Token[] { ELang.M }
            //    },
            //[ELang.M] = new Token[][]
            //    {
            //        new Token[] { ELang.M, ELang.Mul, ELang.E, new Op(o => { o[0] *= o[2]; }) },
            //        new Token[] { ELang.M, ELang.Div, ELang.E, new Op(o => { o[0] /= o[2]; }) },
            //        new Token[] { ELang.E }
            //    },
            //[ELang.E] = new Token[][]
            //    {
            //        new Token[] { ELang.E, ELang.Pow, ELang.T, new Op(o => { o[0] = Math.Pow(o[0], o[2]); }) },
            //        new Token[] { ELang.T }
            //    },
            //[ELang.T] = new Token[][]
            //    {
            //        new Token[] { ELang.LParenthesis, ELang.A, ELang.RParenthesis, new Op(o => {o[0] = o[1]; }) },
            //        new Token[] { ELang.Number, new Op(o => o[0] = Convert.ToDouble(o[0])) }
            //    }
        });

        var lexer = new Lexer<ELang>(tokens, ELang.Ignore);
        parser = new ParserGenerator<ELang>(lexer, grammarRules).CompileParser();
    }

    //public void Parse()
    //{
    //    string fs = grammarFile.text;
    //    string[] fLines = Regex.Split(fs, Tokens.Newline);

    //    Match m;
    //    while (m.Success)
    //    {
    //        m = Regex.Match(fs, Tokens.Name);
    //        MatchCollection mc = Regex.Matches()
    //    }

    //    Match name = Regex.Match(fLines[0], Tokens.Name);
    //    if (name.Success && rules.ContainsKey(name.Value))
    //    {
    //        MatchCollection nums_s = Regex.Matches(m.Value, Tokens.Number);
    //        string[] parameters = new string[nums_s.Count];
    //        nums_s.CopyTo(parameters, 0);

    //        rules[name.Value].parameters = parameters;
    //        rules[name.Value].Call();
    //    }
    //    else
    //    {
    //        string error = name + " is not a valid generator";
    //        Debug.LogError(error);
    //    }


}