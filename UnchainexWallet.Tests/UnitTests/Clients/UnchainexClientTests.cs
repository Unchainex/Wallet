using NBitcoin;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using UnchainexWallet.WebClients.Unchainex;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Clients;

public class UnchainexClientTests
{
	[Fact]
	public void ConstantsTests()
	{
		var min = int.Parse(UnchainexWallet.Helpers.Constants.ClientSupportBackendVersionMin);
		var max = int.Parse(UnchainexWallet.Helpers.Constants.ClientSupportBackendVersionMax);
		Assert.True(min <= max);

		int.Parse(UnchainexWallet.Helpers.Constants.BackendMajorVersion);
	}
}
