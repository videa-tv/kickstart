using System;

namespace Kickstart.Pass2.CModel.Proto
{
    [Flags]
    public enum CProtoRpcRefDataDirection
    {
        Undefined = 0,
        Pull = 1,
        Push = 2,
        Trigger = 4,
        Notification = 8
    }
}