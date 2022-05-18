﻿using System.Windows.Controls;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    public class TextWrapper : ControlWrapper<string, TextBox>
    {
        public TextWrapper(TextBox textBox) => UIElement = textBox;

        public override TextBox UIElement { get; }

        protected override void setValue(string value) => UIElement.Text = value;

        protected override SaveResult<string> tryGetValue(GetValueRequest request) => new(UIElement.Text);
    }
}