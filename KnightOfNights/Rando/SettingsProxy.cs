using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace KnightOfNights.Rando;

internal class SettingsProxy : RandoSettingsProxy<RandomizationSettings, string>
{
    internal static void Setup() => RandoSettingsManager.RandoSettingsManagerMod.Instance.RegisterConnection(new SettingsProxy());

    public override string ModKey => nameof(KnightOfNightsMod);

    public override VersioningPolicy<string> VersioningPolicy => new StrictModVersioningPolicy(KnightOfNightsMod.Instance!);

    public override bool TryProvideSettings(out RandomizationSettings? settings)
    {
        settings = KnightOfNightsMod.RS;
        return settings.IsEnabled;
    }

    public override void ReceiveSettings(RandomizationSettings? settings) => ConnectionMenu.Instance!.ApplySettings(settings ?? new());
}
