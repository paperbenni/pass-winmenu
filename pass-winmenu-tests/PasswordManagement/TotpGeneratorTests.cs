using System;
using System.Collections.Generic;
using PassWinmenu.PasswordManagement;
using PassWinmenuTests.Utilities.ExtensionMethods;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.PasswordManagement
{
	public class TotpGeneratorTests
	{
		[Fact]
		public void GenerateTotpCode_NoTotp_None()
		{
			var keys = new List<KeyValuePair<string, string>>() {
				new KeyValuePair<string, string>("Key", "Value")
			};

			var file = new KeyedPasswordFile(new PasswordFile(null, null), string.Empty, null, keys);
			var code = TotpGenerator.GenerateTotpCode(file, DateTime.Now);

			code.ShouldBeNone();
		}

		[Fact]
		public void GenerateTotpCode_ValidTotp_ValidCode()
		{
			var keys = new List<KeyValuePair<string, string>>() {
				new KeyValuePair<string, string>("TOTP", "ABCDEF")
			};

			var file = new KeyedPasswordFile(new PasswordFile(null, null), string.Empty, null, keys);
			var code = TotpGenerator.GenerateTotpCode(file, new DateTime(2021, 10, 13, 15, 15, 30, DateTimeKind.Utc));

			code.ShouldBeSome("235025");
		}

	}
}
