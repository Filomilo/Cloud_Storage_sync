using Lombok.NET;

namespace Cloud_Storage_Common.Models
{
    public enum MESSAGE_TYPE
    {
        UPDATE,
        TEXT,
    }

    public class MessageData
    {
        public string text { get; set; }
        public UpdateFileDataRequest FlieUpdate { get; set; }
    }

    [AllArgsConstructor]
    [NoArgsConstructor]
    public partial class WebSocketMessage
    {
        public MESSAGE_TYPE messageType { get; set; }

        private MessageData _data = new MessageData();

        public MessageData data
        {
            get { return _data; }
            set { _data = value; }
        }

        public WebSocketMessage(UpdateFileDataRequest data)
        {
            this.messageType = MESSAGE_TYPE.UPDATE;
            this._data.FlieUpdate = data;
        }

        public WebSocketMessage(string text)
        {
            this.messageType = MESSAGE_TYPE.TEXT;
            this._data.text = text;
        }

        public bool Equals(WebSocketMessage other)
        {
            if (other == null)
                return false;
            if (this.messageType != other.messageType)
                return false;
            switch (this.messageType)
            {
                case MESSAGE_TYPE.UPDATE:
                    return this._data.FlieUpdate.Equals(other._data.FlieUpdate);
                case MESSAGE_TYPE.TEXT:
                    return this.messageType == MESSAGE_TYPE.TEXT
                        && this._data.text == other._data.text;
                default:
                    return false;
            }
        }
    }
}
