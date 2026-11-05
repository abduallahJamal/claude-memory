using System.Globalization;

namespace ClaudeMemory.Core.Text;

/// <summary>
/// Clips generated text that grows past a configured cap (PRD §9.29) so the window and the
/// preview stay responsive on very large sessions.
/// </summary>
/// <remarks>
/// <para>
/// The reference rendered whatever the transcript produced; PRD §9.29 is an intentional
/// difference that only matters for pathological inputs. The notice is part of the clipped
/// text, so the user always learns that characters were dropped, that the source is complete,
/// and how to ask for all of it. Nothing is ever dropped silently.
/// </para>
/// <para>
/// Counts are formatted with the invariant culture: the golden files and the tests must not
/// depend on the machine's locale.
/// </para>
/// </remarks>
public static class OutputClipper
{
    /// <summary>Default cap for generated output (4 Mi characters, about 8 MiB of UTF-16).</summary>
    public const int DefaultOutputChars = 4 * 1024 * 1024;

    /// <summary>Default cap for the Markdown handed to the preview (2 Mi characters).</summary>
    public const int DefaultPreviewChars = 2 * 1024 * 1024;

    /// <summary>True when <paramref name="maxChars"/> would clip <paramref name="text"/>.</summary>
    public static bool NeedsClip(string? text, int maxChars)
        => maxChars > 0 && text is not null && text.Length > maxChars;

    /// <summary>
    /// Truncates generated output to <paramref name="maxChars"/> and appends the visible
    /// <c>--max-output-chars</c> notice. A non-positive cap means unlimited.
    /// </summary>
    public static string ClipOutput(string? text, int maxChars)
    {
        text ??= string.Empty;
        if (!NeedsClip(text, maxChars)) return text;

        var kept = text[..maxChars];
        var notice = string.Format(
            CultureInfo.InvariantCulture,
            "_Output clipped: {0:N0} characters omitted to stay inside --max-output-chars "
            + "({1:N0}). The source is complete; raise --max-output-chars or re-run with "
            + "--mode transcript for the full text._\n",
            text.Length - maxChars,
            maxChars);
        return kept + "\n\n---\n" + notice;
    }

    /// <summary>
    /// Truncates the Markdown handed to the preview and appends a visible notice that renders
    /// through the normal (raw-HTML-disabled) pipeline. A non-positive cap means unlimited.
    /// </summary>
    public static string ClipPreview(string? markdown, int maxChars)
    {
        markdown ??= string.Empty;
        if (!NeedsClip(markdown, maxChars)) return markdown;

        var kept = markdown[..maxChars];
        var notice = string.Format(
            CultureInfo.InvariantCulture,
            "_Preview clipped: {0:N0} characters omitted from the rendered preview to keep "
            + "the window responsive. The source is complete; open the source view or save "
            + "the file to read it all._\n",
            markdown.Length - maxChars);
        return kept + "\n\n---\n\n" + notice;
    }
}
