using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.Views.RoutedViews
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