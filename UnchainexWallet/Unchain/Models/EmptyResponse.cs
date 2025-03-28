using System.Threading.Tasks;

namespace UnchainexWallet.Unchain.Models;

/// <remarks>Type useful to avoid implementing API methods for both <see cref="Task"/> and <see cref="Task{TResult}"/> return types.</remarks>
public record EmptyResponse()
{
	public static readonly EmptyResponse Instance = new();
}
