using System;
using System.Globalization;
using System.IO;

namespace PassWinmenu.WinApi
{
	internal static class PathUtilities
	{
		/// <summary>
		/// Reformats a path as relative to the specified base directory.
		/// If the path does not point to a child of the base directory,
		/// the full path is returned.
		/// Note: If Windows-style directory separators are provided,
		/// they will be converted to UNIX-style.
		/// </summary>
		/// <param name="baseDir"></param>
		/// <param name="absoluteDir"></param>
		/// <returns></returns>
		public static string MakeRelativePath(string baseDir, string absoluteDir)
		{
			if (!baseDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal))
			{
				baseDir = baseDir + Path.DirectorySeparatorChar;
			}
			if (string.IsNullOrEmpty(baseDir)) throw new ArgumentNullException(nameof(baseDir));
			if (string.IsNullOrEmpty(absoluteDir)) throw new ArgumentNullException(nameof(absoluteDir));

			var baseUri = new Uri(baseDir);
			var absoluteUri = new Uri(absoluteDir);

			if (baseUri.Scheme != absoluteUri.Scheme) { return absoluteDir; } // path can't be made relative.

			var relativeUri = baseUri.MakeRelativeUri(absoluteUri);
			var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			return relativePath;
		}

		/// <summary>
		/// Normalises a directory, replacing all AltDirectorySeparatorChars with DirectorySeparatorChars
		/// and stripping any trailing directory separators.
		/// </summary>
		/// <param name="directory">The directory to be normalised.</param>
		/// <returns>The normalised directory.</returns>
		internal static string NormaliseDirectory(string directory)
		{
			var normalised = directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			var stripped = normalised.TrimEnd(Path.DirectorySeparatorChar);
			return stripped;
		}

	}
}
