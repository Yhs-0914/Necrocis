using System;
using System.Collections.Generic;

namespace Necrocis
{
    // Synchronous main-thread scratch. Each nested call gets a distinct slot.
    // A copied stale lease cannot return a slot that has since been rented again.
    public readonly struct ScratchLease<T> : IDisposable
    {
        private sealed class Slot
        {
            internal T[] Items=Array.Empty<T>();
            internal ulong Version;
            internal bool InUse;
        }
        private const int MaximumRetainedSlots=32, MaximumRetainedCapacity=4096;
        private static readonly Stack<Slot> available=new Stack<Slot>();
        private readonly Slot slot;
        private readonly ulong version;
        public int Length { get; }
        private ScratchLease(Slot slot,int length)
        {this.slot=slot;version=slot.Version;Length=length;}
        public static ScratchLease<T> Rent(int length)
        {
            if(length<0)throw new ArgumentOutOfRangeException(nameof(length));
            if(length==0)return default;
            Slot slot=available.Count>0?available.Pop():new Slot();
            if(slot.Items.Length<length)
            {
                int capacity=Math.Max(1,slot.Items.Length);
                while(capacity<length)capacity=checked(capacity*2);
                slot.Items=new T[capacity];
            }
            unchecked{slot.Version++;}slot.InUse=true;
            return new ScratchLease<T>(slot,length);
        }
        private void Check(int index)
        {
            if(slot==null||!slot.InUse||slot.Version!=version)throw new ObjectDisposedException(nameof(ScratchLease<T>));
            if((uint)index>=(uint)Length)throw new ArgumentOutOfRangeException(nameof(index));
        }
        // Only the first Length entries are valid. Keep this array inside the owning
        // synchronous scope, as with ListLease.Items; never return it from a hit callback.
        public T[] Buffer
        {
            get
            {
                if(Length==0)return Array.Empty<T>();
                if(slot==null||!slot.InUse||slot.Version!=version)throw new ObjectDisposedException(nameof(ScratchLease<T>));
                return slot.Items;
            }
        }
        public T this[int index]
        {
            get{Check(index);return slot.Items[index];}
            set{Check(index);slot.Items[index]=value;}
        }
        public void Dispose()
        {
            if(slot==null||!slot.InUse||slot.Version!=version)return;
            Array.Clear(slot.Items,0,Length);slot.InUse=false;
            if(slot.Items.Length<=MaximumRetainedCapacity && available.Count<MaximumRetainedSlots)available.Push(slot);
        }
    }

    public readonly struct ListLease<T> : IDisposable
    {
        private sealed class Slot
        {
            internal readonly List<T> Items=new List<T>(64);
            internal ulong Version;
            internal bool InUse;
        }
        private const int MaximumRetainedSlots=32, MaximumRetainedCapacity=4096;
        private static readonly Stack<Slot> available=new Stack<Slot>();
        private readonly Slot slot;
        private readonly ulong version;
        private ListLease(Slot slot){this.slot=slot;version=slot.Version;}
        public static ListLease<T> Rent()
        {
            Slot slot=available.Count>0?available.Pop():new Slot();
            unchecked{slot.Version++;}slot.InUse=true;return new ListLease<T>(slot);
        }
        // Do not retain the list past the owning synchronous invocation.
        public List<T> Items
        {
            get
            {
                if(slot==null||!slot.InUse||slot.Version!=version)throw new ObjectDisposedException(nameof(ListLease<T>));
                return slot.Items;
            }
        }
        public void Dispose()
        {
            if(slot==null||!slot.InUse||slot.Version!=version)return;
            slot.Items.Clear();slot.InUse=false;
            if(slot.Items.Capacity<=MaximumRetainedCapacity && available.Count<MaximumRetainedSlots)available.Push(slot);
        }
    }
}
