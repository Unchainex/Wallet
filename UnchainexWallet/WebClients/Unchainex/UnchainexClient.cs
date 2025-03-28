using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using NBitcoin.RPC;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Backend.Models;
using UnchainexWallet.Backend.Models.Responses;
using UnchainexWallet.Blockchain.Transactions;
using UnchainexWallet.Extensions;
using UnchainexWallet.Helpers;
using UnchainexWallet.Logging;
using UnchainexWallet.Serialization;
using UnchainexWallet.Services;

namespace UnchainexWallet.WebClients.Unchainex;

public abstract record FiltersResponse
{
	public record AlreadyOnBestBlock : FiltersResponse;

	public record BestBlockUnknown : FiltersResponse;

	public record NewFiltersAvailable(int BestHeight, FilterModel[] Filters) : FiltersResponse;
}

public class UnchainexClient
{
	public UnchainexClient(HttpClient httpClient, EventBus? eventBus = null)
	{
		_httpClient = httpClient;
		_eventBus = eventBus ?? new EventBus();
	}

	private readonly HttpClient _httpClient;
	private readonly EventBus _eventBus;

	public static ushort ApiVersion { get; private set; } = ushort.Parse(Helpers.Constants.BackendMajorVersion);

	public async Task<FiltersResponse> GetFiltersAsync(uint256 bestKnownBlockHash, int count, CancellationToken cancel = default)
	{
		using HttpResponseMessage response = await _httpClient.GetAsync(
			$"api/v{ApiVersion}/btc/blockchain/filters?bestKnownBlockHash={bestKnownBlockHash}&count={count}",
			cancellationToken: cancel).ConfigureAwait(false);

		if (response.StatusCode == HttpStatusCode.NoContent)
		{
			_eventBus.Publish(new BackendAvailabilityStateChanged(true));
			return new FiltersResponse.AlreadyOnBestBlock();
		}

		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			_eventBus.Publish(new BackendAvailabilityStateChanged(true));
			return new FiltersResponse.BestBlockUnknown();
		}

		await CheckErrorsAsync(response, cancel).ConfigureAwait(false);

		using HttpContent content = response.Content;
		var ret = await content.ReadAsJsonAsync(Decode.FiltersResponse).ConfigureAwait(false);

		return new FiltersResponse.NewFiltersAvailable(ret.BestHeight, ret.Filters.ToArray());
	}

	public async Task BroadcastAsync(SmartTransaction transaction, CancellationToken cancellationToken)
	{
		using var content = new StringContent($"\"{transaction.Transaction.ToHex()}\"", Encoding.UTF8, "application/json");
		using HttpResponseMessage response = await _httpClient.PostAsync($"api/v{ApiVersion}/btc/blockchain/broadcast", content, cancellationToken).ConfigureAwait(false);

		await CheckErrorsAsync(response, cancellationToken).ConfigureAwait(false);
	}

	public async Task<ushort> GetBackendMajorVersionAsync(CancellationToken cancel)
	{
		using HttpResponseMessage response = await _httpClient.GetAsync("api/software/versions", cancellationToken: cancel).ConfigureAwait(false);

		await CheckErrorsAsync(response, cancel).ConfigureAwait(false);

		using HttpContent content = response.Content;
		var resp = await content.ReadAsJsonAsync(Decode.VersionsResponse).ConfigureAwait(false);

		return ushort.Parse(resp.BackendMajorVersion);
	}

	public async Task<bool> CheckUpdatesAsync(CancellationToken cancel)
	{
		ushort backendMajorVersion;
		try
		{
			 backendMajorVersion = await GetBackendMajorVersionAsync(cancel).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Logger.LogWarning($"Could not get the backend major version: {ex}");
			throw;
		}

		// If ClientSupportBackendVersionMin <= backend major <= ClientSupportBackendVersionMax, then our software is compatible.
		var backendCompatible = int.Parse(Helpers.Constants.ClientSupportBackendVersionMax) >= backendMajorVersion && backendMajorVersion >= int.Parse(Helpers.Constants.ClientSupportBackendVersionMin);
		var currentBackendMajorVersion = backendMajorVersion;

		if (backendCompatible)
		{
			// Only refresh if compatible.
			ApiVersion = currentBackendMajorVersion;
		}

		return backendCompatible;
	}

	private async Task CheckErrorsAsync(HttpResponseMessage response, CancellationToken cancel)
	{
		_eventBus.Publish(new BackendAvailabilityStateChanged(response.StatusCode == HttpStatusCode.OK));
		if (response.StatusCode != HttpStatusCode.OK)
		{
			await response.ThrowRequestExceptionFromContentAsync(cancel).ConfigureAwait(false);
		}
	}
}
