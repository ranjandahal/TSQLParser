using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DTTools.Classes
{
    public enum MessageType
    {
        Advisory, Warning, Error
    }

    public enum ObjectType
    {
        CheckConstraint, DefaultConstraint, StoredProc, ScalarFunction, TVFunction, Table, View, Rule, Trigger, InlineTVFunction,
        DataType, NullType, Column, Index, UniqueConstraint
    }
    public class Message
    {
        public string MessageType { get; set; }
        public int ErrorNumber { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }                
        public string ObjectType { get; set; }

        public static string GetMessgeKey(MessageType messageType, int startLine, int startColumn)
        {
            return messageType + "-> Ln:" + startLine + " Col:" + startColumn; 
        }
        
        public static string GetJsonMessage(MessageType messageType, int startLine, 
                                            int startColumn, ObjectType objectType, int errorNumber = 0)
        {
            var messageKey = new Message
            {
                MessageType = messageType.ToString(),
                ErrorNumber = errorNumber,
                Line = startLine,
                Column = startColumn,
                ObjectType = objectType.ToString()
            };
            return new JavaScriptSerializer().Serialize(messageKey);
        }
    }
}
