using NBitcoin;

namespace UnchainexWallet.Unchain.Models;

public record CoinJoinFeeRateMedian(TimeSpan TimeFrame, FeeRate MedianFeeRate);
