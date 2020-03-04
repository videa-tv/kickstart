using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;

namespace Kickstart.Pass2.GrpcServiceProject
{
    class GrpcEnumToEnumConverter
    {
        public CEnum Convert(CProtoEnum protoEnum)
        {
            var e = new CEnum()
            {
                EnumName =  $"{protoEnum.EnumName}"
            };

            foreach (var v in protoEnum.EnumValue)
            {
                e.EnumValues.Add(new CEnumValue() { EnumValueName = v.EnumValueName, EnumValue =  v.EnumValueNumber});
            }

            return e;
        }
    }
}
