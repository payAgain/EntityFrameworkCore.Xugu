using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguTableSplitting")]
public sealed class XuguTableSplittingCollection : ICollectionFixture<TableSplittingFixture>;
