﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    internal static class ArrayEx
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static class SingletonArray<T> { public static readonly T[] Empty = new T[0]; }
        public static T[] Empty<T>() => SingletonArray<T>.Empty;
    }
}

namespace WarpWorld.CrowdControl
{
    internal static class Extensions
    {
        private static Task WaitAsync(this WaitHandle waitHandle)
        {
            if (waitHandle == null) { throw new ArgumentNullException(nameof(waitHandle)); }

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, delegate { tcs.TrySetResult(true); }, null, -1, true);
            Task result = tcs.Task;
            result.ContinueWith(t => rwh.Unregister(null));
            return result;
        }

        private static Task<bool> WaitAsync(this WaitHandle waitHandle, TimeSpan timeout)
        {
            if (waitHandle == null) { throw new ArgumentNullException(nameof(waitHandle)); }
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, delegate { tcs.TrySetResult(true); }, null, timeout.Milliseconds, true);
            Task<bool> result = tcs.Task;
            result.ContinueWith(t => rwh.Unregister(null));
            return result;
        }

#if (NET35 || NET40)
        public static async Task<IDisposable> UseWaitAsync(this SemaphoreSlim semaphore)
        {
            await semaphore.AvailableWaitHandle.WaitAsync();
            return new ReleaseWrapper(semaphore);
        }

#else
        public static async Task<IDisposable> UseWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancelToken)
        {
            await semaphore.WaitAsync(cancelToken).ConfigureAwait(false);
            return new ReleaseWrapper(semaphore);
        }
#endif


#if NET35
        private static bool HasFlag(this Enum variable, Enum value)
        {
            // check if from the same type.
            if (variable.GetType() != value.GetType())
            {
                throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
            }

            ulong num = Convert.ToUInt64(value);
            ulong num2 = Convert.ToUInt64(variable);

            return (num2 & num) == num;
        }
#endif

        /// <summary>
        /// Calls ConfigureAwait(false) on a task and logs any errors.
        /// </summary>
        /// <param name="task">The task to forget.</param>
        [DebuggerStepThrough]
        public static async void Forget(this Task task)
        {
            try { await task.ConfigureAwait(false); }
            catch (Exception ex) { CrowdControl.LogException(ex); }
        }

        internal static void MarshalTaskResults<TResult>(
            Task source, TaskCompletionSource<TResult> proxy)
        {
            switch (source.Status)
            {
                case TaskStatus.Faulted:
                    proxy.TrySetException(source.Exception);
                    break;
                case TaskStatus.Canceled:
                    proxy.TrySetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    Task<TResult> castedSource = source as Task<TResult>;
                    proxy.TrySetResult(
                        castedSource == null ? default(TResult) : // source is a Task
                            castedSource.Result); // source is a Task<TResult>
                    break;
            }
        }

        private class ReleaseWrapper : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private bool _disposed;

            public ReleaseWrapper(SemaphoreSlim semaphore) => _semaphore = semaphore;

            public void Dispose()
            {
                if (_disposed) { return; }
                _semaphore.Release();
                _disposed = true;
            }
        }
    }
}
