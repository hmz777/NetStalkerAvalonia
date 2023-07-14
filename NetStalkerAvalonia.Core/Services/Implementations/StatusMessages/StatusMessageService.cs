using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.Services.Implementations.StatusMessages
{
	public class StatusMessageService : IStatusMessageService
	{
		public static Interaction<StatusMessageModel, Unit> MessageInteraction = new();

		public async Task ShowMessage(StatusMessageModel statusMessage)
		{
			await MessageInteraction.Handle(statusMessage);
		}
	}
}