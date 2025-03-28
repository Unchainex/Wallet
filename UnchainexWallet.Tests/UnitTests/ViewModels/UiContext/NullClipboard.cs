using System.Threading.Tasks;
using UnchainexWallet.Fluent.Models.UI;

namespace UnchainexWallet.Tests.UnitTests.ViewModels.UIContext;

public class NullClipboard : IUiClipboard
{
	public Task<string> GetTextAsync()
	{
		return Task.FromResult("");
	}

	public Task SetTextAsync(string? text)
	{
		return Task.CompletedTask;
	}

	public Task ClearAsync()
	{
		return Task.CompletedTask;
	}
}
