using System.Collections.Generic;

namespace KnightOfNights.Scripts.InternalLib;

internal class CompactIdGen
{
    private readonly Queue<int> queue = [];
    private int next;

    public int Acquire() => queue.Count == 0 ? next++ : queue.Dequeue();

    public void Release(int id)
    {
        queue.Enqueue(id);
        if (queue.Count == next)
        {
            queue.Clear();
            next = 0;
        }
    }
}
