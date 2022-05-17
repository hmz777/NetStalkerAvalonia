using NetStalkerAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NetStalkerAvalonia.Services.Implementations.BlockingRedirection
{
    public class BlockerRedirector : IBlockerRedirector
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isStarted;
        private LibPcapLiveDevice? _device;

        private List<Device>? Clients { get; set; }

        public BlockerRedirector()
        {
            InitDevice();
            BindClients();
        }

        private void InitDevice()
        {
            if (_device == null)
            {
                _device = (LibPcapLiveDevice)CaptureDeviceList.New()[HostInfo.NetworkAdapterName];
                _device.Open(DeviceModes.Promiscuous, 1000);
                _device.Filter = "ether proto \\ip";
                _device.OnPacketArrival += DeviceOnOnPacketArrival;
            }
        }

        private void DeviceOnOnPacketArrival(object sender, PacketCapture e)
        {
            // TODO: Implement packet processing logic
        }

        private void BindClients()
        {
            this.WhenAnyObservable(x => MessageBus.Current
                    .Listen<ReadOnlyObservableCollection<Device>>(null))
                .BindTo(this, x => x.Clients);
        }

        private void Start()
        {
            if (_cancellationTokenSource == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
            else if (_cancellationTokenSource.IsCancellationRequested)
            {
                if (_cancellationTokenSource.TryReset() == false)
                {
                    throw new InvalidOperationException("Can not reset the BlockerRedirector's cancellation token.");
                }
            }

            if (_isStarted == false)
            {
                if (_device is not null && _device is { Opened: true, Started: false })
                {
                    _device.StartCapture();

                    Task.Run(() =>
                    {
                        while (_cancellationTokenSource.IsCancellationRequested == false)
                        {
                            SpoofClients();
                        }
                    });

                    _isStarted = true;
                }
            }
        }

        private void SpoofClients()
        {
            // TODO: Implement client spoofing logic
        }

        private void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _isStarted = false;
            _device?.StopCapture();
        }

        public void Destroy()
        {
            Stop();

            if (_device != null)
            {
                _device.Close();
                _device.OnPacketArrival -= DeviceOnOnPacketArrival;
                _device.Dispose();
                _device = null;
            }
        }

        public void Block(Device device)
        {
            device.Block();
            Start();
        }

        public void Redirect(Device device)
        {
            device.Redirect();
            Start();
        }

        public void UnBlock(Device device)
        {
            device.UnBlock();
            Start();
        }

        public void UnRedirect(Device device)
        {
            device.UnRedirect();
            Start();
        }
    }
}