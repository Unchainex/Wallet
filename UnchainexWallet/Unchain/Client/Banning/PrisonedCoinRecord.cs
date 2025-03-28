using NBitcoin;
using UnchainexWallet.JsonConverters;

namespace UnchainexWallet.Unchain.Client.Banning;

public record PrisonedCoinRecord
{
	public PrisonedCoinRecord(OutPoint outpoint, DateTimeOffset bannedUntil)
	{
		Outpoint = outpoint;
		BannedUntil = bannedUntil;
	}

	public OutPoint Outpoint { get; set; }

	public DateTimeOffset BannedUntil { get; set; }
}
