using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;
using System;

namespace NetStalkerAvalonia.Views;

public partial class StatusMessageWindow : ReactiveWindow<StatusMessageViewModel>
{
	public StatusMessageWindow()
	{
		this.WhenActivated(disposables => { ViewModel!.Close.Subscribe(x => Close()).DisposeWith(disposables); });

		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}