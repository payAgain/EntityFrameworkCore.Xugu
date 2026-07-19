using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguNotificationEntities")]
public sealed class XuguNotificationEntitiesCollection : ICollectionFixture<NotificationEntitiesFixture>;
