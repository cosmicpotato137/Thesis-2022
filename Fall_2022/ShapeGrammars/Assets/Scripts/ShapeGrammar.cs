//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ShapeGrammar : MonoBehaviour
//{
//    [HideInInspector]
//    public ShapeGrammarParser parser;
//    [HideInInspector]
//    public ShapeGrammarDriver driver;

//    public void OnEnable()
//    {
//        // function definitions for the parser
//        parser = new ShapeGrammarParser();
//        parser.CompileParser();

//        driver = new ShapeGrammarDriver(transform);

//        Action<string> p = (name) => {
//            driver.PlaceShape(name);
//        };
//        var a = new GeneratorRule<string>("PlaceShape", p);
//        parser.AddRule(a);

//        Action<float, float, float> f = (x, y, z) => driver.Translate(new Vector3(x, y, z));
//        var b = new GeneratorRule<float, float, float>("Translate", f);
//        parser.AddRule(b);

//        Action push = () => driver.SaveTransform();
//        parser.AddRule(new GeneratorRule("Push", push));
//        Action pop = () => driver.LoadTransform();
//        parser.AddRule(new GeneratorRule("Pop", pop));

//    }

//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
