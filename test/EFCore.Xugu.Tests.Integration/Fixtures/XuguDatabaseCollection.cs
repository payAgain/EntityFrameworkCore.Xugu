using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguDatabase")]
public sealed class XuguDatabaseCollection : ICollectionFixture<XuguDatabaseFixture>;
