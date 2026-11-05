using System.Globalization;

namespace ClaudeMemory.Core.Tests;

/// <summary>
/// Re-implements the two format strings in <c>OutputClipper</c> so the expected counts can be
/// checked on machines without a .NET SDK. If the source wording changes, this helper must
/// change with it; the CliRunner and App tests cover the same text end to end.
/// </summary>
internal static class ExpectedNotice
{
    public static string Output(int omitted, int cap)
        => string.Format(
            CultureInfo.InvariantCulture,
            "_Output clipped: {0:N0} characters omitted to stay inside --max-output-chars "
            + "({1:N0}). The source is complete; raise --max-output-chars or re-run with "
            + "--mode transcript for the full text._\n",
            omitted,
            cap);

    public static string Preview(int omitted)
        => string.Format(
            CultureInfo.InvariantCulture,
            "_Preview clipped: {0:N0} characters omitted from the rendered preview to keep "
            + "the window responsive. The source is complete; open the source view or save "
            + "the file to read it all._\n",
            omitted);

    public static string ClipOutput(string text, int cap)
    {
        if (cap <= 0 || text.Length <= cap) return text;
        return text[..cap] + "\n\n---\n" + Output(text.Length - cap, cap);
    }

    public static string ClipPreview(string markdown, int cap)
    {
        if (cap <= 0 || markdown.Length <= cap) return markdown;
        return markdown[..cap] + "\n\n---\n\n" + Preview(markdown.Length - cap);
    }
}
