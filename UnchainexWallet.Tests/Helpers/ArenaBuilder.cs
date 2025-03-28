using NBitcoin;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.BitcoinRpc;
using UnchainexWallet.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.DoSPrevention;
using UnchainexWallet.Unchain.Backend.Rounds;

namespace UnchainexWallet.Tests.Helpers;

/// <summary>
/// Builder class for <see cref="Arena"/>.
/// </summary>
public class ArenaBuilder
{
	public static ArenaBuilder Default => new();

	public TimeSpan? Period { get; set; }
	public Network? Network { get; set; }
	public UnchainConfig? Config { get; set; }
	public IRPCClient? Rpc { get; set; }
	public Prison? Prison { get; set; }
	public RoundParameterFactory? RoundParameterFactory { get; set; }

	/// <param name="rounds">Rounds to initialize <see cref="Arena"/> with.</param>
	public Arena Create(params Round[] rounds)
	{
		TimeSpan period = Period ?? TimeSpan.FromHours(1);
		Prison prison = Prison ?? UnchainFactory.CreatePrison();
		UnchainConfig config = Config ?? new();
		IRPCClient rpc = Rpc ?? UnchainFactory.CreatePreconfiguredRpcClient();
		Network network = Network ?? Network.Main;
		RoundParameterFactory roundParameterFactory = RoundParameterFactory ?? CreateRoundParameterFactory(config, network);

		Arena arena = new(config, rpc, prison, roundParameterFactory, period:period);

		foreach (var round in rounds)
		{
			arena.Rounds.Add(round);
		}

		return arena;
	}

	public Task<Arena> CreateAndStartAsync(params Round[] rounds)
		=> CreateAndStartAsync(rounds, CancellationToken.None);

	public async Task<Arena> CreateAndStartAsync(Round[] rounds, CancellationToken cancellationToken = default)
	{
		Arena? toDispose = null;

		try
		{
			toDispose = Create(rounds);
			Arena arena = toDispose;
			await arena.StartAsync(cancellationToken).ConfigureAwait(false);
			toDispose = null;
			return arena;
		}
		finally
		{
			toDispose?.Dispose();
		}
	}

	public ArenaBuilder With(IRPCClient rpc)
	{
		Rpc = rpc;
		return this;
	}

	public ArenaBuilder With(RoundParameterFactory roundParameterFactory)
	{
		RoundParameterFactory = roundParameterFactory;
		return this;
	}

	public static ArenaBuilder From(UnchainConfig cfg) => new() { Config = cfg };

	public static ArenaBuilder From(UnchainConfig cfg, Prison prison) => new() { Config = cfg, Prison = prison };

	public static ArenaBuilder From(UnchainConfig cfg, IRPCClient mockRpc, Prison prison) => new() { Config = cfg, Rpc = mockRpc, Prison = prison };

	private static RoundParameterFactory CreateRoundParameterFactory(UnchainConfig cfg, Network network) =>
		UnchainFactory.CreateRoundParametersFactory(cfg, network, Constants.P2wpkhInputVirtualSize + Constants.P2wpkhOutputVirtualSize);
}
