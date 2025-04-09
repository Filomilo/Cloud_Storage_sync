using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Lombok.NET;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public SyncFileData syncFileData { get; set; }
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

        public WebSocketMessage(SyncFileData data)
        {
            this.messageType = MESSAGE_TYPE.UPDATE;
            this._data.syncFileData = data;
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
            return this.messageType == other.messageType
                && (
                    (
                        this.messageType == MESSAGE_TYPE.UPDATE
                        && this._data.syncFileData.Equals(other._data.syncFileData)
                    )
                    || (
                        this.messageType == MESSAGE_TYPE.TEXT && this._data.text == other._data.text
                    )
                );
        }
    }
}
