using ReactiveUI;
using Serilog;
using System;

namespace NetStalkerAvalonia
{
	public class ConventionalViewLocator : IViewLocator
	{
		public IViewFor? ResolveView<T>(T viewModel, string? contract = null)
		{
			// Find view's by chopping of the 'Model' on the view model name
			// MyApp.ShellViewModel => MyApp.ShellView
			var viewModelName = viewModel.GetType().FullName;
			string viewTypeName;

			if (viewModel is IRoutableViewModel)
			{
				viewTypeName = viewModelName.TrimEnd("Model".ToCharArray()).Replace(".ViewModels.RoutedViewModels.", ".Views.RoutedViews.");
			}
			else
			{
				viewTypeName = viewModelName.TrimEnd("Model".ToCharArray()).Replace("ViewModels", "Views");
			}

			try
			{
				var viewType = Type.GetType(viewTypeName);
				if (viewType == null)
				{
					Log.Error($"Could not find the view {viewTypeName} for view model {viewModelName}.");
					return null;
				}
				return Activator.CreateInstance(viewType) as IViewFor;
			}
			catch (Exception)
			{
				Log.Error($"Could not instantiate view {viewTypeName}.");
				throw;
			}
		}
	}
}
