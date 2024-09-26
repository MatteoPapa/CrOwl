using ICSharpCode.AvalonEdit;
using System;
using System.Windows;

namespace Crowl_Alpha.View.Behaviors
{
    public static class TextEditorBehavior
    {
        public static readonly DependencyProperty BoundTextProperty =
            DependencyProperty.RegisterAttached(
                "BoundText",
                typeof(string),
                typeof(TextEditorBehavior),
                new PropertyMetadata(default(string), OnBoundTextChanged));

        public static void SetBoundText(DependencyObject element, string value)
        {
            element.SetValue(BoundTextProperty, value);
        }

        public static string GetBoundText(DependencyObject element)
        {
            return (string)element.GetValue(BoundTextProperty);
        }

        private static void OnBoundTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor textEditor)
            {
                textEditor.TextChanged -= OnTextEditorTextChanged;
                textEditor.Text = (string)e.NewValue;
                textEditor.TextChanged += OnTextEditorTextChanged;
            }
        }

        private static void OnTextEditorTextChanged(object sender, EventArgs e)
        {
            if (sender is TextEditor textEditor)
            {
                SetBoundText(textEditor, textEditor.Text);
            }
        }
    }

}
