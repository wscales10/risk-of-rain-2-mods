using System;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    public delegate void MyValidationEventHandler(object sender, MyValidationEventArgs e);

    public delegate void MyValidationEventHandler<TValue>(object sender, MyValidationEventArgs<TValue> e);

    public delegate void MyValidationEventHandler2<TValue>(object sender, MyValidationEventArgs2<TValue> e);

    public class MyValidationEventArgs : EventArgs
    {
        public MyValidationEventArgs(SaveResult saveResult) => SaveResult = saveResult;

        public SaveResult SaveResult { get; private set; }

        public void Extend(SaveResult saveResult) => SaveResult &= saveResult;
    }

    public class MyValidationEventArgs<T> : EventArgs
    {
        public MyValidationEventArgs(SaveResult<T> saveResult) => SaveResult = saveResult;

        public SaveResult<T> SaveResult { get; private set; }

        public void Extend(SaveResult saveResult) => SaveResult &= saveResult;
    }

    public class MyValidationEventArgs2<TValue> : EventArgs
    {
        public MyValidationEventArgs2(TValue value) => Value = value;

        public TValue Value { get; }

        public bool Status { get; private set; } = true;

        public void Invalidate() => Status = false;
    }
}