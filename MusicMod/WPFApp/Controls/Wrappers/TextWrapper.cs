using System;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    public class TextWrapper : ControlWrapper<string, TextBox>
    {
        private Func<string, bool> validator;

        public TextWrapper(TextBox textBox) => UIElement = textBox;

        public override TextBox UIElement { get; }

        public TextWrapper WithValidation(Func<string, bool> validator)
        {
            if (this.validator is not null)
            {
                throw new NotImplementedException();
            }

            this.validator = validator;
            return this;
        }

        protected override void setValue(string value) => UIElement.Text = value;

        protected override SaveResult<string> tryGetValue(GetValueRequest request) => new(UIElement.Text);

        protected override bool Validate(string value)
        {
            return validator is null ? base.Validate(value) : validator(value);
        }
    }
}