using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;

namespace NetStalkerAvalonia.Views
{
	public partial class AppLogWindow : ReactiveWindow<AppLogViewModel>
	{
		public AppLogWindow()
		{
			InitializeComponent();
		}
	}
}