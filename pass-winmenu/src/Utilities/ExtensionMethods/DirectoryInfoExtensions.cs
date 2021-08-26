using System.IO.Abstractions;
using System.Linq;

namespace PassWinmenu.Utilities.ExtensionMethods
{
	internal static class DirectoryInfoExtensions
	{
		/// <summary>
		/// Checks whether the directory contains the given file.
		/// </summary>
		/// <returns>True if the directory contains the file, false otherwise.</returns>
		internal static bool ContainsFile(this IDirectoryInfo directory, string name)
		{
			return directory.EnumerateFiles(name).Any();
		}

		internal static bool IsChildOrSelf(this IDirectoryInfo directory, IDirectoryInfo child)
		{
			do
			{
				if (child.PathEquals(directory))
				{
					return true;
				}
				child = child.Parent;
			} while (child != null);

			return false;
		}
		
		internal static bool IsChild(this IDirectoryInfo directory, IFileInfo child)
		{
			return IsChildOrSelf(directory, child.Directory);
		}
	}
}
