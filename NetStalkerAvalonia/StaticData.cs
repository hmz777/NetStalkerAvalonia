using Avalonia.Controls;
using NetStalkerAvalonia.ViewModels;
using System.Collections.Generic;

namespace NetStalkerAvalonia
{
	public class StaticData
	{
		public static Window? MainWindow { get; set; }

		public static List<ViewModelBase> ViewModels = new();
	}
}