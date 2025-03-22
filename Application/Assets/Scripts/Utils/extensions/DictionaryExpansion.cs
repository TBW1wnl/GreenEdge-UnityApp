using System;
using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, params (TKey key, TValue value)[] items)
    {
        foreach (var (key, value) in items)
        {
            dictionary[key] = value;
        }
    }
}
