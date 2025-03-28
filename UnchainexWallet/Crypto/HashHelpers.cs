using System.Security.Cryptography;
using System.Text;
using UnchainexWallet.Helpers;

namespace UnchainexWallet.Crypto;

public static class HashHelpers
{
	/// <returns>SHA-256 hash. Letters are always in upper-case.</returns>
	public static string GenerateSha256Hash(string input) => ByteHelpers.ToHex(GenerateSha256Hash(Encoding.UTF8.GetBytes(input)));

	public static byte[] GenerateSha256Hash(byte[] input)
	{
		var hash = SHA256.HashData(input);

		return hash;
	}

	public static int ComputeHashCode(params byte[] data)
	{
		var hash = new HashCode();
		foreach (var element in data)
		{
			hash.Add(element);
		}
		return hash.ToHashCode();
	}
}
