using Newtonsoft.Json;

namespace Kickstart.Pass2.CModel.AWS.DMS
{
    public enum RuleAction
    {
        include,
        exclude,
        rename,
        [JsonProperty("remove-column")]
        remove_column,
        convert_lowercase,
        convert_uppercase,
        add_prefix,
        remove_prefix,
        replace_prefix,
        add_suffix,
        remove_suffix,
        replace_suffix
    }
}
