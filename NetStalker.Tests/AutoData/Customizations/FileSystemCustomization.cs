using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalker.Tests.AutoData.Customizations
{
	public class FileSystemCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customizations.Add(new TypeRelay(typeof(IFileSystem), typeof(FileSystem)));
			fixture.Register(() => new FileSystem());
		}
	}
}
