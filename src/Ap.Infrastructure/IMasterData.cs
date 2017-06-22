using System;
using System.Collections.Generic;
using System.Text;

namespace Ap.Infrastructure
{
    public interface IMasterData
    {
        int Id { get; set; }
        string Name { get; set; }
        bool IsDeleted { get; set; }
        int Sort { get; set; }
    }
}
