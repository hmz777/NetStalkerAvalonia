using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;
using System;

namespace NetStalkerAvalonia.Views
{
	public partial class AddUpdateRuleWindow : ReactiveWindow<AddUpdateRuleViewModel>
	{
		public AddUpdateRuleWindow()
		{
			this.WhenActivated(d => d(ViewModel!.Accept.Subscribe(Close)));
			InitializeComponent();
		}
	}
}
