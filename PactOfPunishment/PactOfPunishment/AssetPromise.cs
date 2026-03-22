using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PactOfPunishment
{
    public interface IAssetPromise<out T>
    {
        T Value { get; }

        bool TryUse(Action<T> action);
    }

    public class AssetPromise<T> : IAssetPromise<T>
    {
        private readonly AsyncOperationHandle<T> asyncOperationHandle;

        public AssetPromise(AsyncOperationHandle<T> asyncOperationHandle)
        {
            this.asyncOperationHandle = asyncOperationHandle;
        }

        public T Value => this.asyncOperationHandle.WaitForCompletion();

        public bool TryUse(Action<T> action)
        {
            if (this.asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                return false;
            }

            action(this.asyncOperationHandle.Result);
            return true;
        }
    }
}