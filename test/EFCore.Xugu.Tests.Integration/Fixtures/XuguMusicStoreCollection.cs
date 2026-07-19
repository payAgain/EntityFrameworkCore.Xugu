using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

[CollectionDefinition("XuguMusicStore")]
public sealed class XuguMusicStoreCollection : ICollectionFixture<MusicStoreFixture>;
