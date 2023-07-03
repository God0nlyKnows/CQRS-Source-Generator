using System;
using System.Collections.Generic;
using System.Text;

namespace TestConsoleApp.Domain.Common.Models
{
    public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
    {
        protected AggregateRoot(TId id) : base(id)
        {
        }

        protected AggregateRoot()
            : base()
        {
        }
    }
}
