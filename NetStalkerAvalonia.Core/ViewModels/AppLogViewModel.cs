using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NetStalkerAvalonia.Core.ViewModels
{
	public class AppLogViewModel : ViewModelBase, IDisposable
	{
		private readonly StringWriter AppLog = new();
		private readonly Timer timer;

		public AppLogViewModel()
		{
			Console.SetOut(AppLog);

			timer = new Timer(TimeSpan.FromSeconds(1));
			timer.Elapsed += Timer_Elapsed;

			timer.Start();
		}

		private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
		{
			AppLogOutput = AppLog.ToString();
		}

		private string? appLogOutput;
		public string? AppLogOutput
		{
			get => appLogOutput;
			set => this.RaiseAndSetIfChanged(ref appLogOutput, value);
		}

		public void Dispose()
		{
			timer.Stop();
			timer.Dispose();
			AppLog.Dispose();
		}
	}
}
