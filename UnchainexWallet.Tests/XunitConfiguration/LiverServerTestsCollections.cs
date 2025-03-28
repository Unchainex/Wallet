using Xunit;

namespace UnchainexWallet.Tests.XunitConfiguration;

[CollectionDefinition("LiveServerTests collection")]
public class LiverServerTestsCollections : ICollectionFixture<LiveServerTestsFixture>
{
}
