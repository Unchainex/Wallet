using NBitcoin;

namespace UnchainexWallet.Wallets.BlockProvider;

public record P2pBlockResponse(Block? Block, ISourceData SourceData);
