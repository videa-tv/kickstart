using System.Collections.Generic;
using Kickstart.Pass2.CModel.AWS.DMS;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.SqlServer
{
    public class CTableToDmsRuleConverter : ICTableToDmsRuleConverter
    {
        private int ruleId = 1;
        public List<Rule> Convert(CTable table)
        {
            var rules = new List<Rule>();

            if (table.Schema.SchemaName != table.Schema.SchemaNameOriginal)
            {
                var schemaRenameRule = new Rule();
                schemaRenameRule.RuleId = ruleId++;
                schemaRenameRule.RuleName = $"{schemaRenameRule.RuleId} {table.Schema.SchemaNameOriginal} Rename Rule";
                schemaRenameRule.RuleType = RuleType.transformation;
                schemaRenameRule.ObjectLocator = new ObjectLocator { SchemaName = table.Schema.SchemaNameOriginal };
                schemaRenameRule.RuleTarget = RuleTarget.schema;
                schemaRenameRule.RuleAction = RuleAction.rename;
                schemaRenameRule.Value = table.Schema.SchemaName.WrapReservedAndSnakeCase(table.DatabaseType, table.ConvertToSnakeCase);
                rules.Add(schemaRenameRule);
            }

            var tableSelectionRule = new Rule();
            tableSelectionRule.RuleId = ruleId++;
            tableSelectionRule.RuleName = $"{tableSelectionRule.RuleId} {table.TableNameOriginal} Selection Rule";
            tableSelectionRule.RuleType = RuleType.selection;
            tableSelectionRule.ObjectLocator = new ObjectLocator { SchemaName = table.Schema.SchemaNameOriginal, TableName = table.TableNameOriginal };
            tableSelectionRule.RuleTarget = RuleTarget.table;
            tableSelectionRule.RuleAction = RuleAction.include;
            rules.Add(tableSelectionRule);

            if (table.TableNameOriginal != table.TableName)
            {
                var tableRenameRule = new Rule();
                tableRenameRule.RuleId = ruleId++;
                tableRenameRule.RuleName = $"{tableRenameRule.RuleId} {table.TableNameOriginal} Rename Rule";
                tableRenameRule.RuleType = RuleType.transformation;
                tableRenameRule.ObjectLocator = new ObjectLocator { SchemaName = table.Schema.SchemaNameOriginal, TableName = table.TableNameOriginal };
                tableRenameRule.RuleTarget = RuleTarget.table;
                tableRenameRule.RuleAction = RuleAction.rename;
                tableRenameRule.Value = table.TableName;
                rules.Add(tableRenameRule);
            }
            foreach (var col in table.Column)
            {
                if (col.ColumnNameOriginal == col.ColumnName)
                    continue;

                var columnRenameRule = new Rule();
                columnRenameRule.RuleId = ruleId++;
                columnRenameRule.RuleName = $"{columnRenameRule.RuleId} {col.ColumnNameOriginal} Rename Rule";
                columnRenameRule.RuleType = RuleType.transformation;
                columnRenameRule.ObjectLocator = new ObjectLocator { SchemaName = table.Schema.SchemaNameOriginal, TableName = table.TableNameOriginal, ColumnName = col.ColumnNameOriginal };
                columnRenameRule.RuleTarget = RuleTarget.column;
                columnRenameRule.RuleAction = RuleAction.rename;
                columnRenameRule.Value = col.ColumnName;
                rules.Add(columnRenameRule);
            }
            return rules;
        }
        
    }
}