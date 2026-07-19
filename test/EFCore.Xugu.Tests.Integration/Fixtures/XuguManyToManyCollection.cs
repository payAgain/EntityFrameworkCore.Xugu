using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguManyToMany")]
public sealed class XuguManyToManyCollection : ICollectionFixture<ManyToManyFixture>;
