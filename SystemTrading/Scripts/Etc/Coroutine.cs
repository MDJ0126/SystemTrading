using System;
using System.Collections;
using System.Collections.Generic;
public class Coroutine
{
    public string name;
    public IEnumerator enumerator;

    public Coroutine(string name, IEnumerator enumerator)
    {
        this.name = name;
        this.enumerator = enumerator;
    }
}
