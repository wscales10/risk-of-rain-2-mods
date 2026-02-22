using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PactOfPunishment
{
    public class AssetPromise<T>
    {
        public AssetPromise(AsyncOperationHandle<T> asyncOperationHandle)
        {
            this.AsyncOperationHandle = asyncOperationHandle;
        }

        public AsyncOperationHandle<T> AsyncOperationHandle { get; }

        public T Value => this.AsyncOperationHandle.WaitForCompletion();

        public bool TryUse(Action<T> action)
        {
            if (this.AsyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                return false;
            }

            action(this.AsyncOperationHandle.Result);
            return true;
        }
    }
}