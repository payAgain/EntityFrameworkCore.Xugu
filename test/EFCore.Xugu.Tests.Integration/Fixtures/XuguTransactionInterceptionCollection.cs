using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguTransactionInterception")]
public sealed class XuguTransactionInterceptionCollection : ICollectionFixture<TransactionInterceptionFixture>;
