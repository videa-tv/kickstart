using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;

namespace Kickstart.Pass1.Service
{
    public class KProtoFileService
    {
        private readonly IProtoToKProtoConverter _converter;
        public KProtoFileService(IProtoToKProtoConverter converter)
        {
            _converter = converter;
        }

       
    }
}
