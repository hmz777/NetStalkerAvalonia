using Avalonia.Controls;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using System.Collections.Generic;

namespace NetStalkerAvalonia
{
	public class StaticData
	{
		public static Window? MainWindow { get; set; }

		public static List<ViewModelBase> ViewModels = new();

		public static void InitRoutedViewModels()
		{
			var mainViewModel = new MainWindowViewModel();

			ViewModels = new List<ViewModelBase>
		{
			mainViewModel,
			new SnifferViewModel(mainViewModel),
			new OptionsViewModel(mainViewModel),
			new RuleBuilderViewModel(mainViewModel),
			new HelpViewModel(mainViewModel),
			new AboutViewModel(mainViewModel),
			new AppLogViewModel()
		};
		}
	}
}