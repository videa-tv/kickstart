
using System;

namespace Kickstart.Build.Data.Providers
{
    public interface ITfsProvider
    {
        IDisposable GetConnection();
    }

}