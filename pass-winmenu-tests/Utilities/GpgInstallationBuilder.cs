using PassWinmenu.ExternalPrograms.Gpg;

namespace PassWinmenuTests.Utilities
{
	internal class GpgInstallationBuilder
	{

		public GpgInstallation Build()
		{
			var fileSystem = new MockFileSystemBuilder().Build();
			return new GpgInstallation
			(
				fileSystem.DirectoryInfo.FromDirectoryName(@"C:\gpg\bin"),
				fileSystem.FileInfo.FromFileName(@"C:\gpg\bin\gpg.exe"),
				fileSystem.FileInfo.FromFileName(@"C:\gpg\bin\gpg-agent.exe"),
				fileSystem.FileInfo.FromFileName(@"C:\gpg\bin\gpg-connect-agent.exe")
			);
		}
	}
}
