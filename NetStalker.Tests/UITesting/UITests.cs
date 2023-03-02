using Avalonia.Controls;
using Avalonia.VisualTree;
using FluentAssertions;
using NetStalker.Tests.Avalonia;
using NetStalkerAvalonia.Components;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.Views;
using Splat;

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

			var mainWindow = new MainView()
			{
				DataContext = Locator.Current.GetService<MainViewModel>()
			};

			var navbar = new Navbar()
			{
				DataContext = Locator.Current.GetService<MainViewModel>()
			};
			var aboutButton = navbar.Find<NavButton>("About");

			aboutButton.Command.Execute(null!);

			mainWindow.ViewModel.PageTitle.Should().Be("About");
		}

		private void ChooseAdapter()
		{
			var adapterSelectWindow = new AdapterSelectView()
			{
				DataContext = Locator.Current.GetService<AdapterSelectViewModel>()
			};

			var adapterSelectBox = adapterSelectWindow.Find<ComboBox>("AdapterSelectBox");
			var okButton = adapterSelectWindow.Find<Button>("Ok");
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