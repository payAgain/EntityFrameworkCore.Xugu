using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguOptimisticConcurrency")]
public sealed class XuguOptimisticConcurrencyCollection : ICollectionFixture<OptimisticConcurrencyFixture>;
