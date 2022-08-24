namespace NetStalkerAvalonia.Models;

public class StatusMessage
{
    public MessageType MessageType { get; set; }
    public string Message { get; set; }

    public StatusMessage(MessageType messageType, string message)
    {
        MessageType = messageType;
        Message = message;
    }
}