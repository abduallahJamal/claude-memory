using ClaudeMemory.Core.Model;
using ClaudeMemory.Core.Text;

namespace ClaudeMemory.Core.Tests;

/// <summary>
/// PRD §9.29: an extremely large document is clipped so the window stays responsive, but the
/// user is always told, and the counts must not follow the machine's culture.
/// </summary>
public sealed class OutputClipperTests
{
    [Fact]
    public void Small_output_is_returned_untouched()
    {
        const string text = "hello world";

        Assert.False(OutputClipper.NeedsClip(text, 32));
        Assert.Equal(text, OutputClipper.ClipOutput(text, 32));
    }

    [Fact]
    public void Output_at_exactly_the_cap_is_not_clipped()
    {
        var text = new string('a', 64);

        Assert.Equal(text, OutputClipper.ClipOutput(text, 64));
    }

    [Fact]
    public void Output_over_the_cap_keeps_the_head_and_reports_the_omitted_count()
    {
        var tail = "TAIL-MARKER";
        var text = new string('a', 100) + tail;

        var clipped = OutputClipper.ClipOutput(text, 100);

        Assert.StartsWith(new string('a', 100), clipped, StringComparison.Ordinal);
        Assert.DoesNotContain(tail, clipped, StringComparison.Ordinal);
        Assert.Equal(
            new string('a', 100) + "\n\n---\n" + ExpectedNotice.Output(tail.Length, 100),
            clipped);
        Assert.Contains("The source is complete", clipped, StringComparison.Ordinal);
    }

    [Fact]
    public void A_zero_cap_means_unlimited()
    {
        var text = new string('a', 10_000);

        Assert.False(OutputClipper.NeedsClip(text, 0));
        Assert.Equal(text, OutputClipper.ClipOutput(text, 0));
    }

    [Fact]
    public void The_omitted_count_is_formatted_with_the_invariant_culture()
    {
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            // A culture whose group separator is not a comma would otherwise leak into output.
            System.Globalization.CultureInfo.CurrentCulture =
                new System.Globalization.CultureInfo("de-DE");

            var clipped = OutputClipper.ClipOutput(new string('a', 4000), 1000);

            Assert.Contains("3,000 characters omitted", clipped, StringComparison.Ordinal);
            Assert.Equal(ExpectedNotice.ClipOutput(new string('a', 4000), 1000), clipped);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Null_input_is_treated_as_empty()
    {
        Assert.Equal(string.Empty, OutputClipper.ClipOutput(null, 10));
        Assert.Equal(string.Empty, OutputClipper.ClipPreview(null, 10));
    }

    [Fact]
    public void Preview_clipping_appends_a_markdown_notice()
    {
        var markdown = new string('a', 32);

        var clipped = OutputClipper.ClipPreview(markdown, 10);

        Assert.Equal(ExpectedNotice.ClipPreview(markdown, 10), clipped);
        Assert.Contains("_Preview clipped: 22 characters omitted", clipped, StringComparison.Ordinal);
        Assert.Contains("The source is complete", clipped, StringComparison.Ordinal);
    }

    [Fact]
    public void The_defaults_match_the_documented_caps()
    {
        Assert.Equal(4 * 1024 * 1024, OutputClipper.DefaultOutputChars);
        Assert.Equal(2 * 1024 * 1024, OutputClipper.DefaultPreviewChars);
        Assert.Equal(OutputClipper.DefaultOutputChars,
            new MemoryOptions().MaxOutputChars);
    }
}
