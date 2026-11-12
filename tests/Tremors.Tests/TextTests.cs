using System.Globalization;
using Xunit;

namespace Tremors.Tests;

public class TextTests
{
    [Theory]
    [InlineData("en-US", "Settings…")]
    [InlineData("en-GB", "Settings…")]
    [InlineData("nl-NL", "Instellingen…")]
    [InlineData("nl-BE", "Instellingen…")]
    [InlineData("fr-FR", "Paramètres…")]
    [InlineData("fr-BE", "Paramètres…")]
    [InlineData("de-DE", "Einstellungen…")]
    [InlineData("de-CH", "Einstellungen…")]
    [InlineData("es-ES", "Settings…")] // not translated yet: English
    public void Follows_the_Windows_display_language(string culture, string settings)
    {
        Assert.Equal(settings, Text.Pick(new CultureInfo(culture)).Settings);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("nl")]
    [InlineData("fr")]
    [InlineData("de")]
    public void Every_text_is_filled_in(string culture)
    {
        var language = Text.Pick(new CultureInfo(culture));

        foreach (var property in language.GetType().GetProperties())
            Assert.False(string.IsNullOrWhiteSpace((string?)property.GetValue(language)), property.Name);
    }

    [Fact]
    public void Chosen_language_can_be_switched_and_reset()
    {
        try
        {
            Text.Use("fr");
            Assert.Equal("fr", Text.CurrentCode);
            Assert.Equal("Terminé", Text.Done);

            Assert.Equal("novembre 2026", Text.Month(new DateTime(2026, 11, 1)));

            Text.Use("de");
            Assert.Equal("Fertig", Text.Done);
            Assert.Equal("November 2026", Text.Month(new DateTime(2026, 11, 1)));

            Text.Use("nl");
            Assert.Equal("november 2026", Text.Month(new DateTime(2026, 11, 1)));

            Text.Use("xx"); // unknown: follow Windows again
            Assert.Equal(Text.Pick(CultureInfo.CurrentUICulture).Code, Text.CurrentCode);
        }
        finally
        {
            Text.Use(null);
        }
    }

    [Fact]
    public void Every_language_has_a_flag_and_a_unique_code()
    {
        Assert.Equal(Text.All.Count, Text.All.Select(l => l.Code).Distinct().Count());

        foreach (var language in Text.All)
        {
            using var flag = Flags.Draw(language.Code, 30, 20);
            Assert.Equal(new Size(30, 20), flag.Size);
        }
    }
}
