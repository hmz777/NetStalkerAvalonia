using NetStalkerAvalonia.Core.Configuration;
using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;
using ReactiveUI;
using System.Diagnostics;

namespace NetStalkerAvalonia.Core.Helpers;

public class Tools
{
	public static void OpenLink(string link)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = link,
			UseShellExecute = true
		});
	}
}