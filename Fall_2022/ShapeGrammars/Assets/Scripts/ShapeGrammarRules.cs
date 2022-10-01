using System;
using System.Collections.Generic;

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

public class SGRule : SGObj
{
    public SGRule parent; // parent rule in production
    public static int maxOper;


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

    public virtual void Call(int maxDepth)
    {
        throw new NotImplementedException();
    }
}

public class SGProducer : SGRule
{
    public List<SGRule> rules;

    public SGProducer(string token) : base(token)
    {
        rules = new List<SGRule>();
    }

    public SGProducer(SGProducer other) : base(other)
    {
        this.rules = other.rules;
    }

    public override SGRule Copy()
    {
        return new SGProducer(this.token);
    }

    public override void Call(int maxDepth)
    {
        if (maxDepth >= 0 && maxOper >= 0)
        {
            foreach (SGRule rule in rules)
                rule.Call(maxDepth - 1);
        }
    }
}

public class SGProdGen : SGRule
{
    public Func<SGProducer> callback;
    private SGProducer prod;

    public SGProdGen(string token, Func<SGProducer> callback) : base(token)
    {
        this.callback = callback;
    }

    public SGProdGen(SGProdGen other) : base(other)
    {
        this.callback = other.callback;
    }

    public override SGRule Copy()
    {
        return new SGProdGen(this.token, this.callback);
    }

    public override void Call(int maxDepth)
    {
        if (maxOper <= 0)
            return;

        if (prod == null)
            prod = callback();
        
        prod.Call(maxDepth);
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

    public override void Call(int maxDepth)
    {
        if (maxOper >= 0)
            callback();
        maxOper--;
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

    public override void Call(int maxDepth)
    {
        if (maxOper >= 0)
            callback((T1)parameters[0]);
        maxOper--;
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

    public override void Call(int maxDepth)
    {
        if (maxOper >= 0)
            callback((T1)parameters[0], (T2)parameters[1]);
        maxOper--;
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

    public override void Call(int maxDepth)
    {
        if (maxOper >= 0)
            callback((T1)parameters[0], (T2)parameters[1], (T3)parameters[2]);
        maxOper--;
    }
}