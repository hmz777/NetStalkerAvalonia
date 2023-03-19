using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels;

namespace NetStalkerAvalonia.Core.Views
{
	public partial class AppLogView : ReactiveWindow<AppLogViewModel>
	{
		public AppLogView()
		{
			InitializeComponent();
		}
	}
}