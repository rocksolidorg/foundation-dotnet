using Microsoft.EntityFrameworkCore;

namespace RockSolid.Foundation.Modeling.AspNetCore;

public interface IOutboxContext
{
    protected DbSet<OutboxMessage> OutboxMessages { get; }
}
