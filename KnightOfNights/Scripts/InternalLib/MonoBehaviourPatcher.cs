using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

public class MonobehaviourPatcher<M> where M : MonoBehaviour
{
    private record Field
    {
        public FieldInfo fi;
        public object value;
    }

    private readonly Lazy<List<Field>> fields;

    public MonobehaviourPatcher(System.Func<M> prefab, params string[] fieldNames)
    {
        fields = new(() =>
        {
            List<Field> list = [];

            var obj = prefab();
            var type = obj.GetType();
            foreach (var name in fieldNames)
            {
                var fi = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fi != null)
                {
                    list.Add(new()
                    {
                        fi = fi,
                        value = fi.GetValue(obj)
                    });
                }
                else KnightOfNightsMod.LogError($"Bad field: {type.Name}.{name}");
            }
            return list;
        });
    }

    public void Patch(M component) => fields.Get().ForEach(f => f.fi.SetValue(component, f.value));

    public M Patch(GameObject gameObject)
    {
        var component = gameObject.GetOrAddComponent<M>();
        Patch(component);
        return component;
    }
}