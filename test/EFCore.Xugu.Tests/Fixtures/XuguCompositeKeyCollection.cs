using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguCompositeKey")]
public sealed class XuguCompositeKeyCollection : ICollectionFixture<CompositeKeyFixture>;
