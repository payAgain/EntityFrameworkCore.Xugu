using Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguKeysWithConverters")]
public sealed class XuguKeysWithConvertersCollection : ICollectionFixture<KeysWithConvertersFixture>;
