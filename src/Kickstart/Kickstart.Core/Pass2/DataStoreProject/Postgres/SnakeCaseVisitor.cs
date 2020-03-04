using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.DataStoreProject.Postgres
{
    partial class SnakeCaseVisitor : TSqlFragmentVisitor
    {
        public override void Visit(VariableReference node)
        {
            node.Name = node.Name.ToSnakeCase();
            base.Visit(node);
        }
        public override void Visit(Identifier node)
        {
            //Postgresql doesn't like [columnname]

            if (node.QuoteType == QuoteType.SquareBracket)
                node.QuoteType = QuoteType.NotQuoted;

            node.Value = node.Value.ToSnakeCase();
            
            base.Visit(node);
        }

        public override void Visit(NamedTableReference node)
        {
            //Postgresql doesn't like [columnname]
            if (node.Alias != null)
            {
                if (node.Alias.QuoteType == QuoteType.SquareBracket)
                    node.Alias.QuoteType = QuoteType.NotQuoted;

                node.Alias.Value = node.Alias.Value.ToSnakeCase();
            }

            if (node.SchemaObject.BaseIdentifier.QuoteType == QuoteType.SquareBracket)
                node.SchemaObject.BaseIdentifier.QuoteType = QuoteType.NotQuoted;

            node.SchemaObject.BaseIdentifier.Value = node.SchemaObject.BaseIdentifier.Value.ToSnakeCase();

            base.Visit(node);
        }

        public override void Visit(TableReferenceWithAlias node)
        {
            if (node.Alias != null)
            {
                //Postgresql doesn't like [columnname]
                if (node.Alias.QuoteType == QuoteType.SquareBracket)
                    node.Alias.QuoteType = QuoteType.NotQuoted;


                node.Alias.Value = node.Alias.Value.ToSnakeCase();
            }
            base.Visit(node);
        }

        public override void Visit(TableReferenceWithAliasAndColumns node)
        {
            //Postgresql doesn't like [columnname]
            if (node.Alias != null)
            {
                if (node.Alias.QuoteType == QuoteType.SquareBracket)
                    node.Alias.QuoteType = QuoteType.NotQuoted;

                node.Alias.Value = node.Alias.Value.ToSnakeCase();
            }
            base.Visit(node);
        }



    }
}
