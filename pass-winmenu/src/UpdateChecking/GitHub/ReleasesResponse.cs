using System;
using McSherry.SemanticVersioning;
using Newtonsoft.Json;

namespace PassWinmenu.UpdateChecking.GitHub
{
	public class Release
	{
		public string HtmlUrl { get; set; }
		public string TagName { get; set; }
		public bool Prerelease { get; set; }
		public DateTime PublishedAt { get; set; }
		public string Body { get; set; }
		public SemanticVersion Version => SemanticVersion.Parse(TagName, ParseMode.Lenient);
	}
}
