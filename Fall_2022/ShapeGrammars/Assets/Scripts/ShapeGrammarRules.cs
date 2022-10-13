using System;
using System.Collections.Generic;
using UnityEngine;
using cosmicpotato.Scope;

// Grammar rule classes

public class SGObj
{
    public string token;

    protected SGObj(string token)
    {
        this.token = token;
    }

    protected SGObj(SGObj other)
    {
        this.token = other.token;
    }
}

public class SGVar : SGObj
{
    private dynamic value;

    public SGVar(string token, dynamic value) : base(token)
    {
        this.value = value;
    }

    public SGVar(SGVar other) : base(other)
    {
        this.value = other.value;
    }

    public T Get<T>()
    {
        return (T)value;
    }

    public void Set(dynamic value)
    {
        this.value = value;
    }
}

public class SGProducer : SGObj
{
    public List<LinkedList<SGRule>> ruleLists;
    public List<float> p;

    public static LinkedList<SGRule> opQueue;
    public static System.Random rg = new System.Random(1234);
    

    public SGProducer(string token) : base(token)
    {
        ruleLists = new List<LinkedList<SGRule>>();
        p = new List<float>();
    }

    public SGProducer(SGProducer other) : base(other)
    {
        this.ruleLists = other.ruleLists;
        this.p = other.p;
    }

    public void PushChildren(SGProdGen prodGen)
    {
        int idx = 0;
        if (p.Count > 0)
        {
            // normalize probabilities
            List<float> ps = new List<float>();
            float mag = 0;
            foreach (float f in p)
                mag += f;
            float tot = 0;
            for (int i = 0; i < p.Count; i++)
            {
                ps.Add(tot + p[i] / mag);
                tot += p[i] / mag;
            }

            float rand = (float)rg.NextDouble();
            while (ps[idx] < rand)
            {
                idx++;
            }
            if (idx == ps.Count)
                idx--;
        }

        var rules = ruleLists[idx];
        // for shape elements that must be evaluated first
        if (prodGen.depthFirst)
        {
            LinkedListNode<SGRule> n = rules.Last;
            while (n != null)
            {
                var rule = n.Value;
                rule.depth = prodGen.depth + 1;
                rule.parent = prodGen.parent;
                opQueue.AddAfter(opQueue.First, rule.Copy());
                n = n.Previous;
            }
        }
        else
        {
            foreach (SGRule rule in rules)
            {
                rule.depth = prodGen.depth + 1;
                rule.parent = prodGen;
                opQueue.AddLast(rule.Copy());
            }
        }
    }
}

public class SGRule : SGObj
{
    public SGProdGen parent; // parent rule in production
    public static int maxDepth;
    public int depth;


    protected SGRule(string token) : base(token)
    {
    }

    protected SGRule(SGRule other) : base(other)
    {
        this.parent = other.parent;
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

public class SGProdGen : SGRule
{
    public Func<SGProducer> callback;
    private SGProducer prod;
    public bool depthFirst { get; private set; }

    public bool adoptParentScope;
    public Matrix4x4 scope;
    private LinkedList<Matrix4x4> scopeStack;
    public GameObject gameObject;

    public SGProdGen(string token, Func<SGProducer> callback, bool adoptParentScope=true, bool depthFirst=false) : base(token)
    {
        scopeStack = new LinkedList<Matrix4x4>();
        this.callback = callback;
        this.depthFirst = depthFirst;
        this.adoptParentScope = adoptParentScope;
    }

    public SGProdGen(SGProdGen other) : base(other)
    {
        this.callback = other.callback;
        this.scope = other.scope;
        this.scopeStack = other.scopeStack;
        this.gameObject = other.gameObject;
        this.depthFirst = other.depthFirst;
        this.depth = other.depth;
    }

    public override SGRule Copy()
    {
        var pg = new SGProdGen(this.token, this.callback);
        pg.scope = scope;
        pg.scopeStack = scopeStack;
        pg.gameObject = gameObject;
        pg.parent = parent;
        pg.depth = depth;
        pg.depthFirst = depthFirst;
        pg.adoptParentScope = adoptParentScope;
        return pg;
    }

    public override void Call()
    {
        if (depth > maxDepth)
            return;
        if (prod == null)
            prod = callback();

        if (parent != null && adoptParentScope)
        {
            this.scope = parent.scope;
            this.gameObject = parent.gameObject;
        }

        // pass scope and gameobject down the tree
        prod.PushChildren(this);
    }

    public void SaveTransform()
    {
        scopeStack.AddLast(scope);
    }

    public void LoadTransform()
    {
        if (scopeStack.Last == null)
        {
            Debug.LogWarning("Trying to pop from empty stack");
            return;
        }
        scope = scopeStack.Last.Value;
        scopeStack.RemoveLast();
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
    private Action<SGProdGen> callback;

    public SGGenerator(string token, Action<SGProdGen> callback) : base(token)
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
        var n = new SGGenerator(token, callback);
        n.parent = parent;
        n.depth = depth;
        parameters.CopyTo(n.parameters, 0);

        return n;
    }

    public override void Call()
    {
        callback(parent);
    }
}

public class SGGenerator<T1> : SGGeneratorBase
{
    private Action<SGProdGen, T1> callback;

    public SGGenerator(string token, Action<SGProdGen, T1> callback) : base(token)
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
        var n = new SGGenerator<T1>(token, callback);
        n.parent = parent;
        n.depth = depth;
        parameters.CopyTo(n.parameters, 0);

        return n;
    }

    public override void Call()
    {
        callback(parent, (T1)parameters[0]);
    }
}

public class SGGenerator<T1, T2> : SGGeneratorBase
{
    private Action<SGProdGen, T1, T2> callback;

    public SGGenerator(string token, Action<SGProdGen, T1, T2> callback) : base(token)
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
        var n = new SGGenerator<T1, T2>(token, callback);
        n.parent = parent;
        n.depth = depth;
        parameters.CopyTo(n.parameters, 0);

        return n;
    }

    public override void Call()
    {
        callback(parent, (T1)parameters[0], (T2)parameters[1]);
    }
}

public class SGGenerator<T1, T2, T3> : SGGeneratorBase
{
    private Action<SGProdGen, T1, T2, T3> callback;

    public SGGenerator(string token, Action<SGProdGen, T1, T2, T3> callback) : base(token)
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
        var n = new SGGenerator<T1, T2, T3>(token, callback);
        n.parent = parent;
        n.depth = depth;
        parameters.CopyTo(n.parameters, 0);
        return n;
    }

    public override void Call()
    {
        callback(parent, (T1)parameters[0], (T2)parameters[1], (T3)parameters[2]);
    }
}