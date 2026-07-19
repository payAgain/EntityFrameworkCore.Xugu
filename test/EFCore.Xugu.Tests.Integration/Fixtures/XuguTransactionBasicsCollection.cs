using Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguTransactionBasics")]
public sealed class XuguTransactionBasicsCollection : ICollectionFixture<TransactionBasicsFixture>;
