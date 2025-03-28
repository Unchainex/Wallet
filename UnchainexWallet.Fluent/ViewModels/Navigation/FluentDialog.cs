using System.Threading.Tasks;
using UnchainexWallet.Fluent.ViewModels.Dialogs.Base;

namespace UnchainexWallet.Fluent.ViewModels.Navigation;

public class FluentDialog<TResult>
{
	private readonly Task<DialogResult<TResult>> _resultTask;

	public FluentDialog(Task<DialogResult<TResult>> resultTask)
	{
		_resultTask = resultTask;
	}

	public async Task<TResult?> GetResultAsync()
	{
		var result = await _resultTask;

		return result.Result;
	}
}
