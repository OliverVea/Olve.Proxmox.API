using Olve.Proxmox.Models;

namespace Olve.Proxmox.Storage;

public interface IEntityRepository<TEntity, in TEntityId>
{
    Task<Result> SetAsync(TEntityId id, TEntity entity, CancellationToken ct = default);
    Task<Result<TEntity>> GetAsync(TEntityId id, CancellationToken ct = default);
    Task<DeletionResult> DeleteAsync(TEntityId id, CancellationToken ct = default);
    Task<bool> ExistsAsync(TEntityId id, CancellationToken ct = default);
}

public abstract class SynchronousEntityRepository<TEntity, TEntityId> : IEntityRepository<TEntity, TEntityId>
{
    protected abstract Result Set(TEntityId id, TEntity entity);
    protected abstract Result<TEntity> Get(TEntityId id);
    protected abstract DeletionResult Delete(TEntityId id);
    protected abstract bool Exists(TEntityId id);

    public Task<Result> SetAsync(TEntityId id, TEntity entity, CancellationToken ct = default)
        => Task.FromResult(Set(id, entity));

    public Task<Result<TEntity>> GetAsync(TEntityId id, CancellationToken ct = default)
        => Task.FromResult(Get(id));

    public Task<DeletionResult> DeleteAsync(TEntityId id, CancellationToken ct = default)
        => Task.FromResult(Delete(id));

    public Task<bool> ExistsAsync(TEntityId id, CancellationToken ct = default)
        => Task.FromResult(Exists(id));
}

public interface IGroupingRepository : IEntityRepository<DeviceGrouping, DeviceGroupingId>;

public class InMemoryGroupingRepository : SynchronousEntityRepository<DeviceGrouping, DeviceGroupingId>, IGroupingRepository
{
    private readonly Dictionary<DeviceGroupingId, DeviceGrouping> _groupings = new();

    protected override Result Set(DeviceGroupingId id, DeviceGrouping entity)
    {
        _groupings[id] = entity;
        
        return Result.Success();
    }

    protected override DeletionResult Delete(DeviceGroupingId id)
    {
        var found = _groupings.Remove(id);

        return found ? DeletionResult.Success() : DeletionResult.NotFound();
    }

    protected override bool Exists(DeviceGroupingId id)
    {
        return _groupings.ContainsKey(id);
    }

    protected override Result<DeviceGrouping> Get(DeviceGroupingId id)
    {
        if (_groupings.TryGetValue(id, out var deviceGrouping))
        {
            return deviceGrouping;
        }

        return new ResultProblem("Failed to get device grouping with id {0}", id.Value);
    }
}

public readonly record struct DeviceGroupingId(string Value);
