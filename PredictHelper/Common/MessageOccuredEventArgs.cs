using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredictHelper.Common
{
    public class MessageOccuredEventArgs : EventArgs
    {
        public string Message { get; set; }
        public MessageImportance MsgImportance { get; set; }

        public MessageOccuredEventArgs(string message, MessageImportance msgImportance)
        {
            Message = message;
            MsgImportance = msgImportance;
        }
    }
}