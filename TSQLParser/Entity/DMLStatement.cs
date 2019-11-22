using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTTools.Classes
{
    public class DMLStatement
    {
        private static int MAX_BATCH_SIZE = 10000;
        public static void ProcessNoLockHint(NamedTableReference tblRef, ref Dictionary<string, string> messages)
        {
            string tblName = tblRef.SchemaObject.BaseIdentifier.Value;
            if (!tblName.StartsWith("#") || !tblName.StartsWith("@"))
            {
                IList<TableHint> tableHint = tblRef.TableHints;
                bool nolockHint = false;
                if (tableHint.Count == 0)
                {
                    messages.Add(Message.GetJsonMessage(MessageType.Advisory, tblRef.StartLine,
                                    (tblRef.StartColumn + tblName.Length + 1), ObjectType.Table),
                                    "Consider using a <b>[WITH](NOLOCK)</b> on <b>" + tblName + "</b>");
                }
                else
                {
                    foreach (TableHint tblHint in tableHint)
                    {
                        if (tableHint[0].HintKind.ToString().ToLower().Equals("nolock"))
                        {
                            nolockHint = true;
                            break;
                        }
                    }
                    if (!nolockHint)
                        messages.Add(Message.GetJsonMessage(MessageType.Advisory, tblRef.StartLine,
                                        (tblRef.StartColumn + tblName.Length + 1), ObjectType.Index),
                                    "Consider using a <b>[WITH](NOLOCK)</b> on <b>" + tblName + "</b>");
                }
            }
        }

        public static void ProcessLinkServerReference(NamedTableReference tblRef, ref Dictionary<string, string> messages)
        {
            Identifier linkServer = tblRef.SchemaObject.ServerIdentifier;
            if (linkServer != null)
            {
                string linkServerName = tblRef.SchemaObject.ServerIdentifier.Value;
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, tblRef.StartLine,
                                    (tblRef.StartColumn + linkServerName.Length + 1), ObjectType.Index),
                                    "Avoid <b>Link Server</b> reference <b>(" + linkServerName + 
                                    ")</b>. Ask DBA for Replication possibility.");
            }            
        }
        public static void ProcessQualifiedJoins(QualifiedJoin joinTable, ref Dictionary<string, string> messages)
        {
            if(joinTable.FirstTableReference is QualifiedJoin)
            {
                ProcessQualifiedJoins((QualifiedJoin)joinTable.FirstTableReference, ref messages);
            }
            else
            {
                ProcessNoLockHint((NamedTableReference)joinTable.FirstTableReference, ref messages);
                ProcessNoLockHint((NamedTableReference)joinTable.SecondTableReference, ref messages);
                ProcessLinkServerReference((NamedTableReference)joinTable.FirstTableReference, ref messages);
                ProcessLinkServerReference((NamedTableReference)joinTable.SecondTableReference, ref messages);
            }
        }
        public static Dictionary<string, string> ProcessSelectStatement(TSqlStatement stmt)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            SelectStatement select = (SelectStatement)stmt;
            QuerySpecification querySpecification = (QuerySpecification)select.QueryExpression;
            FromClause fromClause = querySpecification.FromClause;
            IList<TableReference> tableReference = fromClause.TableReferences;

            foreach (TableReference tbl in tableReference)
            {
                if (tbl is QualifiedJoin)
                {
                    ProcessQualifiedJoins(((QualifiedJoin)tbl), ref messages);
                }
                else if(tbl is NamedTableReference)
                {
                    ProcessNoLockHint((NamedTableReference)tbl, ref messages);
                    ProcessLinkServerReference((NamedTableReference)tbl, ref messages);
                }
            }
            return messages;
        }
        public static Dictionary<string, string> ProcessDeleteStatement(TSqlStatement stmt)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            DeleteStatement delete = (DeleteStatement)stmt;
            
            TopRowFilter filter = delete.DeleteSpecification.TopRowFilter;
            if (filter == null)
            {
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, delete.StartLine,
                                delete.StartColumn, ObjectType.Table),
                                "Delete must have <b>TOP(@batchsize)</b> clause");
            }
            else if (Int32.Parse(((IntegerLiteral)((ParenthesisExpression)filter.Expression).Expression).Value) > MAX_BATCH_SIZE)
            {
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, delete.StartLine,
                                delete.StartColumn, ObjectType.Table),
                                "<b>TOP @batchsize</b> must be <b><= " + MAX_BATCH_SIZE + "</b>");
            }
            return messages;
        }
        public static Dictionary<string, string> ProcessInsertStatement(TSqlStatement stmt)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            InsertStatement insert = (InsertStatement)stmt;
            TopRowFilter filter = insert.InsertSpecification.TopRowFilter;
            if (filter == null)
            {
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, insert.StartLine,
                                insert.StartColumn, ObjectType.Table),
                                "Insert must have <b>TOP(@batchsize)</b> clause");
            }
            else if (Int32.Parse(((IntegerLiteral)((ParenthesisExpression)filter.Expression).Expression).Value) > MAX_BATCH_SIZE)
            {
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, insert.StartLine,
                                insert.StartColumn, ObjectType.Table),
                                "<b>TOP @batchsize</b> must be <b><= " + MAX_BATCH_SIZE + "</b>");
            }
            return messages;
        }
        public static Dictionary<string, string> ProcessUpdateStatement(TSqlStatement stmt)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            UpdateStatement update = (UpdateStatement)stmt;

            IList<TSqlParserToken> sqlFragment = update.ScriptTokenStream;
            TopRowFilter filter = update.UpdateSpecification.TopRowFilter;
            if (filter == null)
            {
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, update.StartLine,
                                update.StartColumn, ObjectType.Table),
                                "Update must have <b>TOP(@batchsize)</b> clause");
            }
            else if (Int32.Parse(((IntegerLiteral)((ParenthesisExpression)filter.Expression).Expression).Value) > MAX_BATCH_SIZE)
            {
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, update.StartLine,
                                update.StartColumn, ObjectType.Table),
                                "<b>TOP @batchsize</b> must be <b><= " + MAX_BATCH_SIZE + "</b>");
            }
            return messages;
        }
    }
}
