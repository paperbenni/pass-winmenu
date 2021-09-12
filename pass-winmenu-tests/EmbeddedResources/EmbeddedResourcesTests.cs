using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.EmbeddedResources
{
		public class EmbeddedResourcesTests
	{
		private const string Category = "Core: Embedded resources";

		[Fact, TestCategory(Category)]
		public void EmbeddedResources_ContainsVersionString()
		{
			PassWinmenu.EmbeddedResources.Load();
			PassWinmenu.EmbeddedResources.Version.ShouldNotBeNullOrWhiteSpace();
			PassWinmenu.EmbeddedResources.Version.ShouldNotBe("<unknown version>");
			Assert.False(string.IsNullOrWhiteSpace(PassWinmenu.EmbeddedResources.UnknownVersion));
		}
	}
}
