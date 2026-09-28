using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static T Draw<T>(this List<T> list)
    {
        if (list.Count == 0) return default;
        int r = UnityEngine.Random.Range(0, list.Count);
        T t = list[r];
        list.Remove(t);
        return t;
    }

    public static T RandomItem<T>(this List<T> list)
    {
        if (list.Count == 0)
            throw new IndexOutOfRangeException("List is Empty");

        var randomIndex = UnityEngine.Random.Range(0, list.Count);
        return list[randomIndex];
    }
}
