using System.Collections.Generic;
using Kickstart.Interface;
using Newtonsoft.Json;

namespace Kickstart.Pass2.CModel.Proto
{
    public class CProtoService : CPart
    {
        #region Constructors

        public CProtoService(CProtoFile protoFile)
        {
            ProtoFile = protoFile;
        }

        #endregion Constructors

        #region Methods

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        [JsonIgnore]
        public CProtoFile ProtoFile { get; set; }

        public string ServiceName { get; set; }

        public List<CProtoRpc> Rpc { get; set; } = new List<CProtoRpc>();

        #endregion Properties
    }
}