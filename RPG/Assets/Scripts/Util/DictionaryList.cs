using System;
using System.Collections.Generic;

public class DictionaryList<TKey, TValue>
{
    public Dictionary<TKey, TValue> Dictionary;
    public List<TKey> List;

    public int Count => List.Count;

    public TValue this[TKey key]
    {
        get { return Dictionary[key]; }
        set { Dictionary[key] = value; }
    }

    public TValue this[int index]
    {
        get { return Dictionary[List[index]]; }
        set { Dictionary[List[index]] = value; }
    }

    public DictionaryList()
    {
        Dictionary = new Dictionary<TKey, TValue>();
        List = new List<TKey>();
    }

    public void Add(TKey key, TValue value)
    {
        Dictionary.Add(key, value);
        List.Add(key);
    }

    public bool Contains(TKey key)
    {
        return Dictionary.ContainsKey(key);
    }

    public bool Contains(int index)
    {
        if (index < 0 || index > List.Count)
            return false;
        return Dictionary.ContainsKey(List[index]);
    }
}