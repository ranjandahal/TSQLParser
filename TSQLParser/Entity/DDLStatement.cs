using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PoorMansTSqlFormatterLib.Formatters;

namespace DTTools.Classes
{
    public class DDLStatement
    {
        TSqlStandardFormatter formatter = new TSqlStandardFormatter();
        
        public static Dictionary<string, string> ProcessCreateTableStatement(TSqlStatement stmt)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            CreateTableStatement createTableStatement = (CreateTableStatement)stmt;
            string tableName = createTableStatement.SchemaObjectName.BaseIdentifier.Value;
            TableDefinition tblDefinition = createTableStatement.Definition;

            foreach (ColumnDefinition cd in tblDefinition.ColumnDefinitions)
            {
                SqlDataTypeReference dtReference = (SqlDataTypeReference)cd.DataType;
                if (cd.Constraints.Count == 0)
                {
                    messages.Add(Message.GetJsonMessage(MessageType.Advisory, cd.StartLine, cd.StartColumn, ObjectType.NullType),
                                    "<b>NULL</b> or <b>NOT NULL</b> should be explicitly defined.");
                }
                else
                {
                    if (dtReference.SqlDataTypeOption.ToString().Equals("Bit") 
                        && ((NullableConstraintDefinition)cd.Constraints[0]).Nullable)
                    {
                        messages.Add(Message.GetJsonMessage(MessageType.Advisory, cd.StartLine, cd.StartColumn, ObjectType.NullType),
                                        "<b>BIT</b> columns should rarely, if ever, be nullable.");
                    }
                }
                
                if (dtReference.SqlDataTypeOption.ToString().Equals("NVarChar") &&
                    dtReference.Parameters.Count > 0 && dtReference.Parameters[0].Value.ToLower().Equals("max"))
                {
                    messages.Add(Message.GetJsonMessage(MessageType.Advisory, cd.StartLine, cd.StartColumn, ObjectType.DataType),
                                    "Use defined size rather than <b>MAX</b>");
                }
                if (cd.DefaultConstraint != null && cd.DefaultConstraint.ConstraintIdentifier == null)
                {
                    messages.Add(Message.GetJsonMessage(MessageType.Advisory, cd.StartLine, cd.StartColumn, ObjectType.DefaultConstraint),
                                    "Use named <b>Constraint</b> like <b>DF_" + tableName + "_" + 
                                    cd.ColumnIdentifier.Value + "</b>");
                }
                if (cd.DefaultConstraint != null && cd.DefaultConstraint.ConstraintIdentifier != null)
                {
                    string constraint = Utilities.GetColumnConstraintName(ConstraintType.DF, tableName, cd);
                    if(!constraint.ToLower().Equals(cd.DefaultConstraint.ConstraintIdentifier.Value.ToLower()))
                        messages.Add(Message.GetJsonMessage(MessageType.Advisory, cd.StartLine, cd.StartColumn, ObjectType.DefaultConstraint),
                                    "Invalid Constraint name. Change <b>Constraint</b> to <b>DF_" + tableName + "_" +
                                    cd.ColumnIdentifier.Value + "</b>");
                }
            }
            if (tblDefinition.TableConstraints.Count > 0)
            {
                foreach (ConstraintDefinition cd in tblDefinition.TableConstraints)
                {
                    string constraintName = cd.ConstraintIdentifier.Value;
                    if (cd is DefaultConstraintDefinition)
                    {

                    }
                    else if (cd is UniqueConstraintDefinition)
                    {
                        UniqueConstraintDefinition uniqueConstraint = (UniqueConstraintDefinition)cd;
                        string constName = (uniqueConstraint.IsPrimaryKey ? "PK_" : "UC_") + tableName;              
                        foreach (ColumnWithSortOrder cwso in uniqueConstraint.Columns)
                        {
                            foreach (Identifier identifier in cwso.Column.MultiPartIdentifier.Identifiers)
                            {
                                constName += "_" + identifier.Value;
                            }
                        }

                        if (!constName.ToLower().Equals(constraintName.ToLower()))
                        {
                            messages.Add(Message.GetJsonMessage(MessageType.Advisory, cd.StartLine, cd.StartColumn, ObjectType.DefaultConstraint),
                                        "Invalid constraint name, name <b>Constraint</b> like <b>" + constName + "</b>");
                        }

                        if (uniqueConstraint.OnFileGroupOrPartitionScheme != null)
                        {
                            messages.Add(Message.GetJsonMessage(MessageType.Advisory, uniqueConstraint.OnFileGroupOrPartitionScheme.StartLine,
                                                                uniqueConstraint.OnFileGroupOrPartitionScheme.StartColumn, ObjectType.UniqueConstraint),
                                            "Remove FileGroup option <b> ON [" +
                                            uniqueConstraint.OnFileGroupOrPartitionScheme.Name.Value + "]</b>");
                        }
                    }
                    else if (cd is CheckConstraintDefinition)
                    {

                    }
                    else if (cd is ForeignKeyConstraintDefinition)
                    {

                    }


                }
            }
            foreach(TableOption opt in createTableStatement.Options){
                if (opt is TableDataCompressionOption)
                {

                }
                else if (opt is TablePartitionOption)
                {
                }
            }
            if(createTableStatement.OnFileGroupOrPartitionScheme != null)
            {
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, createTableStatement.OnFileGroupOrPartitionScheme.StartLine,
                                                    createTableStatement.OnFileGroupOrPartitionScheme.StartColumn, ObjectType.Table),
                                "Remove FileGroup options <b>ON [" + 
                                createTableStatement.OnFileGroupOrPartitionScheme.Name.Value + "]</b>");
            }
            return messages;
        }

        public static Dictionary<string, string> ProcessCreateOrAlterViewStatement(TSqlStatement stmt)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            String viewName = "";
            if (stmt is CreateViewStatement)
            {
                CreateViewStatement cView = (CreateViewStatement)stmt;
                viewName = cView.SchemaObjectName.Identifiers.Count > 0 ?
                                  cView.SchemaObjectName.Identifiers[0].Value : "";
            }
            else if (stmt is AlterViewStatement)
            {
                CreateViewStatement cView = (CreateViewStatement)stmt;
                viewName = cView.SchemaObjectName.Identifiers.Count > 0 ?
                                  cView.SchemaObjectName.Identifiers[0].Value : "";
            }
            else
            {
                CreateOrAlterViewStatement cView = (CreateOrAlterViewStatement)stmt;
                viewName = cView.SchemaObjectName.Identifiers.Count > 0 ?
                                  cView.SchemaObjectName.Identifiers[0].Value : "";
            }
            if(!viewName.ToLower().StartsWith("vw"))
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, stmt.StartLine, stmt.StartColumn, ObjectType.View),
                              "View name should begin with <b>vw</b>");
            return messages;
        }

        public static Dictionary<string, string> ProcessCreateIndexStatement(TSqlStatement stmt)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            CreateIndexStatement createIndexStatement = (CreateIndexStatement)stmt;

            string indexName = createIndexStatement.Name.Value;
            string tableName = createIndexStatement.OnName.BaseIdentifier.Value;

            string contraintName = Utilities.GetTableConstraintName(ConstraintType.IX, tableName,
                                                                    createIndexStatement.Columns, 
                                                                    (createIndexStatement.IncludeColumns.Count > 0));
            if (!indexName.ToLower().Equals(contraintName.ToLower()))
            {
                messages.Add(Message.GetJsonMessage(MessageType.Advisory, createIndexStatement.StartLine, 
                                        createIndexStatement.StartColumn, ObjectType.Index),
                             "Index name should be <b>" + contraintName + "</b>");
            }
            return messages;
        }
    }
}
