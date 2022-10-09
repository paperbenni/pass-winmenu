using System;
using System.Collections.Generic;
using System.Linq;

namespace PassWinmenu.Utilities.ExtensionMethods
{
	internal static class EnumerableExtensions
	{
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			var seen = new HashSet<TKey>();
			foreach(var item in source)
			{
				if (seen.Add(keySelector(item)))
				{
					yield return item;
				}
			}
		}

		public static IEnumerable<TSource> ConcatSingle<TSource>(this IEnumerable<TSource> source, TSource element)
		{
			return source.Concat(new[] {element});
		}
	}
}
