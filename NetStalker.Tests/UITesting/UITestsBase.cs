using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using NetStalker.Tests.Avalonia;

namespace NetStalker.Tests.UITesting
{
    public class UITestsBase
	{
		internal IClassicDesktopStyleApplicationLifetime Application { get; }
		public UITestsBase()
		{
			Application = AvaloniaApp.GetApp() ?? throw new InvalidOperationException("Failed to initialize application");
		}
	}
}
