using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;

namespace Kickstart.SqlServer.SnakeCase.App
{
    public interface ISnakeCaseService
    {
        void SnakeCaseFile(string filePath);


    }

    class SnakeCaseService : ISnakeCaseService
    {
        public void SnakeCaseFile(string filePath)
        {
            
            Encoding encoding;
            string f1 = null;

            //try to use same encoding as source file
            using (var reader = new StreamReader(filePath))
            {
                f1 = reader.ReadToEnd().ToLower();
                encoding = reader.CurrentEncoding;
            }

            string sqlFileText = File.ReadAllText(filePath);
            var sqlFileTextLines = File.ReadAllLines(filePath);
            //File.WriteAllText(file.FullName, text);

            var parser = new TSql150Parser(false);
            var options = new SqlScriptGeneratorOptions();
            
            var scriptGen = new Sql150ScriptGenerator(options)
            {
                
            };
            IList<ParseError> errors;
            var script2 =
                parser.Parse(new StringReader(sqlFileText), out errors) as TSqlScript;
            if (errors.Count > 0)
            {
                RaiseSqlErrorsException(errors, sqlFileText, filePath);
                return;
            }

            //var sb = new StringBuilder(sqlFileText);

            
            foreach (var batch in script2.Batches.Reverse())
            foreach (var statement in batch.Statements.Reverse())
            {
                var snakeCaseVisitor = new SqlServerSnakeCaseVisitor2();
                statement.Accept(snakeCaseVisitor);

                var sortedReplacements = snakeCaseVisitor.Replacements.OrderByDescending(r => r.Offset);
                foreach (var r in sortedReplacements)
                {
                        if (filePath.EndsWith("Schedule_ProgramPattern.sql") && r.LineNumber ==1)
                        {
                            int x = 1;
                        }
                        var sb2 = new StringBuilder(sqlFileTextLines[r.LineNumber-1]);
                        sb2 = sb2.Replace(r.OldValue, r.NewValue, r.Column-1, r.OldValue.Length);
                        sqlFileTextLines[r.LineNumber-1] = sb2.ToString();

                }
            }

            try
            {
                File.WriteAllLines(filePath, sqlFileTextLines, encoding);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e);
            }
        }

        private void RaiseSqlErrorsException(IList<ParseError> sqlParseErrors, string sql, string file)
        {
            foreach(var error in sqlParseErrors)
            {
                Console.WriteLine($"Error snake casing: {file} : {error.Message}");
            }
        }
    }
}