﻿using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.DataStoreProject.Postgres
{
    partial class SqlServerSnakeCaseVisitor : TSqlFragmentVisitor
    {
        public override void Visit(VariableReference node)
        {
            node.Name = node.Name.ToSnakeCase();
            base.Visit(node);
        }
        public override void Visit(Identifier node)
        {
           
            node.Value = node.Value.ToSnakeCase();
            
            base.Visit(node);
        }

        public override void Visit(NamedTableReference node)
        {
            if (node.Alias != null)
            {
           
                node.Alias.Value = node.Alias.Value.ToSnakeCase();
            }

           
            node.SchemaObject.BaseIdentifier.Value = node.SchemaObject.BaseIdentifier.Value.ToSnakeCase();

            base.Visit(node);
        }

        public override void Visit(TableReferenceWithAlias node)
        {
            if (node.Alias != null)
            {
           
                node.Alias.Value = node.Alias.Value.ToSnakeCase();
            }
            base.Visit(node);
        }

        public override void Visit(TableReferenceWithAliasAndColumns node)
        {
            if (node.Alias != null)
            {
                node.Alias.Value = node.Alias.Value.ToSnakeCase();
            }
            base.Visit(node);
        }



    }
}
