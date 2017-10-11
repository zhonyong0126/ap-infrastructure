using System;
using System.Collections.Generic;
using System.Text;

namespace Ap.Infrastructure
{
    public interface IHasId
    {
        long Id { get; set; }
    }
}
