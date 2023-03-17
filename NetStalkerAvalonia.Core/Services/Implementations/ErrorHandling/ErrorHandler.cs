using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using Serilog;
using System;

namespace NetStalkerAvalonia.Core.Services.Implementations.ErrorHandling
{
	public class ErrorHandler : IErrorHandler
	{
		private readonly IStatusMessageService statusMessageService;

		public ErrorHandler(IStatusMessageService statusMessageService)
		{
			this.statusMessageService = statusMessageService;
		}

		public void HandleError(StatusMessageModel statusMessage)
		{
			ArgumentNullException.ThrowIfNull(statusMessage, nameof(statusMessage));

			Log.Error("Exception triggered with message:{Message}", statusMessage.Message);

			statusMessageService.ShowMessage(statusMessage);
		}
	}
}