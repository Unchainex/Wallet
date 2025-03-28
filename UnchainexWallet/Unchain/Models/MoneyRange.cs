using NBitcoin;

namespace UnchainexWallet.Unchain.Models;

public record MoneyRange(Money Min, Money Max)
{
	public bool Contains(Money value) =>
		value >= Min && value <= Max;
}
