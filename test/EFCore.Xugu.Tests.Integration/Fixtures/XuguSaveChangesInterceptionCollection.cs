using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguSaveChangesInterception")]
public sealed class XuguSaveChangesInterceptionCollection : ICollectionFixture<SaveChangesInterceptionFixture>;
