using NetStalkerAvalonia.Components.DeviceList;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

namespace NetStalkerAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        #region Routing

        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new RoutingState();

        // Commands to navigate the different views
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToHome { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToRules { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToSniffer { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToOptions { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? GoToHelp { get; }

        // The command that navigates a user back.
        public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;
        public void GoBackHandler() { GoBack?.Execute(); }

        private readonly ObservableAsPropertyHelper<bool> _canGoBack;
        public bool CanGoBack => _canGoBack.Value;

        private Stack<string> ViewNames = new();

        #endregion

        #region Properties

        #region UI Info

        private readonly ObservableAsPropertyHelper<string> _pageTitle;
        public string PageTitle => _pageTitle.Value;

        #endregion

        #region Devices List

        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public DeviceListViewSettings DeviceListViewSettings { get; set; } = new();

        #region Testing

        private static ICollection<Device> PopulateDummyData(ICollection<Device> devices)
        {
            for (int i = 0; i < 50; i++)
            {
                devices.Add(
                     new Device()
                     {
                         IP = $"192.168.1.{i}",
                         Mac = $"fffffff{i}",
                         Blocked = i % 2 == 0,
                         Redirected = i + 1 % 2 == 0,
                         Download = $"{i} Kb/s",
                         Upload = $"{i} Kb/s",
                         Name = $"Dev {i}",
                         Type = DeviceType.PC
                     });
            }

            return devices;
        }

        #endregion

        #endregion

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            // Info wiring
            _pageTitle = this.WhenAnyObservable(x => x.Router.CurrentViewModel)
                             .Select(x => GetPageNameFromViewModel(x))
                             .ToProperty(this, x => x.PageTitle);

            // Navigation wiring
            _canGoBack = this.WhenAnyValue(x => x.Router.NavigationStack.Count)
                             .Select(count => count > 0)
                             .ToProperty(this, x => x.CanGoBack);

            GoToSniffer = ReactiveCommand.CreateFromObservable(
                () => { ViewNames.Push("Packet Sniffer"); return Router.Navigate.Execute(new SnifferViewModel(this)); });

            // Dummy data population
            PopulateDummyData(Devices);
        }

        #endregion

        #region Tools

        //We get the view name, othewise we return the initial name
        private static string GetPageNameFromViewModel(IRoutableViewModel routableViewModel)
        {
            return routableViewModel?.UrlPathSegment ?? "Device List";
        }

        #endregion
    }
}