using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node<T>
{
    private T _data;
    private Node<T> _next;

    public Node(T t)
    {
        _data = t;
        _next = null;
    }

    public T Data
    {
        get { return _data; }
        set { _data = value; }
    }

    public Node<T> Next
    {
        get { return _next; }
        set { _next = value; }
    }
    
}
