using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels;
using ReactiveUI;
using System;

namespace NetStalkerAvalonia.Core.Views
{
	public partial class AddUpdateRuleView : ReactiveWindow<AddUpdateRuleViewModel>
	{
		public AddUpdateRuleView()
		{
			this.WhenActivated(d => d(ViewModel!.Accept.Subscribe(Close)));
			InitializeComponent();
		}
	}
}
