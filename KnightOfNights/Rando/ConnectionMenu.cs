using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using System.Linq;

namespace KnightOfNights.Rando;

internal class ConnectionMenu
{
    internal static ConnectionMenu? Instance { get; private set; }

    internal static void Setup()
    {
        RandomizerMenuAPI.AddMenuPage(OnRandomizerMenuConstruction, TryGetMenuButton);
        MenuChangerMod.OnExitMainMenu += () => Instance = null;
    }

    internal static void OnRandomizerMenuConstruction(MenuPage page) => Instance = new(page);

    internal static bool TryGetMenuButton(MenuPage page, out SmallButton button)
    {
        button = Instance!.entryButton;
        return true;
    }

    private readonly SmallButton entryButton;
    private readonly MenuElementFactory<RandomizationSettings> factory;

    private ConnectionMenu(MenuPage landingPage)
    {
        MenuPage mainPage = new("Knight of Nights", landingPage);
        entryButton = new(landingPage, "Knight of Nights");
        entryButton.AddHideAndShowEvent(mainPage);

        factory = new(mainPage, KnightOfNightsMod.RS);
        foreach (var menuItem in factory.Elements.OfType<MenuItem>()) menuItem.SelfChanged += _ => SetEnabledColor();

        MenuLabel header = new(mainPage, "Knight of Nights");
        header.MoveTo(SpaceParameters.TOP_CENTER);
        new VerticalItemPanel(mainPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_MEDIUM, true, [.. factory.Elements]);

        SetEnabledColor();
    }

    internal void ApplySettings(RandomizationSettings settings) => factory.SetMenuValues(settings);

    private void SetEnabledColor() => entryButton.Text.color = KnightOfNightsMod.RS.IsEnabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
}
