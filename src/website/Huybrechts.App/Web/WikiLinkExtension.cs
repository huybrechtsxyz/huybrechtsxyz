using Huybrechts.App.Features.Wiki.WikiInfoFlow;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;
using System.Text;

namespace Huybrechts.App.Web;

public class WikiLinkInline : LinkInline
{
    public string Namespace { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public WikiLinkInline()
    {
        // Setting the IsClosed flag to true so Markdig knows it's a closed element.
        IsClosed = true;
    }
}

public class WikiLinkParser : InlineParser
{
    private readonly string _urlPrefix;
    private readonly string _defaultNamespace;

    public WikiLinkParser(string urlPrefix, string defaultNamespace)
    {
        // Register the starting character for WikiLinks, which is '['
        OpeningCharacters = ['['];
        _urlPrefix = urlPrefix;
        _defaultNamespace = defaultNamespace;
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        // Ensure the input starts with '[['
        if (slice.CurrentChar != '[' || slice.PeekChar() != '[')
        {
            return false;
        }

        // Save the starting position for recovery in case of failure
        var start = slice.Start;
        slice.NextChar(); // Move past the first '['
        slice.NextChar(); // Move past the second '['

        // Find the closing ']]'
        var closingIndex = slice.Text.IndexOf("]]", slice.Start);
        if (closingIndex == -1)
        {
            // No closing brackets, return false to indicate this isn't a valid match
            slice.Start = start;
            return false;
        }

        // Extract the link text between the brackets
        var linkText = slice.Text[slice.Start..closingIndex];
        slice.Start = closingIndex + 2; // Move past the closing ']]'

        // Parse the link text into namespace, path, and optional link text
        var parts = linkText.Split(['|'], 2);
        var pathParts = parts[0].Split([':'], 2);

        string? linkNamespace = pathParts.Length > 1 ? pathParts[0] : null;
        string linkPath = pathParts.Length > 1 ? pathParts[1] : pathParts[0];
        string linkDisplayText = parts.Length > 1 ? parts[1] : linkPath;
        if (string.IsNullOrEmpty(linkDisplayText))
            linkDisplayText = linkPath;

        // Resolve this URL properly based on your wiki structure
        string wikiUrl = WikiInfoHelper.GetWikiUrl(
            _urlPrefix,
            _defaultNamespace,
            linkNamespace ?? string.Empty,
            linkPath);

        // Create a new WikiLinkInline and add it to the processor
        processor.Inline = new WikiLinkInline
        {
            Namespace = linkNamespace ?? string.Empty,
            Path = linkPath,
            Url = wikiUrl,
            Title = linkDisplayText
        };

        return true; // Indicate the match was successful
    }
}


public class WikiLinkRenderer : HtmlObjectRenderer<WikiLinkInline>
{
    protected override void Write(HtmlRenderer renderer, WikiLinkInline link)
    {
        renderer.Write("<a href=\"");
        renderer.Write(link.Url); // You can add logic here to resolve URLs from namespaces
        renderer.Write("\">");
        renderer.WriteEscape(link.Title);
        renderer.Write("</a>");
    }
}

public class WikiLinkExtension : IMarkdownExtension
{
    private readonly string _urlPrefix;
    private readonly string _defaultNamespace;

    public WikiLinkExtension(string urlPrefix, string defaultNamespace)
    {
        _urlPrefix = urlPrefix;
        _defaultNamespace = defaultNamespace;
    }

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.InlineParsers.Contains<WikiLinkParser>())
        {
            pipeline.InlineParsers.Insert(0, new WikiLinkParser(_urlPrefix, _defaultNamespace));
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            htmlRenderer.ObjectRenderers.Insert(0, new WikiLinkRenderer());
        }
    }
}