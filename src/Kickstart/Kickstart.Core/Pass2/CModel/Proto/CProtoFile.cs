using System.Collections.Generic;
using System.Linq;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;
using Newtonsoft.Json;

namespace Kickstart.Pass2.CModel.Proto
{
    public class CProtoFile : CPart
    {
        #region Fields

        #endregion Fields

        #region Properties
        public string SourceProtoText { get; set; }

        public string ProtoFileName => ProtoService.First().ServiceName;

        public string ProtoSyntax { get; set; } = "proto3";

        public List<string> Package { get; set; } = new List<string>();

        public List<string> Import { get; set; } = new List<string>();

        public List<CProtoService> ProtoService { get; set; } = new List<CProtoService>();

        public List<CProtoMessage> ProtoMessage { get; set; } = new List<CProtoMessage>();

        public List<CProtoEnum> ProtoEnum { get; set; } = new List<CProtoEnum>();

        public string CSharpNamespace { get; set; }
        public List<string> Option { get; set; } = new List<string>();

        // [JsonIgnore]
        // public SProject OwnedByProject { get; set; }

        #endregion Properties

        #region Constructors

        #endregion Constructors

        #region Methods

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string ToJson()
        {
            //todo: should this be a "convert" service? 
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static CProtoFile FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            var protoFile = JsonConvert.DeserializeObject<CProtoFile>(json);
            if (protoFile.Package == null)
                protoFile.Package = new List<string>();
            foreach (var protoService in protoFile.ProtoService)
            {
                protoService.ProtoFile = protoFile;
                foreach (var rpc in protoService.Rpc)
                    rpc.ProtoService = protoService;
            }
            return protoFile;
        }
        public IList<CProtoMessage> GetRepeatedMessagesUsedInAResponse()
        {
            List<CProtoMessage> used = new List<CProtoMessage>();
            foreach (var service in this.ProtoService)
            {
                foreach (var rpc in service.Rpc)
                {
                    var responseMessage = rpc.Response;

                    foreach (var field in responseMessage.ProtoField)
                    {
                        //todo: do a deeper, recursive search
                        if (field.FieldType == GrpcType.__message)
                        {
                            if (!field.Repeated)
                            {
                                continue;
                            }
                            var fieldMessage = this.ProtoMessage.FirstOrDefault(m => m.MessageName == field.MessageType);
                            if (fieldMessage != null)
                            {
                                if (!used.Exists(m => m == fieldMessage))
                                {
                                    used.Add(fieldMessage);
                                }
                            }
                        }
                    }
                }
            }
            return used;
        }
        public IList<CProtoRpc> GetRpcListThatGet()
        {
            List<CProtoRpc> getList = new List<CProtoRpc>();
            foreach (var service in this.ProtoService)
            {
                foreach (var rpc in service.Rpc)
                {
                    if (rpc.OperationIs.HasFlag(COperationIs.Get) || rpc.OperationIs.HasFlag(COperationIs.Find) ||
                                                rpc.OperationIs.HasFlag(COperationIs.List) ||
                        rpc.OperationIs.HasFlag(COperationIs.Check) || rpc.OperationIs.HasFlag(COperationIs.Read) || rpc.OperationIs.HasFlag(COperationIs.Dequeue))
                    {
                        getList.Add(rpc);
                    }
                }
            }

            return getList;
        }

        public IList<CProtoRpc> GetRpcListThatSet()
        {
            List<CProtoRpc> setList = new List<CProtoRpc>();
            foreach (var service in this.ProtoService)
            {
                foreach (var rpc in service.Rpc)
                {
                    if (rpc.OperationIs.HasFlag(COperationIs.Set) || rpc.OperationIs.HasFlag(COperationIs.Update) ||
                        rpc.OperationIs.HasFlag(COperationIs.Save) ||
                        rpc.OperationIs.HasFlag(COperationIs.Approve) ||
                        rpc.OperationIs.HasFlag(COperationIs.Add)  || rpc.OperationIs.HasFlag(COperationIs.Create) ||
                        rpc.OperationIs.HasFlag(COperationIs.Queue))
                    {
                        setList.Add(rpc);
                    }
                }
            }

            return setList;
        }
    }
}
        #endregion Methods