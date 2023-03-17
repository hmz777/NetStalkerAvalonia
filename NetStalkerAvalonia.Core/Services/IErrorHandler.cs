using NetStalkerAvalonia.ViewModels.InteractionViewModels;

namespace NetStalkerAvalonia.Core.Services
{
	public interface IErrorHandler
	{
		void HandleError(StatusMessageModel statusMessageModel);
	}
}
