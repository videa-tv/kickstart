using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kickstart.Interface;
using Kickstart.Utility;
using Newtonsoft.Json;

namespace Kickstart.Pass2.CModel.Proto
{
    public class CProtoMessageField : CPart
    {
        #region Fields

        public string _fieldName;

        #endregion Fields

        #region Constructors

        public CProtoMessageField(CPart derivedFrom)
        {
            DerivedFrom = derivedFrom;
        }

        #endregion Constructors

        #region Methods

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{FieldType} {FieldName}";
        }
        #endregion Methods

        #region Properties

        [JsonIgnore]
        public string FieldName
        {
            get => FieldNameGrpc.CapitalizeAfter(new[] {'_'}).Replace("_", "");
            set => _fieldName = value;
        }

        [JsonIgnore]
        public string FieldNameGrpc
        {
            get
            {
                if (_fieldName.Contains("_"))
                    return _fieldName;
                if (_fieldName.ToLower() == _fieldName)
                    return _fieldName;
                return SnakeCaseTranslator.SnakeCase(_fieldName);
            }
        }

        public string MessageType { get; set; }
        public string EnumType { get; set; }
        public GrpcType FieldType { get; set; }

        public bool Required { get; set; }

        public bool Singular { get; set; }

        public bool Repeated { get; set; }

        public bool Optional { get; set; }

        public string Comment { get; set; }

        public bool IsScalar { get; set; } = true;
        public string MapType { get; internal set; }
        public bool IsMap { get { return this.FieldType == GrpcType.__map; } }
        public bool IsTimestampMessage { get { return this.MessageType == "Timestamp"; } }

        public bool IsEnum { get { return this.FieldType == GrpcType.__enum; } }

        #endregion Properties
    }

    internal static class Ext
    {
        public static string CapitalizeAfter(this string s, IEnumerable<char> chars)
        {
            if (s.Length == 0)
                return string.Empty;

            var charsHash = new HashSet<char>(chars);
            var sb = new StringBuilder(s);
            sb[0] = char.ToUpper(sb[0]);

            for (var i = 1; i < sb.Length - 2; i++)
                if (charsHash.Contains(sb[i]))
                    sb[i + 1] = char.ToUpper(sb[i + 1]);
            return sb.ToString();
        }
    }
}