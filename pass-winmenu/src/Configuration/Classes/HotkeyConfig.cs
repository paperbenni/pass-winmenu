using System;
using PassWinmenu.Utilities.ExtensionMethods;
using YamlDotNet.Serialization;

#nullable enable
namespace PassWinmenu.Configuration
{
	internal class HotkeyConfig
	{
		public string Hotkey { get; set; } = string.Empty;
		[YamlIgnore]
		public HotkeyAction Action => (HotkeyAction)Enum.Parse(typeof(HotkeyAction), ActionString.ToPascalCase(), true);
		[YamlMember(Alias = "action")]
		public string ActionString { get; set; } = "decrypt-password";
		public HotkeyOptions Options { get; set; } = new HotkeyOptions();
	}
}
