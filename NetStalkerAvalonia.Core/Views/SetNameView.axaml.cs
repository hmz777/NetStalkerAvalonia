using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace NetStalkerAvalonia.Core.Views;

public partial class SetNameView : ReactiveWindow<SetNameViewModel>
{
	public SetNameView()
	{
		this.WhenActivated(disposables => { ViewModel!.Accept.Subscribe(Close).DisposeWith(disposables); });

		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}