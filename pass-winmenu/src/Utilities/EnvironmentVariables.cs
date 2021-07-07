using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassWinmenu.Utilities
{
	public class EnvironmentVariables
	{
		public string PasswordStoreKey { get; set; }

		public static EnvironmentVariables LoadFromEnvironment() {
			return new EnvironmentVariables()
			{
				PasswordStoreKey = Environment.GetEnvironmentVariable("PASSWORD_STORE_KEY"),
			};
		}
	}
}
