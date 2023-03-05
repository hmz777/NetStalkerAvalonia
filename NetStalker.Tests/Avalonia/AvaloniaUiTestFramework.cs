using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Threading;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("NetStalker.Tests.Avalonia.AvaloniaUiTestFramework", "NetStalker.Tests")]
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = false, MaxParallelThreads = 1)]
namespace NetStalker.Tests.Avalonia
{
	internal class AvaloniaUiTestFramework : XunitTestFramework
	{
		public AvaloniaUiTestFramework(IMessageSink messageSink)
			: base(messageSink)
		{

		}

		protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
			=> new Executor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

		private class Executor : XunitTestFrameworkExecutor
		{
			public Executor(
				AssemblyName assemblyName,
				ISourceInformationProvider sourceInformationProvider,
				IMessageSink diagnosticMessageSink)
				: base(
					assemblyName,
					sourceInformationProvider,
					diagnosticMessageSink)
			{

			}

			protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases,
				IMessageSink executionMessageSink,
				ITestFrameworkExecutionOptions executionOptions)
			{
				executionOptions.SetValue("xunit.execution.DisableParallelization", false);
				using var assemblyRunner = new AvaloniaRunner(
					TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink,
					executionOptions);

				await assemblyRunner.RunAsync();
			}
		}

		internal class AvaloniaRunner : XunitTestAssemblyRunner
		{
			public AvaloniaRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions) :
				base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
			{
			}

			public override void Dispose()
			{
				AvaloniaApp.Stop();

				base.Dispose();
			}

			protected override void SetupSyncContext(int maxParallelThreads)
			{
				var tcs = new TaskCompletionSource<SynchronizationContext>();

				new Thread(() =>
				{
					try
					{
						var builder = AvaloniaApp
							.BuildAvaloniaApp()
							.UseHeadless()
							.AfterSetup((b) =>
							{
								var lifeTime = b.Instance.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

								lifeTime.Startup += (sender, args) =>
								{
									HostInfo.SetHostInfo(null, null, null, null, null, IpType.Ipv4, null, NetworkClass.C);

									// We wait until the framework and app are completely initialized and ready to run
									tcs.SetResult(SynchronizationContext.Current);

									Dispatcher.UIThread.MainLoop(CancellationToken.None);
								};
							})
							.StartWithClassicDesktopLifetime(Array.Empty<string>());
					}
					catch (Exception e)
					{
						tcs.SetException(e);
					}
				})
				{
					IsBackground = true

				}.Start();

				SynchronizationContext.SetSynchronizationContext(tcs.Task.Result);
			}
		}
	}
}