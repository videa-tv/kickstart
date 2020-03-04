using System;

namespace Kickstart.Pass2.CModel.Code
{
    [Flags]
    public enum COperationIs
    {
        Undefined = 0,
        Add = 1,
        Update = 2,
        Delete = 4,
        Get = 8,
        Find = 16,
        Check = 32,
        Set = 64,
        Bulk = 128,
        CRUD = 256,
        Read =512,
        Create =1024,
        Dequeue=2048,
        Queue = 4096,
        List = 8192,
        Save = 16384,
        Approve = 32768

    }
}