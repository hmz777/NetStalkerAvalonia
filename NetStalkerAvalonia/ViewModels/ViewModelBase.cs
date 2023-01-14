using NetStalkerAvalonia.Theme;
using ReactiveUI;

namespace NetStalkerAvalonia.ViewModels
{
	public class ViewModelBase : ReactiveObject
	{
		public AppTheme Theme => AppTheme.Instance;
	}
}