#nullable enable
namespace PassWinmenu.ExternalPrograms.Gpg
{
	public interface IGpgHomedirResolver
	{
		/// <summary>
		/// Returns the path GPG will use as its home directory.
		/// </summary>
		string GetHomeDir();

		/// <summary>
		/// Returns the home directory as configured by the user, or null if no home directory has been defined.
		/// </summary>
		string? GetConfiguredHomeDir();
	}
}
