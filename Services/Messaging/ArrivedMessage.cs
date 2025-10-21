using CommunityToolkit.Mvvm.Messaging.Messages;


namespace RHF_Foundation.Messaging;


public sealed class ArrivedMessage : ValueChangedMessage<string>
{
public ArrivedMessage(string placeId) : base(placeId) { }
}