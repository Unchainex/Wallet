using System.Collections.Generic;
using System.Linq;
using Unchain.Crypto.Randomness;
using UnchainexWallet.Extensions;

namespace UnchainexWallet.Crypto.Randomness;

public class MockRandom : UnchainexRandom
{
	public List<byte[]> GetBytesResults { get; } = new List<byte[]>();

	public override void GetBytes(Span<byte> output)
	{
		var first = GetBytesResults.First();
		GetBytesResults.RemoveFirst();
		first.AsSpan().CopyTo(output);
	}

	public override void GetBytes(byte[] output)
	{
		throw new NotImplementedException();
	}

	public override int GetInt(int fromInclusive, int toExclusive)
	{
		throw new NotImplementedException();
	}
}
