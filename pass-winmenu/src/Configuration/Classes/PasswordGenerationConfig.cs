namespace PassWinmenu.Configuration
{
	public class PasswordGenerationConfig
	{
		public int Length { get; set; } = 20;
		public CharacterGroupConfig[] CharacterGroups { get; set; } =
		{
			new("Symbols", "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~", true),
			new("Numeric", "0123456789", true),
			new("Lowercase", "abcdefghijklmnopqrstuvwxyz", true),
			new("Uppercase", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", true),
			new("Whitespace", " ", false), 
		};
		public string DefaultContent { get; set; } = "Username: \n";
	}
}
