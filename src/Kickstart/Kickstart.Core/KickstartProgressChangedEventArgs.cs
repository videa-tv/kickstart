using System;

namespace Kickstart
{
    public class KickstartProgressChangedEventArgs : EventArgs
    {
        public int ProgressPercentChange { get; set; }
        public string ProgressMessage { get; set; }
    }
}
