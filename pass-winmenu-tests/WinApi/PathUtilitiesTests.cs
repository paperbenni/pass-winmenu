using PassWinmenu.WinApi;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.WinApi
{
	public class PathUtilitiesTests
	{
		[Fact]
		public void MakeRelativePath_IsChildPath_MakesRelativeToBase()
		{
			var parent = @"C:\parent";
			var child = @"C:\parent\child";

			PathUtilities.MakeRelativePathForDisplay(parent, child).ShouldBe("child");
		}

		[Fact]
		public void MakeRelativePath_TrailingSlash_PreservesTrailingSlash()
		{
			var parent = @"C:\parent";
			var child = @"C:\parent\child\";

			PathUtilities.MakeRelativePathForDisplay(parent, child).ShouldBe("child/");
		}

		[Fact]
		public void MakeRelativePath_SamePath_ProducesSingleDot()
		{
			var filesystem = new MockFileSystemBuilder().Build();

			var parent = filesystem.DirectoryInfo.FromDirectoryName(@"C:\parent");
			var child = filesystem.DirectoryInfo.FromDirectoryName(@"C:\parent");

			PathUtilities.MakeRelativePathForDisplay(parent, child).ShouldBe(".");
		}

		[Fact]
		public void MakeRelativePath_Directory_AddsTrailingSlash()
		{
			var filesystem = new MockFileSystemBuilder().Build();

			var parent = filesystem.DirectoryInfo.FromDirectoryName(@"C:\parent");
			var child = filesystem.DirectoryInfo.FromDirectoryName(@"C:\parent\sub\child");

			PathUtilities.MakeRelativePathForDisplay(parent, child).ShouldBe("sub/child/");
		}

		[Fact]
		public void MakeRelativePath_File_NoTrailingSlash()
		{
			var filesystem = new MockFileSystemBuilder().Build();

			var parent = filesystem.DirectoryInfo.FromDirectoryName(@"C:\parent");
			var child = filesystem.FileInfo.FromFileName(@"C:\parent\sub\child");

			PathUtilities.MakeRelativePathForDisplay(parent, child).ShouldBe("sub/child");
		}
	}
}
