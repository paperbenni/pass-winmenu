using System;
using System.Collections.Generic;

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
	}
}
