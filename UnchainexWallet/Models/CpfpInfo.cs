using System.Collections.Generic;
using NBitcoin;

namespace UnchainexWallet.Models;

public record RelativeCpfpInfo(uint256 TxId, long Fee, long Weight);
public record CpfpInfo(List<RelativeCpfpInfo> Ancestors, decimal Fee, decimal EffectiveFeePerVSize, decimal AdjustedVSize);
