using Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguStoreGeneratedFixup")]
public sealed class XuguStoreGeneratedFixupCollection : ICollectionFixture<StoreGeneratedFixupFixture>;
