using FluentAssertions;

namespace NetStalker.Tests.UITesting
{
	public class UITests : UITestsBase
	{
		[Fact]
		public void App_Initialized()
		{
			var mainWindow = Application.MainWindow;

			mainWindow.Should().NotBe(null);
		}
	}
}