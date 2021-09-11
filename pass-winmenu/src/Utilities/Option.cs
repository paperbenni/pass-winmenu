using System;

#nullable enable
namespace PassWinmenu.Utilities
{
	internal readonly struct Option<T>
	{
		public T Value { get; }
		public bool IsSome { get; }
		public bool IsNone => !IsSome;

		public Option(T value, bool hasValue)
		{
			Value = value;
			IsSome = hasValue;
		}

		//  Bit of a hack, but we can't use nullable annotations until C# 9.
		public static Option<T> None() => new Option<T>(default!, false);
	}

	internal static class Option
	{
		public static Option<T> FromNullable<T>(T? value) where T : class => new Option<T>(value!, value != null);

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
			return Option<TDst>.None();
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
			return default!;
		}
	}
}
