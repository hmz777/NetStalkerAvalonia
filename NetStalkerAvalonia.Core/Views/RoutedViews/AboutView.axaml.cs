using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;
using ReactiveUI;
using System.Diagnostics;
using System.Linq;

namespace NetStalkerAvalonia.Core.Views.RoutedViews
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
	}
}