using FluentAssertions;
using NetStalker.Tests.AutoData;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.Services.Implementations.BlockingRedirection;

namespace NetStalker.Tests.ServiceSpecs
{
	public class BlockerRedirectorSpec
	{
		[Theory, AutoServiceData]
		public void Can_Block_A_Device(IRuleService ruleService, IPcapDeviceManager pcapDeviceManager, Device device)
		{
			var sut = new BlockerRedirector(ruleService, pcapDeviceManager);

			sut.Block(device);

			device.Blocked.Should().Be(true);
			device.Redirected.Should().Be(false);

			sut.Dispose();
		}

		[Theory, AutoServiceData]
		public void Can_UnBlock_A_Device(IRuleService ruleService, IPcapDeviceManager pcapDeviceManager, Device device)
		{
			var sut = new BlockerRedirector(ruleService, pcapDeviceManager);

			sut.UnBlock(device);

			device.Blocked.Should().Be(false);
			device.Redirected.Should().Be(false);

			sut.Dispose();
		}

		[Theory, AutoServiceData]
		public void Can_Redirect_A_Device(IRuleService ruleService, IPcapDeviceManager pcapDeviceManager, Device device)
		{
			var sut = new BlockerRedirector(ruleService, pcapDeviceManager);

			sut.Redirect(device);

			device.Redirected.Should().Be(true);
			device.Blocked.Should().Be(false);

			sut.Dispose();
		}

		[Theory, AutoServiceData]
		public void Can_UnRedirect_A_Device(IRuleService ruleService, IPcapDeviceManager pcapDeviceManager, Device device)
		{
			var sut = new BlockerRedirector(ruleService, pcapDeviceManager);

			sut.UnRedirect(device);

			device.Redirected.Should().Be(false);
			device.Blocked.Should().Be(false);

			sut.Dispose();
		}

		[Theory, AutoServiceData]
		public void Can_Limit_A_Device(IRuleService ruleService, IPcapDeviceManager pcapDeviceManager, Device device)
		{
			var sut = new BlockerRedirector(ruleService, pcapDeviceManager);

			sut.Limit(device, 15, 12);

			device.Redirected.Should().Be(true);
			device.DownloadCap.Should().Be(15 * 1024);
			device.UploadCap.Should().Be(12 * 1024);

			sut.Dispose();
		}
	}
}