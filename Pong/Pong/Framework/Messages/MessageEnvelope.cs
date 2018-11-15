namespace Pong.Framework.Messages
{
    internal class MessageEnvelope
    {
        public string MessageType { get; }
        public long RecipientId { get; }
        public object Data { get; }

        public MessageEnvelope(object data, long recipientId)
        {
            this.Data = data;
            this.MessageType = data.GetType().Name;
            this.RecipientId = recipientId;
        }
    }
}
