using System.Security.Cryptography;
using Unchain.Crypto.Randomness;

namespace UnchainexWallet.Crypto.Randomness;

public class SecureRandom : UnchainexRandom
{
	public static readonly SecureRandom Instance = new();

	public override void GetBytes(byte[] buffer)
	{
		RandomNumberGenerator.Fill(buffer);
	}

	public override void GetBytes(Span<byte> buffer)
	{
		RandomNumberGenerator.Fill(buffer);
	}

	public override int GetInt(int fromInclusive, int toExclusive)
	{
		return RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);
	}
}
