using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace NetStalkerAvalonia.Core.Views;

public partial class StatusMessageView : ReactiveWindow<StatusMessageViewModel>
{
	public StatusMessageView()
	{
		this.WhenActivated(disposables => { ViewModel!.Close.Subscribe(x => Close()).DisposeWith(disposables); });

		InitializeComponent();
	}
}