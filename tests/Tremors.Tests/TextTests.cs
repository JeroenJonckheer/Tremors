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
}
