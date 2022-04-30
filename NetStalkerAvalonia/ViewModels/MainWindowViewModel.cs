using NetStalkerAvalonia.Components.DeviceList;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.ViewModels.RoutedViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive;

namespace NetStalkerAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new RoutingState();

        // Commands to navigate the different views
        public ReactiveCommand<Unit, IRoutableViewModel> GoToHome { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoToRules { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoToSniffer { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoToOptions { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> GoToHelp { get; }

        // The command that navigates a user back.
        public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;

        public MainWindowViewModel()
        {
            // Manage the routing state. Use the Router.Navigate.Execute
            // command to navigate to different view models. 
            //
            // Note, that the Navigate.Execute method accepts an instance 
            // of a view model, this allows you to pass parameters to 
            // your view models, or to reuse existing view models.
            //

            // Navigation wiring

            GoToSniffer = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new SnifferViewModel(this))
            );

            // Dummy data population
            PopulateDummyData(Devices);
        }

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
                         Name = $"Dev {i}"
                     });
            }

            return devices;
        }

        #endregion
    }
}