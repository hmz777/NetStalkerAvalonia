using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Services
{
	public interface IStatusMessageService
	{
		void ShowMessage(StatusMessageModel statusMessageModel);
	}
}
