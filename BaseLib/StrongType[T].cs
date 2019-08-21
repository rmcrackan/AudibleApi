namespace BaseLib
{
    /// <summary>
    /// This makes writing code cumbersome ...it's all just strings afterall.
    /// However, this makes it intent clear in all contexts for what belongs where.
    /// In the string-heavy world of the web, it's VERY easy to put strings in the wrong order and this has saved my bacon many times. Same with int.
    /// It also facilitates the mentality to move away from primative addiction.
    /// 
    /// Similar to ValueObject, but do not inherit from it. ValueObject ensures only that instances of ValueObject are equal.
    /// StrongType also ensures that a StrongType instance is equal to another StrongType's internal Value property's value.
    /// Eg: StrongType&gt;int>(123) == 123;
    /// StrongType is like a special kind of ValueObject.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StrongType<T>
    {
        private const int NULL_PRIME_NUMBER = 19;

        public T Value { get; }

		public StrongType(T value)
		{
			ValidateInput(value);
			Value = value;
		}

		protected virtual void ValidateInput(T value) { }

        public override string ToString() => Value?.ToString();

        public override bool Equals(object obj)
            => (obj is StrongType<T> st) ? Equals(st.Value)
            : (Value == null) ? (obj is null)
            : Value.Equals(obj);

        public override int GetHashCode()
            => (Value == null)
            ? NULL_PRIME_NUMBER
            : Value.GetHashCode();

        public static implicit operator T(StrongType<T> strongType) => strongType.Value;

        public static implicit operator StrongType<T>(T t) => new StrongType<T>(t);

        public static bool operator ==(StrongType<T> a, StrongType<T> b)
            => ((a is null || a.Value == null) && (b is null || b.Value == null)) ? true
            : (a is null || a.Value == null || b is null || b.Value == null) ? false
            : a.Equals(b);

        public static bool operator !=(StrongType<T> a, StrongType<T> b) => !(a == b);
    }
}
