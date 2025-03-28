using System.Net.Http;
using System.Threading.Tasks;
using UnchainexWallet.Serialization;

namespace UnchainexWallet.Extensions;

public static class HttpContentExtensions
{
	public static async Task<T> ReadAsJsonAsync<T>(this HttpContent me, Decoder<T> decoder)
	{
		var jsonString = await me.ReadAsStringAsync().ConfigureAwait(false);
		return JsonDecoder.FromString(jsonString, decoder)
			?? throw new InvalidOperationException("'null' is forbidden.");
	}
}
