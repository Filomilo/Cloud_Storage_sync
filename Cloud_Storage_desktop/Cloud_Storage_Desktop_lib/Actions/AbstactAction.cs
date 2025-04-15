using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Actions
{
    public enum ActionType
    {
        UPLOAD_ACTION,
        DOWNLOAD_ACTION,
        DELETE_ACTION,
        RENAME_ACTION,
    }

    public abstract class AbstactAction : ITaskToRun
    {
        protected string file;
        public abstract ActionType ActionType { get; }
        public object Id
        {
            get { return file; }
        }

        public abstract Action ActionToRun { get; }
    }
}
