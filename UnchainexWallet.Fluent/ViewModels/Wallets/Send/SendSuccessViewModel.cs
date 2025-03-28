using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;
using UnchainexWallet.Blockchain.Transactions;
using UnchainexWallet.Fluent.ViewModels.Navigation;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Send;

public partial class SendSuccessViewModel : RoutableViewModel
{
	private readonly SmartTransaction _finalTransaction;

	private SendSuccessViewModel(SmartTransaction finalTransaction, string? title = null, string? caption = null)
	{
		_finalTransaction = finalTransaction;
		Title = title ?? "Payment successful";

		Caption = caption ?? "Your transaction has been successfully sent.";

		NextCommand = ReactiveCommand.CreateFromTask(OnNextAsync);

		SetupCancel(enableCancel: false, enableCancelOnEscape: true, enableCancelOnPressed: true);
	}

	public override string Title { get; protected set; }

	public string? Caption { get; }

	private async Task OnNextAsync()
	{
		await Task.Delay(UiConstants.CloseSuccessDialogMillisecondsDelay);

		Navigate().Clear();

		// TODO: Remove this
		MainViewModel.Instance.NavBar.SelectedWallet?.WalletViewModel?.SelectTransaction(_finalTransaction.GetHash());
	}

	protected override void OnNavigatedTo(bool isInHistory, CompositeDisposable disposables)
	{
		base.OnNavigatedTo(isInHistory, disposables);

		if (NextCommand is not null && NextCommand.CanExecute(default))
		{
			NextCommand.Execute(default);
		}
	}
}
