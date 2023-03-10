using FluentAssertions;
using NetStalker.Tests.AutoData;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;

namespace NetStalker.Tests.ViewModelTests
{
	public class MainViewModelTests
	{
		#region Routing

		[Theory, AutoServiceData]
		public void CanNavigateToRules(IRouter router, RuleBuilderViewModel ruleBuilderViewModel)
		{
			var sut = new MainViewModel(router, null, null, null, null, null, ruleBuilderViewModel, null, null, null);

			sut.GoToRules.Execute();

			sut.Router.CurrentViewModel.Subscribe(x => x.Should().Be(ruleBuilderViewModel));

			sut.Dispose();
		}

		#endregion
	}
}