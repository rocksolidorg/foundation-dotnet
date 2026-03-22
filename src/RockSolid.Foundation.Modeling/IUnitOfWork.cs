namespace RockSolid.Foundation.Modeling;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}