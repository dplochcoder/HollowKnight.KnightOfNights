using System;

namespace KnightOfNights.Scripts.SharedLib
{
    public static class EnumExt
    {
        public static ArgumentException InvalidEnum<T>(this T self) where T : Enum => new ArgumentException($"Invalid enum: {self}");
    }
}
