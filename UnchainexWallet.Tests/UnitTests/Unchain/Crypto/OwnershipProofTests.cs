using NBitcoin;
using UnchainexWallet.Crypto;
using UnchainexWallet.Tests.Helpers;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Crypto;

/// <summary>
/// Tests for <see cref="OwnershipProof"/> class.
/// </summary>
public class OwnershipProofTests
{
	[Fact]
	public void EqualityTest()
	{
		using Key key = new();
		uint256 roundHash = BitcoinFactory.CreateUint256();

		// Request #1.
		OwnershipProof request1 = CreateOwnershipProof(key, roundHash);

		// Request #2.
		OwnershipProof request2 = CreateOwnershipProof(key, roundHash);

		Assert.Equal(request1, request2);

		// Request #3.
		using Key key2 = new();
		OwnershipProof request3 = CreateOwnershipProof(key2, roundHash); // Key intentionally changed.
		Assert.NotEqual(request1, request3);

		// Request #4.
		OwnershipProof request4 = CreateOwnershipProof(key, roundHash: BitcoinFactory.CreateUint256()); // Round hash intentionally changed.
		Assert.NotEqual(request1, request4);
	}

	/// <remarks>Each instance represents the same proof but a new object instance.</remarks>
	public static OwnershipProof CreateOwnershipProof(Key key, uint256 roundHash)
		=> OwnershipProof.GenerateCoinJoinInputProof(
			key,
			new OwnershipIdentifier(key, key.PubKey.GetScriptPubKey(ScriptPubKeyType.Segwit)),
			new CoinJoinInputCommitmentData("unchainex.org", roundHash),
			ScriptPubKeyType.Segwit);
}
