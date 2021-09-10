using System.Collections.Generic;
using PassWinmenu.Configuration;
using PassWinmenu.PasswordManagement;
using PassWinmenu.Utilities.ExtensionMethods;

namespace PassWinmenu.Windows
{
	class PathDisplayHelper
	{
		private readonly string directorySeparator;

		public PathDisplayHelper(InterfaceConfig config)
		{
			this.directorySeparator = config.DirectorySeparator;
		}

		public string GetDisplayPath(PasswordFile file)
		{
			var names = new List<string>
			{
				file.FileNameWithoutExtension
			};

			var current = file.Directory;
			while (!current.PathEquals(file.PasswordStore))
			{
				names.Insert(0, current.Name);
				current = current.Parent;
			}

			return string.Join(directorySeparator, names);
		}
	}
}
