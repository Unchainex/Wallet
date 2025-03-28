using Unchain.Crypto.Randomness;
using UnchainexWallet.Helpers;

namespace UnchainexWallet.Crypto.Randomness;

public static class RandomString
{
	public static string FromCharacters(int length, string characters, bool secureRandom = false)
	{
		UnchainexRandom random = secureRandom ? SecureRandom.Instance : InsecureRandom.Instance;

		var res = random.GetString(length, characters);
		return res;
	}

	public static string AlphaNumeric(int length, bool secureRandom = false) => FromCharacters(length, Constants.AlphaNumericCharacters, secureRandom);

	public static string CapitalAlphaNumeric(int length, bool secureRandom = false) => FromCharacters(length, Constants.CapitalAlphaNumericCharacters, secureRandom);
}
