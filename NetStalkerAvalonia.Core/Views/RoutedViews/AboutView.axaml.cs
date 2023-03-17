using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;
using System.Diagnostics;
using System.Linq;

namespace NetStalkerAvalonia.Views.RoutedViews
{
	public partial class AboutView : ReactiveUserControl<AboutViewModel>
	{
		public AboutView()
		{
			this.WhenActivated(disposables =>
			{
			});

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}