using System;

namespace Dinah.Core
{
    public interface ISystemDateTime
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
