using NBitcoin.Crypto;
using NBitcoin;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Linq;

namespace UnchainexWallet.Helpers;

public class UnchainexSignerHelpers
{
	public static async Task SignSha256SumsFileAsync(string sha256SumsAscFilePath, Key unchainexPrivateKey)
	{
		var computedHash = await GetShaComputedBytesOfFileAsync(sha256SumsAscFilePath).ConfigureAwait(false);

		ECDSASignature signature = unchainexPrivateKey.Sign(new uint256(computedHash));

		string base64Signature = Convert.ToBase64String(signature.ToDER());
		var unchainexSignatureFilePath = Path.ChangeExtension(sha256SumsAscFilePath, "unchainexsig");

		await File.WriteAllTextAsync(unchainexSignatureFilePath, base64Signature).ConfigureAwait(false);
	}

	public static async Task VerifySha256SumsFileAsync(string sha256SumsAscFilePath)
	{
		// Read the content file
		byte[] hash = await GetShaComputedBytesOfFileAsync(sha256SumsAscFilePath).ConfigureAwait(false);

		// Read the signature file
		var unchainexSignatureFilePath = Path.ChangeExtension(sha256SumsAscFilePath, "unchainexsig");
		string signatureText = await File.ReadAllTextAsync(unchainexSignatureFilePath).ConfigureAwait(false);
		byte[] signatureBytes = Convert.FromBase64String(signatureText);

		VerifySha256Sum(hash, signatureBytes);
	}

	public static void VerifySha256Sum(byte[] sha256Hash, byte[] signatureBytes)
	{
		ECDSASignature unchainexSignature = ECDSASignature.FromDER(signatureBytes);

		PubKey pubKey = new(Constants.UnchainexPubKey);

		if (!pubKey.Verify(new uint256(sha256Hash), unchainexSignature))
		{
			throw new InvalidOperationException("Invalid unchainex signature.");
		}
	}

	public static async Task GeneratePrivateAndPublicKeyToFileAsync(string unchainexPrivateKeyFilePath, string unchainexPublicKeyFilePath)
	{
		if (File.Exists(unchainexPrivateKeyFilePath))
		{
			throw new ArgumentException("Private key file already exists.");
		}

		IoHelpers.EnsureContainingDirectoryExists(unchainexPrivateKeyFilePath);

		using Key key = new();
		await File.WriteAllTextAsync(unchainexPrivateKeyFilePath, key.ToString(Network.Main)).ConfigureAwait(false);
		await File.WriteAllTextAsync(unchainexPublicKeyFilePath, key.PubKey.ToString()).ConfigureAwait(false);
	}

	public static async Task<Key> GetPrivateKeyFromFileAsync(string unchainexPrivateKeyFilePath)
	{
		string keyFileContent = await File.ReadAllTextAsync(unchainexPrivateKeyFilePath).ConfigureAwait(false);
		BitcoinSecret secret = new(keyFileContent, Network.Main);
		return secret.PrivateKey;
	}

	public static async Task VerifyInstallerFileHashesAsync(string[] finalFiles, string sha256SumsFilePath)
	{
		string[] lines = await File.ReadAllLinesAsync(sha256SumsFilePath).ConfigureAwait(false);
		var hashWithFileNameLines = lines.Where(line => line.Contains("Unchainex-"));

		foreach (var installerFilePath in finalFiles)
		{
			string installerName = Path.GetFileName(installerFilePath);
			string installerExpectedHash = hashWithFileNameLines.Single(line => line.Contains(installerName)).Split(" ")[0];

			var bytes = await GetShaComputedBytesOfFileAsync(installerFilePath).ConfigureAwait(false);
			string installerRealHash = Convert.ToHexString(bytes).ToLower();

			if (installerExpectedHash != installerRealHash)
			{
				throw new InvalidOperationException("Installer file's hash doesn't match expected hash.");
			}
		}
	}

	/// <summary>
	/// This function returns a SHA256 computed byte array of a file on the provided file path.
	/// </summary>
	/// <exception cref="FileNotFoundException"></exception>
	public static async Task<byte[]> GetShaComputedBytesOfFileAsync(string filePath, CancellationToken cancellationToken = default)
	{
		byte[] bytes = await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
		byte[] computedHash = SHA256.HashData(bytes);
		return computedHash;
	}
}
