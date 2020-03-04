using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.SqlServer.SnakeCase.App
{
    class Program
    {
        static void Main(string[] args)
        {   
            var searchDirectory = @"C:\ps2\";
            var sqlFileFinder = new SqlFileFinder(new SnakeCaseService());
            var includedExtensions = new string[] {".sql", ".esql"};
            sqlFileFinder.FindAndSnakeCaseSqlFiles(searchDirectory, true, null, includedExtensions);
        }
    }
}
