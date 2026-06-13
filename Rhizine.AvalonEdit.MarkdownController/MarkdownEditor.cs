using ICSharpCode.AvalonEdit;
using System.Windows;

namespace Rhizine.AvalonEdit.MarkdownEditorControl;

public class MarkdownEditor : TextEditor
{
    public static readonly DependencyProperty IsMarkdownHiddenProperty =
        DependencyProperty.Register(nameof(IsMarkdownHidden), typeof(bool), typeof(MarkdownEditor),
            new PropertyMetadata(true, OnIsMarkdownHiddenChanged));

    public bool IsMarkdownHidden
    {
        get { return (bool)GetValue(IsMarkdownHiddenProperty); }
        set { SetValue(IsMarkdownHiddenProperty, value); }
    }

    private readonly MarkdownColorizingTransformer _transformer;

    static MarkdownEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MarkdownEditor),
            new FrameworkPropertyMetadata(typeof(MarkdownEditor)));
    }

    public MarkdownEditor()
    {
        _transformer = new MarkdownColorizingTransformer();
        TextArea.TextView.LineTransformers.Add(_transformer);

        SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("Markdown");
    }

    private static void OnIsMarkdownHiddenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (MarkdownEditor)d;
        editor._transformer.HideMarkdown = (bool)e.NewValue;
        editor.TextArea.TextView.Redraw();
    }
}