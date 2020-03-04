using Kickstart.Pass1.KModel;
using System;
using System.Threading.Tasks;

namespace Kickstart.Wizard.View
{
    public interface IDatabaseSqlView : IView
    {
        bool GenerateStoredProcAsEmbeddedQuery { get; set; }
        string MockSqlViewText { get; set; }
        string SqlStoredProcText { get; set; } 
        string SqlTableText { get; set; }
        string SqlTableTypeText { get; set; }

        string SqlViewText { get; set; }


        bool ConvertToSnakeCase { get; set; }
        Func<Object, EventArgs, Task> SqlTableTextChanged { get; set; }
        Func<Object, EventArgs, Task> SqlTableTypeTextChanged { get; set; }

        Func<Object, EventArgs, Task> SqlStoredProcTextChanged { get; set; }

        Func<Object, EventArgs, Task> GenerateStoredProcAsEmbeddedQueryChanged { get; set; }

        Func<Object, EventArgs, Task> ConvertToSnakeCaseChanged { get; set; }

    }
}