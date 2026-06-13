using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace Rhizine.AvalonEdit.MarkdownEditorControl;

public class MarkdownEditorBehavior : Behavior<TextEditor>
{
    public static readonly DependencyProperty TransformerProperty =
        DependencyProperty.Register("Transformer", typeof(MarkdownColorizingTransformer), typeof(MarkdownEditorBehavior),
            new PropertyMetadata(null, OnTransformerChanged));

    public MarkdownColorizingTransformer Transformer
    {
        get { return (MarkdownColorizingTransformer)GetValue(TransformerProperty); }
        set { SetValue(TransformerProperty, value); }
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (Transformer != null)
        {
            AssociatedObject.TextArea.TextView.LineTransformers.Add(Transformer);
        }
    }

    protected override void OnDetaching()
    {
        if (Transformer != null)
        {
            AssociatedObject.TextArea.TextView.LineTransformers.Remove(Transformer);
        }
        base.OnDetaching();
    }

    private static void OnTransformerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (MarkdownEditorBehavior)d;
        var oldTransformer = e.OldValue as MarkdownColorizingTransformer;
        var newTransformer = e.NewValue as MarkdownColorizingTransformer;

        if (behavior.AssociatedObject != null)
        {
            if (oldTransformer != null)
            {
                behavior.AssociatedObject.TextArea.TextView.LineTransformers.Remove(oldTransformer);
            }
            if (newTransformer != null)
            {
                behavior.AssociatedObject.TextArea.TextView.LineTransformers.Add(newTransformer);
            }
        }
    }
}