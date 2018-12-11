namespace Pong.Framework.Messages
{
    internal static class MailBox
    {
        public static void Send(MessageEnvelope envelope)
        {
            ModEntry.Instance.Helper.Multiplayer.SendMessage(envelope.Data, envelope.MessageType, new[] { ModEntry.Instance.PongId }, new[] { envelope.RecipientId });
        }
    }
}
