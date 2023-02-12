using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using NetStalkerAvalonia.Models;

namespace NetStalkerAvalonia.Services
{
	public interface IBlockerRedirector : IService, IDisposable
	{
		void Block(Device device);
		void UnBlock(Device device);
		void Redirect(Device device);
		void UnRedirect(Device device);
		void Limit(Device device, int download, int upload);

		public ReadOnlyObservableCollection<Device> Devices { get; }
	}
}