using FluentAssertions;
using NetStalker.Tests.AutoData;
using NetStalkerAvalonia.Models;

namespace NetStalker.Tests.ServiceSpecs
{
	public class BlockerRedirectorSpec : BlockerRedirectorSpecBase
	{
		[Theory, AutoDeviceData]
		public void Can_Block_A_Device(Device device)
		{
			BlockerRedirectorService.Block(device);

			device.Blocked.Should().Be(true);
			device.Redirected.Should().Be(false);
		}

		[Theory, AutoDeviceData]
		public void Can_UnBlock_A_Device(Device device)
		{
			BlockerRedirectorService.UnBlock(device);

			device.Blocked.Should().Be(false);
			device.Redirected.Should().Be(false);
		}

		[Theory, AutoDeviceData]
		public void Can_Redirect_A_Device(Device device)
		{
			BlockerRedirectorService.Redirect(device);

			device.Redirected.Should().Be(true);
			device.Blocked.Should().Be(false);
		}

		[Theory, AutoDeviceData]
		public void Can_UnRedirect_A_Device(Device device)
		{
			BlockerRedirectorService.UnRedirect(device);

			device.Redirected.Should().Be(false);
			device.Blocked.Should().Be(false);
		}

		[Theory, AutoDeviceData]
		public void Can_Limit_A_Device(Device device)
		{
			BlockerRedirectorService.Limit(device, 15, 12);

			device.Redirected.Should().Be(true);
			device.DownloadCap.Should().Be(15 * 1024);
			device.UploadCap.Should().Be(12 * 1024);
		}
	}
}