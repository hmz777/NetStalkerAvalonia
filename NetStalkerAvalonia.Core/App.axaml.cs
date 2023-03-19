using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NetStalkerAvalonia.Core.Helpers;
using Serilog;
using System;

namespace NetStalkerAvalonia.Core
{
	public partial class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			try
			{
				if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				{
					LifeTimeHelpers.ConfigureAppStart(this);
				}

				base.OnFrameworkInitializationCompleted();
			}
			catch (Exception e)
			{
				Log.Error(LogMessageTemplates.ExceptionTemplate,
					e.GetType(), this.GetType(), e.Message);

				throw;
			}
		}
	}
}