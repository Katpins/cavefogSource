//https://github.com/Eldemarkki/Marching-Cubes-Terrain

using System;
using Unity.Jobs;
using Unity.Collections;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;

namespace caveFog
{
    public unsafe struct NativeCounter : IDisposable
    {

        private readonly Allocator _allocator;

        [NativeDisableUnsafePtrRestriction] private readonly int* _counter;


        public int Count{ get => *_counter; set => (*_counter) = value;}

        public NativeCounter(Allocator allocator)
        {
            _allocator = allocator;
            _counter = (int*)UnsafeUtility.Malloc(sizeof(int), 4, allocator);
            Count = 0;
        }


        public int Increment()
        {
            return Interlocked.Increment(ref *_counter) - 1;
        }

        public void Dispose()
        {
            UnsafeUtility.Free(_counter, _allocator);
        }
    }

}
