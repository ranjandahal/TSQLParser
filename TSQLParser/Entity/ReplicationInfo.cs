using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTTools.Classes
{
    public enum ObjectTypes
    {
        IF
        ,U
        ,FS
        ,PC
        ,P
        ,V
        ,FT
        ,TF
        ,FN
    }

    public class ReplicationInfo
    {
        public string ObjectName { get; set; }
        public string Publication { get; set; }
        public string SourceDatabase { get; set; }
        public string SourceServer { get; set; }
        public string DestinationDatabase { get; set; }
        public string DestinationServer { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public bool IsSchemaOnly { get; set; }
        //public ObjectType ObjType { get; set; }
        public string ObjectTypes { get; set; }

        /*public ReplicationInfo(
            string objectName,
            string publicationName,
            string sourceServerName,
            string sourceDatabaseName,
            string destinationServerName,
            string destinationDatabaseName,
            bool isSchemaOnly,
            string objectType)
        {
            this.ObjectName = objectName;
            this.PublicationName = publicationName;
            this.SourceDatabaseName = sourceDatabaseName;
            this.SourceServerName = sourceServerName;
            this.DestinationDatabaseName = destinationDatabaseName;
            this.DestinationServerName = destinationServerName;
            this.ObjectType = objectType;
        }

        //public ReplicationInfo() { }*/

    }
}
