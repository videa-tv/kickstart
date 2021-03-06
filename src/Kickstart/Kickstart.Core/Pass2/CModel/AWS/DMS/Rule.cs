using System.Collections.Generic;
using System.Data;
using Kickstart.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kickstart.Pass2.CModel.AWS.DMS
{
    public class CRules : CPart
    {
        [JsonProperty("rules")]
        public List<Rule> Rule { get; set; } = new List<Rule>();

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
        public string ToJson()
        {
            //todo: should this be a "convert" service? 
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }
    public class Rule 
    {
        [JsonProperty("rule-type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleType RuleType { get; set; }

        [JsonProperty("rule-id")]
        public int RuleId { get; set; }

        [JsonProperty("rule-name")]
        public string RuleName { get; set; }

        [JsonProperty("object-locator")]
        public ObjectLocator ObjectLocator { get; set; }

        [JsonProperty("rule-action")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleAction RuleAction { get; set; }

        [JsonProperty("rule-target", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleTarget RuleTarget { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]

        public string Value { get; set; }

    }
   

    public partial class ObjectLocator
    {
        [JsonProperty("schema-name")]
        public string SchemaName { get; set; }

        [JsonProperty("table-name", NullValueHandling = NullValueHandling.Ignore)]
        public string TableName { get; set; }

        [JsonProperty("column-name", NullValueHandling = NullValueHandling.Ignore)]
        public string ColumnName { get; set; }
    }
}