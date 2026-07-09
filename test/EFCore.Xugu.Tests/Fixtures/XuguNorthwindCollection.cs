using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguNorthwind")]
public sealed class XuguNorthwindCollection : ICollectionFixture<XuguNorthwindQueryFixture>;
