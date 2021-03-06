using System;
using System.Globalization;
using System.Linq;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;
using Newtonsoft.Json;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.Proto
{
    public class CProtoRpc : CPart
    {
        #region Constructors

        public CProtoRpc(CProtoService protoService)
        {
            ProtoService = protoService;
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
        public CProtoService ProtoService { get; set; }

        public string RpcName { get; set; }
        public string RpcDescription { get; set; }
        public CProtoMessage Request { get; set; }
        public CProtoMessage Response { get; set; }

        public COperationIs OperationIs
        {
            get
            {
                if (RpcName.StartsWith("Get"))
                    return COperationIs.Get;
                else if (RpcName.StartsWith("Is"))
                    return COperationIs.Get;
                else if (RpcName.StartsWith("List"))
                    return COperationIs.List;
                else if (RpcName.StartsWith("Find"))
                    return COperationIs.Find;
                else if (RpcName.StartsWith("Check"))
                    return COperationIs.Check;
                else if (RpcName.StartsWith("Read"))
                    return COperationIs.Read;
                else if (RpcName.StartsWith("Create"))
                    return COperationIs.Create;
                else if (RpcName.StartsWith("Update"))
                    return COperationIs.Update;
                else if (RpcName.StartsWith("Queue"))
                    return COperationIs.Queue;
                else if (RpcName.StartsWith("Dequeue"))
                    return COperationIs.Dequeue;
                else if (RpcName.StartsWith("Delete"))
                    return COperationIs.Delete;
                else if (RpcName.StartsWith("Add"))
                    return COperationIs.Add;
                else if (RpcName.StartsWith("Save"))
                    return COperationIs.Save;
                else if (RpcName.StartsWith("Approve"))
                    return COperationIs.Approve;

                return COperationIs.Undefined;
            }
        }

        public string DomainModelNameForInput
        {
            get
            {
                var request = this.GetInnerMessageOrRequest();

                return request.MessageName;
                //return InferDomainModelName(request.)
            }
        }

        public string DomainModelNameForOutput
        {
            get
            {
                var innerMessage = this.GetInnerMessage();
                if (innerMessage != null)
                    return innerMessage.MessageName;
                //if (this.Response.ProtoField.Count == 1)
                //   return SqlMapper.GrpcTypeToClrType(this.Response.ProtoField[0].FieldType).ToClrTypeName();

                return InferDomainModelName(Response.MessageName);
                //return Response.MessageName;
            }
        }

        public string GetReturnType()
        {
            var rpc = this;
            var response = rpc.GetInnerMessageOrResponse();


            var returnType = rpc.Response.HasFields ? $"Model.{rpc.DomainModelNameForOutput}" : "bool";
            var methodReturnType = string.Empty;
            if (rpc.ResponseIsList())
            {
                methodReturnType = $"IEnumerable<Model.{rpc.DomainModelNameForOutput}>";
            }
            else
            {
                methodReturnType = $"{returnType}";
            }
            return methodReturnType;
        }


        public object OutputTypeName { get; private set; }

        internal bool ResponseIsList()
        {
            return Response.ProtoField.Count == 1 && Response.ProtoField.Exists(pf => pf.Repeated);
            
        }

        internal bool RequestIsList()
        {

            return Request.ProtoField.Count == 1 && Request.ProtoField.Exists(pf => pf.Repeated);
        }


        internal CProtoMessage GetInnerMessageOrRequest()
        {
            var request = this.Request;

            if (request.ProtoField.Count == 1 && request.ProtoField[0].FieldType == GrpcType.__message)
            {
                request = this.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm => pm.MessageName == request.ProtoField[0].MessageType);

            }
            return request;
        }

        internal CProtoMessage GetInnerMessageOrResponse()
        {
            var response = this.Response;

            if (response.ProtoField.Count == 1 && response.ProtoField[0].FieldType == GrpcType.__message)
            {
                response = this.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm => pm.MessageName == response.ProtoField[0].MessageType);

            }
            return response;
        }
        internal CProtoMessage GetInnerMessage()
        {
            var response = this.Response;

            if (response.ProtoField.Count == 1 && response.ProtoField[0].FieldType == GrpcType.__message)
            {
                return this.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm => pm.MessageName == response.ProtoField[0].MessageType);

            }
            return null;
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
            
            var s = new Inflector.Inflector( CultureInfo.CurrentCulture );
            name = s.Singularize(name);
            return $"{name}";
        }

        
        #endregion Properties
    }
}