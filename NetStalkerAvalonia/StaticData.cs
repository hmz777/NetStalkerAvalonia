using Avalonia.Controls;
using NetStalkerAvalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia
{
	public class StaticData
	{
		public static Window? MainWindow { get; set; }
		public static List<ViewModelBase> ViewModels = new();
	}
}
