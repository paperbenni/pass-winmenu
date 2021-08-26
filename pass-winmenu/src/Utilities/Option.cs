using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PassWinmenu.Utilities
{
	internal readonly struct Option<T> : IEquatable<Option<T>>
	{
		public T Value { get; }
		public bool IsSome { get; }
		public bool IsNone => !IsSome;

		public Option(T value, bool hasValue)
		{
			Value = value;
			IsSome = hasValue;
		}

		public override bool Equals(object obj)
		{
			try
			{
				var converted = (Option<T>)obj;
				return Equals(converted);
			}
			catch (InvalidCastException)
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			var hashCode = 1816676634;
			hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
			hashCode = hashCode * -1521134295 + IsSome.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Option<T> left, Option<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Option<T> left, Option<T> right)
		{
			return !(left == right);
		}

		public bool Equals(Option<T> other)
		{
			if (!IsSome && !other.IsSome)
			{
				return true;
			}
			if (IsSome && other.IsSome)
			{
				return Value.Equals(other.Value);
			}
			return false;
		}
		public static Option<T> None() => new Option<T>(default, false);
	}

	internal static class Option
	{
		public static Option<T> FromNullable<T>(T value) => new Option<T>(value, value != null);

		public static Option<T> Some<T>(T value) => new Option<T>(value, true);
	}

	internal static class OptionExtensions
	{
		public static Option<TDst> Select<TSrc, TDst>(this Option<TSrc> source, Func<TSrc, TDst> valueMap)
		{
			if (source.IsSome)
			{
				return new Option<TDst>(valueMap(source.Value), true);
			}
			return new Option<TDst>(default, false);
		}

		public static void Apply<T>(this Option<T> source, Action<T> action)
		{
			if (source.IsSome)
			{
				action(source.Value);
			}
		}

		public static T ValueOrDefault<T>(this Option<T> source)
		{
			if (source.IsSome)
			{
				return source.Value;
			}
			return default;
		}
	}
}
