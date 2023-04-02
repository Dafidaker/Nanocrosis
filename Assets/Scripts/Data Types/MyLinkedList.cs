using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class LinkedList<T> : ICollection<T>, System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IReadOnlyCollection<T>, System.Collections.ICollection, System.Collections.IEnumerable, System.Runtime.Serialization.IDeserializationCallback, System.Runtime.Serialization.ISerializable
{
    public Node<T> First { get; set; }
    public Node<T> Last { get; set; }

    private bool Looped { get;  set; }
    private int _count;
    private int _count1;
    private int _count2;

    public LinkedList(bool isLooping)
    {
        First = null;
        Last = null;
        Looped = isLooping;
    }

    public void InsertAtBegin(T data)
    {
        Node<T> node = new Node<T>(data);
        node.Next = First;
        First = node;
        if (IsEmpty())
        {
            Last = node;
        }
        LoopList();
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        Node<T> current = First;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }
    
    public bool IsEmpty()
    {
        return First == null;
    }

    public Node<T> Search(T data)
    {
        Node<T> current = First;
        while (current != null)
        {
            if(current.Data.Equals(data)) return current;
            current = current.Next;
        }

        return null;
    }

    public void InsertAtEnd(T data)
    {
        Node<T> node = new Node<T>(data);
        if (IsEmpty())
        {
            First = node;
            Last = node;
            if (Looped)
            {
                First.Next = Last;
            }
        }
        else
        {
            /*Node<T> current = First;
            while (current.Next != null)
            {
                current = current.Next;
            }

            current.Next = node;
            Last = node;*/

            Last.Next = node;
            //node.Next = First;
            Last = node;
        }
        LoopList();
    }

    private void LoopList()
    {
        if (Looped)
        {
            Last.Next = First;
        }
    }

    public String ToString()
    {
        string str = "[ ";
        Node<T> current = First;
        bool hasLooped = false;
        if (Looped)
        {
            //add the first one 
            str += $"{Convert.ToString(current.Data)}";
            current = current.Next;
            if(current.Next != First) str +=   " ; " ;
            
            while (!hasLooped)
            {
                str += $"{Convert.ToString(current.Data)}";
                if(current.Next != First) str +=   " ; " ;
                
                current = current.Next;
                if (current == First)
                {
                    hasLooped = true;
                }
            }
            str += " ]";
            return str;
        }
        
        while (current != null)
        {
            str += $"{Convert.ToString(current.Data)}";
            if(current.Next != null) str +=   " ; " ;
            

            current = current.Next;
        }

        str += " ]";

        return str;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new NotImplementedException();
    }

    public void OnDeserialization(object sender)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void InsertAfter(T after, T data)
    {
        Node<T> afternode = Search(after);
        if (afternode == null) { return; }
        
        if (afternode == Last)
        {
            InsertAtEnd(data);
            return;
        }
        
        Node<T> node = new Node<T>(data);
        node.Next = afternode.Next;
        afternode.Next = node;
        LoopList();
        
    }
    
    public void InsertBefore(T before, T data)
    {
        /*if (_head.Data.Equals(before))
        {
            InsertAtBegin(data);
            return;
        }*/
        
        Node<T> newnode = new Node<T>(data);
        Node<T> current = First;
        Node<T> beforeCurrent = First;

        
        while ((current != null) && (!current.Data.Equals(before)))
        {
            Debug.Log("didnt jump");
            beforeCurrent = current; 
            current = current.Next;
        }

        if (current.Data.Equals(before))
        {
            First = newnode;
            newnode.Next = First;
        }
        else
        {
            beforeCurrent.Next = newnode;
            newnode.Next = current;
        }

    }

    public void Add(T item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    int ICollection.Count => _count2;

    public bool IsSynchronized { get; }
    public object SyncRoot { get; }

    int ICollection<T>.Count => _count;

    public bool IsReadOnly { get; }

    public void Remove(T data)
    {
        Node<T> current = First;
        Node<T> previous = null;
        
        while ((current != null) && (!current.Data.Equals(data)))
        {
            previous = current; 
            current = current.Next;
        }

        if (current != null)
        {
            if (previous == null)
            {
                First = current.Next;
            }
            else
            {
                previous.Next = current.Next;
            }
        }
        
    }

    /*public void InsertOrdered(T data)
    {
        Node<T> current = _head;
        Node<T> previous = null;
        Node<T> newnode = new Node<T>(data);
        
        while ((current != null) && 
               (current.Data.CompareTo(newnode.Data)  < 0 ) )
        {
            previous = current; 
            current = current.Next;
        }

        if (previous == null)
        {
            newnode.Next = _head;
            _head = newnode;
        }
        else
        {
            newnode.Next = current;
            previous.Next = newnode;
        }
    }*/

    public void Reverse()
    {
        LinkedList<T> reverseLinkedList = new LinkedList<T>(Looped);
        
        Node<T> current = First;
        while (current != null)
        {
            reverseLinkedList.InsertAtBegin(current.Data);
            current = current.Next;
        }

        First = reverseLinkedList.First;
    }

    int IReadOnlyCollection<T>.Count => _count1;
}
