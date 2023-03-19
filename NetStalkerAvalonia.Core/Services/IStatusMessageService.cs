using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.Services
{
	public interface IStatusMessageService
	{
		void ShowMessage(StatusMessageModel statusMessageModel);
	}
}
