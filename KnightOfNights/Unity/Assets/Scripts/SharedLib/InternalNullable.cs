using System;

namespace KnightOfNights.Scripts.SharedLib
{
    // Nullable wrapper since Unity env doesn't support nullable types.
    public class InternalNullable<T>
    {
        private bool isNull;
        private T _value;

        public bool IsNull => isNull;

        public T Value
        {
            get
            {
                if (isNull) throw new NullReferenceException();
                return _value;
            }
            set
            {
                _value = value;
                isNull = false;
            }
        }

        public T MaybeValue => _value;

        public InternalNullable(T value)
        {
            isNull = false;
            _value = value;
        }

        public InternalNullable()
        {
            isNull = true;
            _value = default;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is InternalNullable<T> other)) return false;

            if (other.isNull && isNull) return true;
            if (other.isNull || isNull) return false;
            return other.Value.Equals(Value);
        }

        public override int GetHashCode() => isNull ? 0 : Value.GetHashCode();
    }
}
