using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

public class MonobehaviourPatcher<M>(System.Func<M> prefab, params string[] fieldNames) where M : MonoBehaviour
{
    private record Field(FieldInfo FieldInfo, object Value) { }

    private readonly Lazy<List<Field>> fields = new(() =>
        {
            List<Field> list = [];

            var obj = prefab();
            var type = obj.GetType();
            foreach (var name in fieldNames)
            {
                var fi = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fi != null) list.Add(new(fi, fi.GetValue(obj)));
                else KnightOfNightsMod.LogError($"Bad field: {type.Name}.{name}");
            }
            return list;
        });

    public void Patch(M component) => fields.Get().ForEach(f => f.FieldInfo.SetValue(component, f.Value));

    public M Patch(GameObject gameObject)
    {
        var component = gameObject.GetOrAddComponent<M>();
        Patch(component);
        return component;
    }
}