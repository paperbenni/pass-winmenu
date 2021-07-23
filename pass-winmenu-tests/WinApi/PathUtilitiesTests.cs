using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassWinmenu.WinApi;
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

			PathUtilities.MakeRelativePath(parent, child).ShouldBe("child");
		}

		[Fact]
		public void MakeRelativePath_TrailingSlash_PreservesTrailingSlash()
		{
			var parent = @"C:\parent";
			var child = @"C:\parent\child\";

			PathUtilities.MakeRelativePath(parent, child).ShouldBe("child/");
		}
	}
}
