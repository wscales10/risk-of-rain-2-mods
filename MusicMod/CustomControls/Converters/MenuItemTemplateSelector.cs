using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CustomControls.Converters
{
    // Selects template appropriate for CLR/XML item in order to display string property at
    // DisplayMemberPath on the item.
    public class DisplayMemberTemplateSelector : DataTemplateSelector
    {
        private DataTemplate xmlNodeContentTemplate;

        private DataTemplate clrNodeContentTemplate;

        private string displayMemberPath;

        public string DisplayMemberPath
        {
            get => displayMemberPath;

            set
            {
                if (displayMemberPath != value)
                {
                    displayMemberPath = value;
                    xmlNodeContentTemplate = null;
                    clrNodeContentTemplate = null;
                }
            }
        }

        /// <summary>
        /// Override this method to return an app specific <seealso cref="DataTemplate"/>.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The container in which the content is to be displayed</param>
        /// <returns>a app specific template to apply.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (false/*System.Xml.IsXmlNode(item)*/)
            {
                if (xmlNodeContentTemplate == null)
                {
                    FrameworkElementFactory text = new(typeof(TextBlock));
                    Binding binding = new() { XPath = DisplayMemberPath };
                    text.SetBinding(TextBlock.TextProperty, binding);
                    xmlNodeContentTemplate = new DataTemplate { VisualTree = text };
                    xmlNodeContentTemplate.Seal();
                }
                return xmlNodeContentTemplate;
            }
            else
            {
                if (clrNodeContentTemplate == null)
                {
                    FrameworkElementFactory text = new(typeof(TextBlock));
                    Binding binding = new() { Path = new PropertyPath(DisplayMemberPath, Array.Empty<object>()) };
                    text.SetBinding(TextBlock.TextProperty, binding);
                    clrNodeContentTemplate = new DataTemplate { VisualTree = text };
                    clrNodeContentTemplate.Seal();
                }
                return clrNodeContentTemplate;
            }
        }
    }
}