using NBitcoin;
using System.Collections.Generic;
using UnchainexWallet.Helpers;

namespace UnchainexWallet.Tests.XunitConfiguration;

public class LiveServerTestsFixture
{
	public Dictionary<Network, Uri> UriMappings { get; } = new Dictionary<Network, Uri>
	{
		{ Network.Main, new Uri(Constants.BackendUri) },
		{ Network.TestNet, new Uri(Constants.TestnetBackendUri) }
	};
}
