using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Configuration;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views.RoutedViews
{
    public partial class OptionsView : ReactiveUserControl<OptionsViewModel>
	{
		public OptionsView()
		{
			this.WhenActivated(disposables =>
			{
				ViewModel!.AppSettings = Config.AppSettings!;
			});

			AvaloniaXamlLoader.Load(this);
		}
	}
}