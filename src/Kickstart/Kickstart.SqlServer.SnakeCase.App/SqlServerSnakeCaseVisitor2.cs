using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.SqlServer.SnakeCase.App
{
    public class Replacement
    {
        public int LineNumber { get; set; }
        public int Offset { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int Column { get; internal set; }
    }
    partial class SqlServerSnakeCaseVisitor2 : TSqlFragmentVisitor
    {
        IList<Replacement> _replacements = new List<Replacement>();
        public IList<Replacement> Replacements
        {
            get
            {
                return _replacements;
            }
        }
        public SqlServerSnakeCaseVisitor2()
        {

        }
        public override void Visit(VariableReference node)
        {   
            var oldValue = node.Name;
            var newValue = node.Name.ToSnakeCase();

            if (string.Compare(oldValue, newValue, false) != 0)
            {
                _replacements.Add(new Replacement()
                {
                    LineNumber = node.StartLine,
                    Column = node.StartColumn,
                    Offset = node.StartOffset,
                    OldValue = oldValue,
                    NewValue = newValue

                });
            }
            base.Visit(node);

        }
        public override void Visit(Identifier node)
        {
            var oldValue = node.Value;
            var newValue = node.Value.ToSnakeCase();

            if (string.Compare(oldValue, newValue, false) != 0)
            {
                _replacements.Add(new Replacement()
                {
                    LineNumber = node.StartLine,
                    Column = node.QuoteType == QuoteType.NotQuoted ?  node.StartColumn : node.StartColumn+1,
                    Offset = node.QuoteType == QuoteType.NotQuoted ? node.StartOffset : node.StartOffset + 1,
                    OldValue = oldValue,
                    NewValue = newValue

                });
            }            
            base.Visit(node);
        }

        public override void Visit(NamedTableReference node)
        {
            if (node.Alias != null)
            {
                var oldValue = node.Alias.Value;
                var newValue = node.Alias.Value.ToSnakeCase();

                if (string.Compare(oldValue, newValue, false) != 0)
                {

                    _replacements.Add(new Replacement()
                    {
                        LineNumber =
                        node.StartLine,
                        Column = node.Alias.QuoteType == QuoteType.NotQuoted ? node.StartColumn : node.StartColumn + 1,
                        Offset = node.Alias.QuoteType == QuoteType.NotQuoted ? node.StartOffset : node.StartOffset + 1,
                        OldValue = oldValue,
                        NewValue = newValue

                    });
                }
            }

            {
                var oldValue = node.SchemaObject.BaseIdentifier.Value;
                var newValue = node.SchemaObject.BaseIdentifier.Value.ToSnakeCase();

                if (string.Compare(oldValue, newValue, false) != 0)
                {

                    _replacements.Add(new Replacement()
                    {
                        LineNumber = node.StartLine,
                        Column = node.SchemaObject.BaseIdentifier.QuoteType == QuoteType.NotQuoted ? node.StartColumn : node.StartColumn + 1,
                        Offset = node.SchemaObject.BaseIdentifier.QuoteType == QuoteType.NotQuoted ? node.StartOffset : node.StartOffset + 1,
                        OldValue = oldValue,
                        NewValue = newValue

                    });
                }
            }
            base.Visit(node);
        }

        public override void Visit(TableReferenceWithAlias node)
        {
            if (node.Alias != null)
            {   
                var oldValue = node.Alias.Value;
                var newValue = node.Alias.Value.ToSnakeCase();

                if (string.Compare(oldValue, newValue, false) != 0)
                {

                    _replacements.Add(new Replacement()
                    {
                        LineNumber = node.StartLine,
                        Column = node.Alias.QuoteType == QuoteType.NotQuoted ? node.StartColumn : node.StartColumn + 1,
                        Offset = node.Alias.QuoteType == QuoteType.NotQuoted ? node.StartOffset : node.StartOffset + 1,
                        OldValue = oldValue,
                        NewValue = newValue

                    });
                }

            }
            base.Visit(node);
        }

        public override void Visit(TableReferenceWithAliasAndColumns node)
        {
            if (node.Alias != null)
            {
                var oldValue = node.Alias.Value;
                var newValue = node.Alias.Value.ToSnakeCase();

                if (string.Compare(oldValue, newValue, false) != 0)
                {

                    _replacements.Add(new Replacement()
                    {
                        LineNumber = node.StartLine,
                        Column = node.Alias.QuoteType == QuoteType.NotQuoted ? node.StartColumn : node.StartColumn + 1,
                        Offset = node.Alias.QuoteType == QuoteType.NotQuoted ? node.StartOffset : node.StartOffset + 1,
                        OldValue = oldValue,
                        NewValue = newValue

                    });
                }

            }
            base.Visit(node);
        }
    }

}
