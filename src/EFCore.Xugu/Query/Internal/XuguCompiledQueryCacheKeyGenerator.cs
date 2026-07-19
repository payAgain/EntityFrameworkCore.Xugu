using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
{
    public XuguCompiledQueryCacheKeyGenerator(
        CompiledQueryCacheKeyGeneratorDependencies dependencies,
        RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    public override object GenerateCacheKey(Expression query, bool async)
    {
        var extension = RelationalDependencies.ContextOptions.FindExtension<XuguOptionsExtension>();
        return new XuguCompiledQueryCacheKey(
            GenerateCacheKeyCore(query, async),
            extension?.ServerVersion,
            extension?.CompatibleModeOnOpen ?? XuguCompatibleMode.None);
    }

    private readonly struct XuguCompiledQueryCacheKey(
        RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey,
        Infrastructure.ServerVersion? serverVersion,
        XuguCompatibleMode compatibleModeOnOpen)
    {
        private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
        private readonly Infrastructure.ServerVersion? _serverVersion = serverVersion;
        private readonly XuguCompatibleMode _compatibleModeOnOpen = compatibleModeOnOpen;

        public override bool Equals(object? obj)
            => obj is XuguCompiledQueryCacheKey key && Equals(key);

        private bool Equals(XuguCompiledQueryCacheKey other)
            => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
               && Equals(_serverVersion, other._serverVersion)
               && _compatibleModeOnOpen == other._compatibleModeOnOpen;

        public override int GetHashCode()
            => HashCode.Combine(_relationalCompiledQueryCacheKey, _serverVersion, _compatibleModeOnOpen);
    }
}
