using System;

namespace PredictHelper.Common
{
    public class MessageOccuredEventArgs : EventArgs
    {
        public string Message { get; set; }
        public MessageImportance MsgImportance { get; set; }
        public Exception Ex { get; set; }

        public MessageOccuredEventArgs(string message, MessageImportance msgImportance, Exception ex = null)
        {
            Message = message;
            MsgImportance = msgImportance;
        }
    }
}