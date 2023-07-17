using FluentAssertions;
using NetStalker.Tests.AutoData;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.ViewModels;
using NetStalkerAvalonia.Core.ViewModels.RoutedViewModels;

namespace NetStalker.Tests.ViewModelTests
{
	public class MainViewModelTests
	{
		#region Routing

		[Theory, AutoServiceData]
		public void CanNavigateToRules(IRouter router, HomeViewModel homeViewModel, RuleBuilderViewModel ruleBuilderViewModel)
		{
			var sut = new MainViewModel(router, homeViewModel, null, null, ruleBuilderViewModel, null, null, null);

			sut.GoToRules.Execute();

			sut.Router.CurrentViewModel.Subscribe(x => x.Should().Be(ruleBuilderViewModel));
		}

		#endregion
	}
}