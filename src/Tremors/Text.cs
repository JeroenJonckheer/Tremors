using System.Globalization;

namespace Tremors;

/// <summary>
/// All user-facing text in English, Dutch, French or German. By default the display
/// language of Windows is used (any other language falls back to English); the user can
/// choose another one with the flags in the settings window.
/// To add a language: add a <see cref="Language"/>, list it in <see cref="All"/> and add a
/// flag in <see cref="Flags"/>.
/// </summary>
internal static class Text
{
    internal sealed record Language(
        string Code,
        string Name,
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
        string PointerSpeed,
        string PointerSpeedHint,
        string StillZone,
        string StillZoneHint,
        string ClickHold,
        string ClickHoldHint,
        string SteadyWheel,
        string Done,
        string About,
        string MadeBy,
        string Free,
        string Latest);

    private static readonly Language English = new(
        Code: "en",
        Name: "English",
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
        PointerSpeed: "Pointer speed: {0}%",
        PointerSpeedHint: "Lower = the tremor looks smaller on screen, but you move the mouse further. Windows mouse acceleration is off while Tremors is on.",
        StillZone: "Still zone: {0} px",
        StillZoneHint: "The cursor stays put until the hand moves further than this. Larger absorbs bigger jerks.",
        ClickHold: "Hold to click: {0} ms",
        ClickHoldHint: "A click only counts when the button is held this long; shorter taps are ignored. 0 = off. Try 100-200 ms if clicks happen by accident.",
        SteadyWheel: "Steady scroll wheel",
        Done: "Done",
        About: "About Tremors",
        MadeBy: "Made by {0}",
        Free: "Tremors is free, and always will be.",
        Latest: "Get the latest version");

    private static readonly Language Dutch = new(
        Code: "nl",
        Name: "Nederlands",
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
        PointerSpeed: "Muissnelheid: {0}%",
        PointerSpeedHint: "Lager = de trilling wordt kleiner op het scherm, maar u beweegt de muis verder. De muisversnelling van Windows staat uit zolang Tremors aanstaat.",
        StillZone: "Stilstandzone: {0} px",
        StillZoneHint: "De cursor blijft staan tot de hand verder beweegt dan dit. Groter vangt grotere schokken op.",
        ClickHold: "Vasthouden om te klikken: {0} ms",
        ClickHoldHint: "Een klik telt pas als de knop zo lang wordt ingedrukt; kortere tikjes worden genegeerd. 0 = uit. Probeer 100-200 ms als er per ongeluk geklikt wordt.",
        SteadyWheel: "Scrollwiel stabiel houden",
        Done: "Klaar",
        About: "Over Tremors",
        MadeBy: "Gemaakt door {0}",
        Free: "Tremors is gratis en dat blijft altijd zo.",
        Latest: "Nieuwste versie ophalen");

    private static readonly Language French = new(
        Code: "fr",
        Name: "Français",
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
        PointerSpeed: "Vitesse du pointeur : {0} %",
        PointerSpeedHint: "Plus bas = le tremblement paraît plus petit à l’écran, mais il faut déplacer la souris davantage. L’accélération de la souris de Windows est désactivée tant que Tremors est activé.",
        StillZone: "Zone d’immobilité : {0} px",
        StillZoneHint: "Le curseur ne bouge pas tant que la main reste dans cette distance. Plus grand absorbe des secousses plus fortes.",
        ClickHold: "Maintenir pour cliquer : {0} ms",
        ClickHoldHint: "Un clic ne compte que si le bouton est maintenu aussi longtemps ; les pressions plus courtes sont ignorées. 0 = désactivé. Essayez 100 à 200 ms en cas de clics involontaires.",
        SteadyWheel: "Stabiliser la molette",
        Done: "Terminé",
        About: "À propos de Tremors",
        MadeBy: "Créé par {0}",
        Free: "Tremors est gratuit, et le restera toujours.",
        Latest: "Obtenir la dernière version");

    private static readonly Language German = new(
        Code: "de",
        Name: "Deutsch",
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
        PointerSpeed: "Zeigergeschwindigkeit: {0} %",
        PointerSpeedHint: "Niedriger = das Zittern wirkt auf dem Bildschirm kleiner, aber Sie bewegen die Maus weiter. Die Mausbeschleunigung von Windows ist aus, solange Tremors aktiv ist.",
        StillZone: "Ruhezone: {0} px",
        StillZoneHint: "Der Zeiger bleibt stehen, bis sich die Hand weiter als diese Strecke bewegt. Größer fängt stärkere Ruckler ab.",
        ClickHold: "Zum Klicken halten: {0} ms",
        ClickHoldHint: "Ein Klick zählt erst, wenn die Taste so lange gedrückt wird; kürzeres Antippen wird ignoriert. 0 = aus. Versuchen Sie 100–200 ms, wenn versehentlich geklickt wird.",
        SteadyWheel: "Mausrad stabilisieren",
        Done: "Fertig",
        About: "Über Tremors",
        MadeBy: "Entwickelt von {0}",
        Free: "Tremors ist kostenlos, und das bleibt auch so.",
        Latest: "Neueste Version herunterladen");

    /// <summary>All languages, in the order of the flags.</summary>
    public static IReadOnlyList<Language> All { get; } = [Dutch, English, French, German];

    private static Language Current = Pick(CultureInfo.CurrentUICulture);

    /// <summary>Two-letter code of the language in use.</summary>
    public static string CurrentCode => Current.Code;

    /// <summary>Switches language; null or an unknown code follows Windows again.</summary>
    public static void Use(string? code) =>
        Current = All.FirstOrDefault(l => l.Code == code) ?? Pick(CultureInfo.CurrentUICulture);

    internal static Language Pick(CultureInfo culture) =>
        All.FirstOrDefault(l => l.Code == culture.TwoLetterISOLanguageName) ?? English;

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
    public static string PointerSpeedHint => Current.PointerSpeedHint;
    public static string StillZoneHint => Current.StillZoneHint;
    public static string ClickHoldHint => Current.ClickHoldHint;
    public static string SteadyWheel => Current.SteadyWheel;
    public static string Done => Current.Done;
    public static string About => Current.About;
    public static string Free => Current.Free;
    public static string Latest => Current.Latest;

    public static string TooltipOn(int strength) => string.Format(Current.TooltipOn, strength);
    public static string Strength(int strength) => string.Format(Current.Strength, strength);
    public static string PointerSpeed(int percent) => string.Format(Current.PointerSpeed, percent);
    public static string StillZone(int pixels) => string.Format(Current.StillZone, pixels);
    public static string ClickHold(int milliseconds) => string.Format(Current.ClickHold, milliseconds);
    public static string MadeBy(string maker) => string.Format(Current.MadeBy, maker);

    /// <summary>A month in the current language, such as "november 2026".</summary>
    public static string Month(DateTime month) => month.ToString("MMMM yyyy", CultureInfo.GetCultureInfo(Current.Code));
}
