using CommunityToolkit.Mvvm.Messaging.Messages;


namespace RHF_Foundation.Services.Messaging;


public sealed class DepartedMessage : ValueChangedMessage<string>
{
    public DepartedMessage(string placeId) : base(placeId) { }
}