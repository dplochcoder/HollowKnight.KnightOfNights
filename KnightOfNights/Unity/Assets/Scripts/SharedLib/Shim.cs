using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.SharedLib
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface)]
    internal class Shim : Attribute
    {
        public readonly Type baseType;

        public Shim(Type baseType = null)
        {
            this.baseType = baseType ?? typeof(MonoBehaviour);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class ShimPreload : Attribute
    {
        public readonly string[] names;

        public ShimPreload(params string[] names) => this.names = names;

        public List<string> GetNames(Type baseType)
        {
            if (names.Length > 0) return new List<string>(names);
            else return new List<string>(new string[] { baseType.Name });
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal class ShimField : Attribute
    {
        public readonly string DefaultValue;

        public ShimField(string defaultValue = null) => DefaultValue = defaultValue;
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class ShimMethod : Attribute { }
}
