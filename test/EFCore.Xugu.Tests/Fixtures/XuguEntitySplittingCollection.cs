using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguEntitySplitting")]
public sealed class XuguEntitySplittingCollection : ICollectionFixture<EntitySplittingFixture>;
