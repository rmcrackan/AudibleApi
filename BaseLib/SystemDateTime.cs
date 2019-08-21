using System;
using System.Collections.Generic;
using System.Text;

namespace BaseLib
{
    // use this in production. use a mock ISystemDateTime for testing
    public class SystemDateTime : ISystemDateTime
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
