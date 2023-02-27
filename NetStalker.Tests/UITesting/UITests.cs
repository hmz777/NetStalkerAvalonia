using Avalonia.Controls;
using Avalonia.VisualTree;
using FluentAssertions;
using NetStalker.Tests.Avalonia;
using NetStalkerAvalonia.Components;
using NetStalkerAvalonia.Views;

namespace NetStalker.Tests.UITesting
{
	public class UITests : UITestsBase
	{
		[Fact]
		public void Can_Choose_Adapter()
		{
			ChooseAdapter();

			AvaloniaApp.GetMainWindow().GetType().Should().Be(typeof(MainView));
		}

		[Fact]
		public void Can_Navigate_To_About()
		{
			ChooseAdapter();

			var mainWindow = AvaloniaApp.GetMainWindow();
			var navbar = new Navbar()
			{
				DataContext = mainWindow.DataContext
			};
			var aboutButton = navbar.Find<NavButton>("About");

			aboutButton.Command.Execute(null!);

			mainWindow.FindDescendantOfType<PageTitle>().Find<TextBlock>("PageTitleText").Text.Should().Be("About");
		}

		private void ChooseAdapter()
		{
			var mainWindow = AvaloniaApp.GetMainWindow();
			var adapterSelectBox = mainWindow.Find<ComboBox>("AdapterSelectBox");
			var okButton = mainWindow.Find<Button>("Ok");
			var adapterCount = adapterSelectBox.ItemCount;

			for (int i = 0; i < adapterCount; i++)
			{
				adapterSelectBox.SelectedIndex = i;

				if (okButton.Command.CanExecute(null!))
				{
					okButton.Command.Execute(null!);

					break;
				}
			}
		}
	}
}