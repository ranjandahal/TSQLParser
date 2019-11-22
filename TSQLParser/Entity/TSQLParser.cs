using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTTools.Classes
{
    class TSQLParser
    {                
        public string TextToParse { get; set; }
        public TSQLParser() { }

        public TSQLParser(String textToParse)
        {
            this.TextToParse = textToParse;
        }

        public Dictionary<string, string> parse(string content)
        {
            TSql120Parser SqlParser = new TSql120Parser(false);

            IList<ParseError> parseErrors;
            Dictionary<string, string> messages = new Dictionary<string, string>();
            
            TSqlFragment result = SqlParser.Parse(new StringReader(content),
                                                  out parseErrors);
            if (parseErrors != null && parseErrors.Count == 0)
            {
                TSqlScript SqlScript = result as TSqlScript;
                Dictionary<string, string> tempmessages = new Dictionary<string, string>();
                foreach (TSqlBatch sqlBatch in SqlScript.Batches)
                {
                    foreach (TSqlStatement sqlStatement in sqlBatch.Statements)
                    {
                        tempmessages = ProcessStatementBody(sqlStatement);
                        messages = messages.Union(tempmessages).ToDictionary(k => k.Key, v => v.Value);
                    }
                }
            }
            foreach (ParseError err in parseErrors)
            {
                messages.Add(Message.GetJsonMessage(MessageType.Error, err.Line, err.Column, ObjectType.Rule, err.Number),
                                "Message: " + err.Message);
            }
            if (messages.Count == 0)
                messages.Add(Message.GetJsonMessage(MessageType.Error, 0, 0, ObjectType.Rule, -1), 
                            "No errors found!!");
            return messages;
        }
        private static Dictionary<string, string> ProcessStatementBody(TSqlStatement statement)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            if (statement is SelectStatement)
            {
                return DMLStatement.ProcessSelectStatement(statement);
            }
            else if (statement is InsertStatement)
            {
                return DMLStatement.ProcessInsertStatement(statement);
            }
            else if (statement is UpdateStatement)
            {
                return DMLStatement.ProcessUpdateStatement(statement);
            }
            else if (statement is DeleteStatement)
            {
                return DMLStatement.ProcessDeleteStatement(statement);
            }
            else if (statement is CreateTableStatement)
            {
                return DDLStatement.ProcessCreateTableStatement(statement);
            }
            else if (statement is CreateOrAlterViewStatement ||
                     statement is CreateViewStatement ||
                     statement is AlterViewStatement)
            {
                CreateViewStatement createViewStatement = (CreateViewStatement)statement;
                
                return DDLStatement.ProcessCreateOrAlterViewStatement(statement);
            }
            else if (statement is CreateOrAlterProcedureStatement ||
                     statement is CreateProcedureStatement ||
                     statement is AlterProcedureStatement)
            {
                CreateProcedureStatement createProcedureStatement = (CreateProcedureStatement)statement;
                
            }
            else if (statement is CreateOrAlterFunctionStatement ||
                     statement is CreateFunctionStatement ||
                     statement is AlterFunctionStatement)
            {
                CreateFunctionStatement createProcedureStatement = (CreateFunctionStatement)statement;
            }
            else if (statement is CreateOrAlterTriggerStatement ||
                     statement is CreateTriggerStatement ||
                     statement is AlterTriggerStatement)
            {
                CreateTriggerStatement createProcedureStatement = (CreateTriggerStatement)statement;
            }
            else if (statement is CreateIndexStatement)
            {
                return DDLStatement.ProcessCreateIndexStatement(statement);
            }
            else if (statement is IfStatement)
            {
                IfStatement ifStmt = (IfStatement)statement;                
                BeginEndBlockStatement stmt = (BeginEndBlockStatement)ifStmt.ThenStatement;

                if(stmt.StatementList.Statements.Count > 0)
                {
                    Dictionary<string, string> tempmessages = new Dictionary<string, string>();
                    foreach (TSqlStatement s in stmt.StatementList.Statements)
                    {
                        tempmessages = ProcessStatementBody(s);
                        messages = messages.Union(tempmessages).ToDictionary(k => k.Key, v => v.Value);
                    }
                }
            }
            else if (statement is BeginEndBlockStatement)
            {
                BeginEndBlockStatement stmt = (BeginEndBlockStatement)statement;

                if (stmt.StatementList.Statements.Count > 0)
                {
                    Dictionary<string, string> tempmessages = new Dictionary<string, string>();
                    foreach (TSqlStatement s in stmt.StatementList.Statements)
                    {
                        tempmessages = ProcessStatementBody(s);
                        messages = messages.Union(tempmessages).ToDictionary(k => k.Key, v => v.Value);
                    }
                }
            }
            else
                statement.ToString();
            return messages;
        }
    }
}
