using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using PoorMansTSqlFormatterLib;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.Formatters;

namespace DTTools.Classes
{
    public enum ConstraintType
    {
        IX, UQ, PK, CK, FK, DF
    }

    public class Utilities
    {
        public static string GetTableConstraintName(ConstraintType type, string tableName, 
                                    IList<ColumnWithSortOrder> columns, bool include = false )
        {
            string col = "";
            foreach (ColumnWithSortOrder cols in columns)
            {
                MultiPartIdentifier multiIden = (MultiPartIdentifier)cols.Column.MultiPartIdentifier;
                if (multiIden.Count == 1)
                {
                    col += multiIden.Identifiers[0].Value + "_";
                }
                else
                {
                    foreach (Identifier iden in multiIden.Identifiers)
                    {

                    }
                }
            }
            if (include)
                col += "incl";
            return type + "_" + tableName + "_" + col;
        }

        public static string GetColumnConstraintName(ConstraintType type, string tableName,
                                    ColumnDefinition column, bool include = false)
        {
            return type + "_" + tableName + "_" + column.ColumnIdentifier.Value;
        }

        public static string DoFormatting(string content)
        {
            ISqlTokenizer _tokenizer;
            ISqlTokenParser _parser;
            ISqlTreeFormatter _formatter;

            _tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
            _parser = new PoorMansTSqlFormatterLib.Parsers.TSqlStandardParser();

            ISqlTreeFormatter innerFormatter = new TSqlStandardFormatter//new .TSqlStandardFormatterOptions
            {
                IndentString = "\t",
                SpacesPerTab = 4,
                MaxLineWidth = 999,
                ExpandCommaLists = true,
                TrailingCommas = true,
                SpaceAfterExpandedComma = false,
                ExpandBooleanExpressions = true,
                ExpandCaseStatements = true,
                ExpandBetweenConditions = true,
                //ExpandInLists = true,
                BreakJoinOnSections = false,
                UppercaseKeywords = true,
                HTMLColoring = true
                //HTMLFormatted = true
                //KeywordStandardization = true,
                //NewStatementLineBreaks = 2,
                //NewClauseLineBreaks = 2
            };
        //);

            _formatter = new PoorMansTSqlFormatterLib.Formatters.HtmlPageWrapper(innerFormatter);

            var tokenizedSql = _tokenizer.TokenizeSQL(content);
            var parsedSql = _parser.ParseSQL(tokenizedSql);

            //Remove starting and ending HTML component for modal view.
            var lines = _formatter.FormatSQLTree(parsedSql).ToString();
            lines = lines.Substring(lines.IndexOf('\n')+1);
            lines = lines.Substring(lines.IndexOf('\n')+1);
            lines = lines.Substring(lines.IndexOf('\n')+1);
            lines = lines.Substring(lines.IndexOf('\n')+1);
            lines = lines.Substring(lines.IndexOf('\n')+1);

            lines = lines.Substring(0, lines.Length - 18);
            return lines;
        }
    }
}
