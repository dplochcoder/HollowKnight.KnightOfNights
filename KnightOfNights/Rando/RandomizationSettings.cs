using Newtonsoft.Json;

namespace KnightOfNights.Rando;

public enum RevekSongRandoMode
{
    Disabled,
    Start,
    Vanilla,
    Randomized
}

public class RandomizationSettings
{
    public RevekSongRandoMode RevekSong = RevekSongRandoMode.Disabled;

    [JsonIgnore]
    public bool IsEnabled => RevekSong != RevekSongRandoMode.Disabled;
}
