using NBitcoin;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Backend.Models;
using UnchainexWallet.BitcoinRpc;
using UnchainexWallet.Blockchain.BlockFilters;
using UnchainexWallet.Blockchain.Blocks;
using UnchainexWallet.Extensions;
using UnchainexWallet.Logging;
using UnchainexWallet.Serialization;
using UnchainexWallet.Tests.XunitConfiguration;
using UnchainexWallet.WebClients.Unchainex;
using Xunit;
using Constants = UnchainexWallet.Helpers.Constants;

namespace UnchainexWallet.Tests.RegressionTests;

/// <seealso cref="RegTestCollectionDefinition"/>
[Collection("RegTest collection")]
public class BackendTests : IClassFixture<RegTestFixture>
{
	public BackendTests(RegTestFixture regTestFixture)
	{
		RegTestFixture = regTestFixture;
		BackendApiHttpClient = regTestFixture.BackendHttpClientFactory.CreateClient("test");
	}

	private HttpClient BackendApiHttpClient { get; }
	private RegTestFixture RegTestFixture { get; }

	[Fact]
	public async Task GetClientVersionAsync()
	{
		UnchainexClient client = new(BackendApiHttpClient);
		var backendCompatible = await client.CheckUpdatesAsync(CancellationToken.None);
		Assert.True(backendCompatible);
	}

	[Fact]
	public async Task BroadcastReplayTxAsync()
	{
		await using RegTestSetup setup = await RegTestSetup.InitializeTestEnvironmentAsync(RegTestFixture, numberOfBlocksToGenerate: 1);
		IRPCClient rpc = setup.RpcClient;

		var utxos = await rpc.ListUnspentAsync();
		var utxo = utxos[0];
		var tx = await rpc.GetRawTransactionAsync(utxo.OutPoint.Hash);
		using StringContent content = new($"'{tx.ToHex()}'", Encoding.UTF8, "application/json");

		Logger.TurnOff();

		using var response = await BackendApiHttpClient.PostAsync($"api/v{Constants.BackendMajorVersion}/btc/blockchain/broadcast", content);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal("Transaction is already in the blockchain.", await response.Content.ReadAsJsonAsync(Decode.String));

		Logger.TurnOn();
	}

	[Fact]
	public async Task BroadcastInvalidTxAsync()
	{
		await using RegTestSetup setup = await RegTestSetup.InitializeTestEnvironmentAsync(RegTestFixture, numberOfBlocksToGenerate: 1);

		using StringContent content = new($"''", Encoding.UTF8, "application/json");

		Logger.TurnOff();

		using var response = await BackendApiHttpClient.PostAsync($"api/v{Constants.BackendMajorVersion}/btc/blockchain/broadcast", content);

		Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Contains("The hex field is required.", await response.Content.ReadAsStringAsync());

		Logger.TurnOn();
	}

	[Fact]
	public async Task FilterBuilderTestAsync()
	{
		await using RegTestSetup setup = await RegTestSetup.InitializeTestEnvironmentAsync(RegTestFixture, numberOfBlocksToGenerate: 1);
		IRPCClient rpc = setup.RpcClient;
		using var blockNotifier = new BlockNotifier(rpc);
		IndexBuilderService indexBuilderService = new(rpc, blockNotifier, "filters.txt");
		try
		{
			indexBuilderService.Synchronize();

			// Test initial synchronization.
			var times = 0;
			uint256 firstHash = await rpc.GetBlockHashAsync(0);
			while (indexBuilderService.GetFilterLinesExcluding(firstHash, 101, out _).filters.Count() != 101)
			{
				if (times > 500) // 30 sec
				{
					throw new TimeoutException($"{nameof(IndexBuilderService)} test timed out.");
				}
				await Task.Delay(100);
				times++;
			}

			// Test later synchronization.
			await rpc.GenerateAsync(10);
			times = 0;
			while (indexBuilderService.GetFilterLinesExcluding(firstHash, 111, out bool found5).filters.Count() != 111)
			{
				Assert.True(found5);
				if (times > 500) // 30 sec
				{
					throw new TimeoutException($"{nameof(IndexBuilderService)} test timed out.");
				}
				await Task.Delay(100);
				times++;
			}

			// Test correct number of filters is received.
			var hundredthHash = await rpc.GetBlockHashAsync(100);
			Assert.Equal(11, indexBuilderService.GetFilterLinesExcluding(hundredthHash, 11, out bool found).filters.Count());
			Assert.True(found);
			var bestHash = await rpc.GetBestBlockHashAsync();
			Assert.Empty(indexBuilderService.GetFilterLinesExcluding(bestHash, 1, out bool found2).filters);
			Assert.Empty(indexBuilderService.GetFilterLinesExcluding(uint256.Zero, 1, out bool found3).filters);
			Assert.False(found3);

			// Test filter block hashes are correct.
			var filters = indexBuilderService.GetFilterLinesExcluding(firstHash, 111, out bool found4).filters.ToArray();
			Assert.True(found4);
			for (int i = 0; i < 111; i++)
			{
				var expectedHash = await rpc.GetBlockHashAsync(i + 1);
				var filterModel = filters[i];
				Assert.Equal(expectedHash, filterModel.Header.BlockHash);
			}
		}
		finally
		{
			await indexBuilderService.StopAsync();
		}
	}
}
