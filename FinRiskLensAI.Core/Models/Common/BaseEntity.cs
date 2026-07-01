using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public bool IsDeleted { get; protected set; }

        public void SoftDelete() => IsDeleted = true;
    }
}
