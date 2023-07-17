using NetStalkerAvalonia.Core.Models;
using System;
using System.Collections.ObjectModel;

namespace NetStalkerAvalonia.Core.Services
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