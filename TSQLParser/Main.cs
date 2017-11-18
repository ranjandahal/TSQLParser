using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactSqlScriptDomTest
{
    class Main
    {
        static void Main(string[] args)
        {
            String text = @"
                            SELECT * from tblDBLocation
                            CREATE table (
                            id nvarchar(max) null
                            )";
            /*
            Select * from hello
            Create view vwHome as 
            select * from home
            ";/*
            t.object_id as Table_ObjectID,
            c.column_id as Column_ObjectID,
            c.name as Column_Name,
            t.Name as Table_Name
            FROM sys.views v
            JOIN sys.sql_dependencies d ON d.object_id = v.object_id
            JOIN sys.objects t ON t.object_id = d.referenced_major_id
            JOIN sys.columns c ON c.object_id = d.referenced_major_id AND
                 c.column_id = d.referenced_minor_id
            WHERE
            d.class < 2 AND
            v.name = 'View_1' AND
            v.schema_id = SCHEMA_ID('Dbo')
            ORDER BY COLUMN_ObjectID;";*/
            TSql120Parser SqlParser = new TSql120Parser(false);

            IList<ParseError> parseErrors;
            TSqlFragment result = SqlParser.Parse(new StringReader(text),
                                                  out parseErrors);

            TSqlScript SqlScript = result as TSqlScript;

            foreach (TSqlBatch sqlBatch in SqlScript.Batches)
            {
                foreach (TSqlStatement sqlStatement in sqlBatch.Statements)
                {
                    ProcessViewStatementBody(sqlStatement);
                }
            }
            Console.Read();
        }

        private static void ProcessViewStatementBody(TSqlStatement statement)
        {
            if (statement is SelectStatement)
            {
                SelectStatement select = (SelectStatement)statement;
                QuerySpecification querySpecification = (QuerySpecification)select.QueryExpression;
                FromClause fromClause = querySpecification.FromClause;
                IList<TableReference> tableReference = fromClause.TableReferences;
                foreach (TableReference tbl in tableReference)
                {
                    Console.WriteLine(tbl.ScriptTokenStream.Count);
                    Console.WriteLine(tbl.ToString());
                }
                IList<TSqlParserToken> sqlFragment = select.ScriptTokenStream;
                foreach (TSqlParserToken t in sqlFragment)
                {
                    Console.WriteLine(t.Text);
                }
            }
            else if (statement is CreateTableStatement)
            {
                CreateTableStatement createTableStatement = (CreateTableStatement)statement;
                TableDefinition tblDefinition = createTableStatement.Definition;
                IList<ColumnDefinition> columnDefinition = tblDefinition.ColumnDefinitions;
                foreach (ColumnDefinition cd in columnDefinition)
                {
                    if (cd.DataType.Name.ToString().ToLower().Equals("nvarchar(max)"))
                    {
                        Console.WriteLine("Consider using a number rather than max");
                    }
                }
            }
            else
                statement.ToString();
        }
    }
}