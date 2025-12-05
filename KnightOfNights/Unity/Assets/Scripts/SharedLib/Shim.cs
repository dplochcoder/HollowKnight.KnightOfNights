using System;
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

    [AttributeUsage(AttributeTargets.Field)]
    internal class ShimField : Attribute
    {
        public readonly string DefaultValue;

        public ShimField(string defaultValue = null) => DefaultValue = defaultValue;
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class ShimMethod : Attribute { }
}
