using System;

namespace BaseLib
{
    public interface ISystemDateTime
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
