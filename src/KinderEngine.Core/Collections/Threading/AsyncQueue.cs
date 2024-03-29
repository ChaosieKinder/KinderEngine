﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace KinderEngine.Core.Collections.Threading
{
    /// <summary>
    /// Thread safe Queue that allows waiting for more records during enumeration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncQueue<T> : IAsyncEnumerable<T>
    {
        private readonly SemaphoreSlim _enumerationSemaphore = new SemaphoreSlim(1);
        private readonly BufferBlock<T> _bufferBlock = new BufferBlock<T>();

        public void Enqueue(T item) =>
            _bufferBlock.Post(item);

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default)
        {
            // We lock this so we only ever enumerate once at a time.
            // That way we ensure all items are retvery tourned in a continuous
            // fashion with no 'holes' in the data when two foreach compete.
            await _enumerationSemaphore.WaitAsync();
            try
            {
                // Return new elements until cancellationToken is triggered.
                while (true)
                {
                    // Make sure to throw on cancellation so the Task will transfer into a canceled state
                    token.ThrowIfCancellationRequested();
                    yield return await _bufferBlock.ReceiveAsync(token);
                }
            }
            finally
            {
                _enumerationSemaphore.Release();
            }

        }
    }
}
