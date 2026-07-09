using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguNonSharedUpdates")]
public sealed class XuguNonSharedUpdatesCollection : ICollectionFixture<NonSharedModelUpdatesFixture>;
