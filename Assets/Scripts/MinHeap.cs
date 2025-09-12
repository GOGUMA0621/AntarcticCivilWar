// MinHeap.cs
using System;
using System.Collections.Generic;

public class MinHeap<T> where T : IComparable<T>
{
    private List<T> heap = new();

    public int Count => heap.Count;

    public void Add(T item)
    {
        heap.Add(item);
        int i = heap.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[i].CompareTo(heap[parent]) >= 0) break;
            (heap[i], heap[parent]) = (heap[parent], heap[i]);
            i = parent;
        }
    }

    public T RemoveMin()
    {
        if (heap.Count == 0) throw new InvalidOperationException("Heap is empty");
        T min = heap[0];
        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);
        int i = 0;
        while (true)
        {
            int left = 2 * i + 1, right = 2 * i + 2, smallest = i;
            if (left < heap.Count && heap[left].CompareTo(heap[smallest]) < 0) smallest = left;
            if (right < heap.Count && heap[right].CompareTo(heap[smallest]) < 0) smallest = right;
            if (smallest == i) break;
            (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
            i = smallest;
        }
        return min;
    }

    public bool Contains(T item) => heap.Contains(item);
}