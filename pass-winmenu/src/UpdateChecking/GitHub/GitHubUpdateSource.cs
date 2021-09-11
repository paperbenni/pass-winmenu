using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#nullable enable
namespace PassWinmenu.UpdateChecking.GitHub
{
	internal class GitHubUpdateSource : IUpdateSource
	{
		private const string UpdateUrl = "https://api.github.com/repos/geluk/pass-winmenu/releases";
		private readonly JsonSerializerSettings settings;

		public bool RequiresConnectivity => true;

		public GitHubUpdateSource()
		{
			settings = new JsonSerializerSettings
			{
				ContractResolver = new DefaultContractResolver
				{
					NamingStrategy = new SnakeCaseNamingStrategy()
				}
			};
		}

		private ProgramVersion ToProgramVersion(Release release)
		{
			var importantRegex = new Regex(@"\*\*\s*important\s+release:?\s*\*\*", RegexOptions.IgnoreCase);
			var important = importantRegex.IsMatch(release.Body);

			return new ProgramVersion
			{
				VersionNumber = release.Version,
				DownloadLink = new Uri(release.HtmlUrl),
				ReleaseDate = release.PublishedAt,
				ReleaseNotes = new Uri(release.HtmlUrl),
				IsPrerelease = release.Prerelease,
				Important = important,
			};
		}

		public IEnumerable<ProgramVersion> GetAllReleases()
		{
			var releases = FetchReleases();
			return releases.Select(ToProgramVersion);
		}

		private Release[] FetchReleases()
		{
			var rq = WebRequest.CreateHttp(UpdateUrl);
			rq.ServerCertificateValidationCallback += ValidateCertificate;
			rq.UserAgent = $"pass-winmenu/{Program.Version}";

			using var stream = rq.GetResponse().GetResponseStream();
			if (stream == null)
			{
				throw new UpdateException("Unable to fetch response stream.");
			}
			using var reader = new StreamReader(stream);
			var responseText = reader.ReadToEnd();
			return JsonConvert.DeserializeObject<Release[]>(responseText, settings) ?? Array.Empty<Release>();
		}

		private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None) return true;
			
			Log.Send($"Server certificate failed to validate: {sslPolicyErrors}", LogLevel.Warning);
			return false;
		}
	}
}
