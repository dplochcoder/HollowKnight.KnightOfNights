using KnightOfNights.Scripts.SharedLib;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal enum CustomEnvironmentType
{
    SNOW = 12,  // This cannot change even if `ToIntId()` changes.
    ICE = 13,   // This cannot change even if `ToIntId()` changes.
}

internal static class CustomEnvironmentExtensions
{
    internal static int ToIntId(this CustomEnvironmentType self) => self switch { CustomEnvironmentType.SNOW => 12, CustomEnvironmentType.ICE => 13, _ => throw self.InvalidEnum() };
}
