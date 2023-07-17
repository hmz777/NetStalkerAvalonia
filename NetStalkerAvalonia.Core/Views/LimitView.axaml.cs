using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace NetStalkerAvalonia.Core.Views;

public partial class LimitView : ReactiveWindow<LimitViewModel>
{
	public LimitView()
	{
		this.WhenActivated(disposables =>
		{
			ViewModel!.Apply.Subscribe(Close).DisposeWith(disposables);
		});

		InitializeComponent();
	}
}