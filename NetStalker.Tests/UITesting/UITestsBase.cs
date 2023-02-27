using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
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
