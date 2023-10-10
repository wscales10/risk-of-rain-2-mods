using Rules.RuleTypes.Mutable;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    internal class BucketWrapper<TContext, TOut, TControl> : ControlWrapper<Bucket<TContext, TOut>, TControl>
        where TControl : FrameworkElement
    {
        private readonly ControlWrapper<TOut, TControl> innerWrapper;

        private Bucket<TContext, TOut> bucket;

        public BucketWrapper(ControlWrapper<TOut, TControl> innerWrapper)
        {
            this.innerWrapper = innerWrapper;
        }

        public override TControl UIElement => innerWrapper.UIElement;

        protected override void setValue(Bucket<TContext, TOut> value)
        {
            innerWrapper.SetValue((bucket = value).Output);
        }

        protected override SaveResult<Bucket<TContext, TOut>> tryGetValue(GetValueRequest request)
        {
            var result1 = innerWrapper.TryGetValue(request);

            if (result1)
            {
                bucket.Output = result1.Value;
            }

            return new(result1, bucket);
        }
    }
}