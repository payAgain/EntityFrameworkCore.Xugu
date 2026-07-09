using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguLoad")]
public sealed class XuguLoadCollection : ICollectionFixture<LoadFixture>;
