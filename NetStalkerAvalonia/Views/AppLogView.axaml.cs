using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;

namespace NetStalkerAvalonia.Views
{
	public partial class AppLogView : ReactiveWindow<AppLogViewModel>
	{
		public AppLogView()
		{
			InitializeComponent();
		}
	}
}