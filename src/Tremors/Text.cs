using System.Globalization;

namespace Tremors;

/// <summary>
/// All user-facing text, in the display language of Windows: English, Dutch, French
/// or German. Any other language falls back to English.
/// To add a language: add a field to <see cref="Language"/> and a case in <see cref="Pick"/>.
/// </summary>
internal static class Text
{
    internal sealed record Language(
        string Active,
        string Settings,
        string StartWithWindows,
        string Quit,
        string WelcomeTitle,
        string WelcomeBody,
        string HookFailed,
        string TooltipOn,
        string TooltipOff,
        string IsOn,
        string Strength,
        string StrengthHint,
        string SteadyWheel,
        string Done);

    private static readonly Language English = new(
        Active: "Active",
        Settings: "Settings…",
        StartWithWindows: "Start with Windows",
        Quit: "Quit Tremors",
        WelcomeTitle: "Tremors is on",
        WelcomeBody: "Your mouse is now kept steady. Click this icon to change the settings.",
        HookFailed: "Cannot follow the mouse: ",
        TooltipOn: "Tremors: on (strength {0})",
        TooltipOff: "Tremors: off",
        IsOn: "Tremors is on",
        Strength: "Strength: {0}",
        StrengthHint: "0 = off. Higher = steadier, but the mouse responds a little slower.",
        SteadyWheel: "Steady scroll wheel",
        Done: "Done");

    private static readonly Language Dutch = new(
        Active: "Actief",
        Settings: "Instellingen…",
        StartWithWindows: "Starten met Windows",
        Quit: "Tremors stoppen",
        WelcomeTitle: "Tremors staat aan",
        WelcomeBody: "De muis wordt nu stabiel gehouden. Klik op dit icoon voor de instellingen.",
        HookFailed: "Kan de muis niet volgen: ",
        TooltipOn: "Tremors: aan (sterkte {0})",
        TooltipOff: "Tremors: uit",
        IsOn: "Tremors staat aan",
        Strength: "Sterkte: {0}",
        StrengthHint: "0 = uit. Hoger = rustiger, maar de muis reageert iets trager.",
        SteadyWheel: "Scrollwiel stabiel houden",
        Done: "Klaar");

    private static readonly Language French = new(
        Active: "Activé",
        Settings: "Paramètres…",
        StartWithWindows: "Démarrer avec Windows",
        Quit: "Quitter Tremors",
        WelcomeTitle: "Tremors est activé",
        WelcomeBody: "La souris est maintenant stabilisée. Cliquez sur cette icône pour modifier les paramètres.",
        HookFailed: "Impossible de suivre la souris : ",
        TooltipOn: "Tremors : activé (intensité {0})",
        TooltipOff: "Tremors : désactivé",
        IsOn: "Tremors est activé",
        Strength: "Intensité : {0}",
        StrengthHint: "0 = désactivé. Plus élevé = plus stable, mais la souris réagit un peu plus lentement.",
        SteadyWheel: "Stabiliser la molette",
        Done: "Terminé");

    private static readonly Language German = new(
        Active: "Aktiv",
        Settings: "Einstellungen…",
        StartWithWindows: "Mit Windows starten",
        Quit: "Tremors beenden",
        WelcomeTitle: "Tremors ist aktiv",
        WelcomeBody: "Die Maus wird jetzt ruhig gehalten. Klicken Sie auf dieses Symbol, um die Einstellungen zu ändern.",
        HookFailed: "Die Maus kann nicht verfolgt werden: ",
        TooltipOn: "Tremors: an (Stärke {0})",
        TooltipOff: "Tremors: aus",
        IsOn: "Tremors ist aktiv",
        Strength: "Stärke: {0}",
        StrengthHint: "0 = aus. Höher = ruhiger, aber die Maus reagiert etwas langsamer.",
        SteadyWheel: "Mausrad stabilisieren",
        Done: "Fertig");

    private static readonly Language Current = Pick(CultureInfo.CurrentUICulture);

    internal static Language Pick(CultureInfo culture) => culture.TwoLetterISOLanguageName switch
    {
        "nl" => Dutch,
        "fr" => French,
        "de" => German,
        _ => English,
    };

    public static string Active => Current.Active;
    public static string Settings => Current.Settings;
    public static string StartWithWindows => Current.StartWithWindows;
    public static string Quit => Current.Quit;
    public static string WelcomeTitle => Current.WelcomeTitle;
    public static string WelcomeBody => Current.WelcomeBody;
    public static string HookFailed => Current.HookFailed;
    public static string TooltipOff => Current.TooltipOff;
    public static string IsOn => Current.IsOn;
    public static string StrengthHint => Current.StrengthHint;
    public static string SteadyWheel => Current.SteadyWheel;
    public static string Done => Current.Done;

    public static string TooltipOn(int strength) => string.Format(Current.TooltipOn, strength);
    public static string Strength(int strength) => string.Format(Current.Strength, strength);
}
