using NetStalkerAvalonia.Core.Theme;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.ViewModels
{
	public class ViewModelBase : ReactiveObject
	{
		public AppTheme Theme => AppTheme.Instance;
	}
}