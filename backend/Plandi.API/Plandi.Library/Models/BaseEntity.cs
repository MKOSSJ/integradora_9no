using System;
using System.Collections.Generic;
using System.Text;

namespace Plandi.Library.Models
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }

        public Guid PublicId { get; set; } = Guid.NewGuid();

        public bool Activo { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
