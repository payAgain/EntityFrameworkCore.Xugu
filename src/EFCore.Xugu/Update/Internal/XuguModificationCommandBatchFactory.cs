using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Update.Internal;

public class XuguModificationCommandBatchFactory : IModificationCommandBatchFactory
{
    private const int DefaultMaxBatchSize = 42;
    private const int MaxMaxBatchSize = 1000;
    private readonly int _maxBatchSize;

    public XuguModificationCommandBatchFactory(
        ModificationCommandBatchFactoryDependencies dependencies,
        IDbContextOptions options)
    {
        Dependencies = dependencies;

        _maxBatchSize = Math.Min(
            options.Extensions.OfType<XuguOptionsExtension>().FirstOrDefault()?.MaxBatchSize ?? DefaultMaxBatchSize,
            MaxMaxBatchSize);

        if (_maxBatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RelationalOptionsExtension.MaxBatchSize),
                RelationalStrings.InvalidMaxBatchSize(_maxBatchSize));
        }
    }

    protected virtual ModificationCommandBatchFactoryDependencies Dependencies { get; }

    public virtual ModificationCommandBatch Create()
        => new XuguModificationCommandBatch(Dependencies, _maxBatchSize);
}
