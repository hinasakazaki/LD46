using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter 
{
    bool real;
    string to;
    string from;
    string text;

    public Letter(bool real, string text)
    {
        this.real = real;
        this.text = text;
    }

    public void SetFrom(string from)
    {
        this.from = from;
    }
    public void SetTo(string to)
    {
        this.to = to;
    }

    public string ReadContent()
    {
        return text;
    }

    public string GetFrom()
    {
        return this.from;
    }
    public string GetTo()
    {
        return this.to;
    }
    
}
