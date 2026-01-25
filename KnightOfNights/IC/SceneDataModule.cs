using System;
using System.Collections.Generic;
using System.Linq;

namespace KnightOfNights.IC;

internal class SceneDataModule : AbstractDataModule<SceneDataModule, SortedDictionary<string, List<object>>>
{
    public IEnumerable<T> GetForScene<T>(string sceneName) => Data.TryGetValue(sceneName, out var values) ? values.OfType<T>() : [];

    public IEnumerable<T> GetForActiveScene<T>() => GetForScene<T>(GameManager.instance.sceneName);

    public bool TryGetSingle<T>(string sceneName, out T value)
    {
        var iter = GetForScene<T>(sceneName).GetEnumerator();
        if (iter.MoveNext())
        {
            value = iter.Current;

            if (iter.MoveNext()) throw new ArgumentException($"Multiple {typeof(T)} in {sceneName}");
            return true;
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
        return false;
    }

    public bool TryGetSingleActive<T>(out T value) => TryGetSingle(GameManager.instance.sceneName, out value);

    public T? GetSingleOrDefault<T>(string sceneName) => TryGetSingle<T>(sceneName, out var value) ? value : default;

    public T? GetSingleOrDefaultActive<T>() => GetSingleOrDefault<T>(GameManager.instance.sceneName);

    public IEnumerable<(string, T)> GetForAllScenes<T>() => Data.SelectMany(e => e.Value.OfType<T>().Select(v => (e.Key, v)));

    protected override bool Unity() => true;

    protected override string JsonName() => "scene_data";

    protected override SceneDataModule Self() => this;
}
