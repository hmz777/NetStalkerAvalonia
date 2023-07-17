using NetStalkerAvalonia.Core.Models;

namespace NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;

public class StatusMessageModel
{
    public MessageType MessageType { get; set; }
    public string Message { get; set; }

    public StatusMessageModel(MessageType messageType, string message)
    {
        MessageType = messageType;
        Message = message;
    }
}