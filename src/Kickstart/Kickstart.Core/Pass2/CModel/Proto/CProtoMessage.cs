using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Proto
{
    public class CProtoMessage : CPart
    {
        #region Methods

        public override string ToString()
        {
            return $"{base.ToString()}-->{this.MessageName}";
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties
        public bool IsExternal { get; set; }

        public string MessageName
        {
            get;
            set;
        }
        public bool IsRequest { get; set; }
        public bool IsResponse { get; set; }
        public bool HasFields => ProtoField.Count > 0;

        public List<CProtoMessageField> ProtoField { get; set; } = new List<CProtoMessageField>();
        public string AsColumnList
        {
            get
            {
                var columns = string.Empty;
                return string.Join(",", ProtoField.Select(f=>f.FieldName));
            }
        }

        #endregion Properties

        public CProtoRpc Rpc { get; set; }

        public CProtoMessage(CProtoRpc protoRpc)
        {
            Rpc = protoRpc;
        }

        public string DomainModelNameForOutput
        {
            get
            {
                return InferDomainModelName(MessageName);
            }
        }

        private string InferDomainModelName(string name)
        {
            if (name.Contains("By"))
            {
                name = name.Substring(0, name.IndexOf("By"));
            }
            if (name.StartsWith("Get"))
            {
                name = name.Substring(3, name.Length - 3);
            }
            if (name.StartsWith("Is"))
            {
                name = name.Substring(2, name.Length - 2);
            }

            if (name.StartsWith("Read"))
            {
                name = name.Substring(4, name.Length - 4);
            }
            if (name.StartsWith("List"))
            {
                name = name.Substring(4, name.Length - 4);
            }

            if (name.StartsWith("Find"))
            {
                name = name.Substring(4, name.Length - 4);
            }

            if (name.StartsWith("Check"))
            {
                name = name.Substring(5, name.Length - 5);
            }

            if (name.StartsWith("Create"))
            {
                name = name.Substring(6, name.Length - 6);
            }
            if (name.StartsWith("Update"))
            {
                //name = name.Substring(6, name.Length - 6);
            }

            if (name.StartsWith("Queue"))
            {
                name = name.Substring(5, name.Length - 5);
            }
            if (name.StartsWith("Dequeue"))
            {
                name = name.Substring(7, name.Length - 7);
            }

            if (name.StartsWith("All"))
            {
                name = name.Substring(3, name.Length - 3);
            }

            if (name.StartsWith("Update"))
            {
                if (name.EndsWith("Response"))
                {
                    name = name.Replace("Response", "Model");
                }
            }
            else
            {
                if (name.EndsWith("Response"))
                {
                    name = name.Substring(0, name.LastIndexOf("Response"));

                }
            }

            var s = new Inflector.Inflector(CultureInfo.CurrentCulture);
            name = s.Singularize(name);
            return $"{name}";

        }


    }
}