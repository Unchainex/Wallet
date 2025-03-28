using System.Threading.Tasks;
using ReactiveUI;
using UnchainexWallet.Blockchain.Transactions;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Fluent.Models;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.ViewModels.Navigation;
using UnchainexWallet.Logging;

namespace UnchainexWallet.Fluent.ViewModels.TransactionBroadcasting;

[NavigationMetaData(Title = "Broadcast Transaction")]
public partial class BroadcastTransactionViewModel : RoutableViewModel
{
	public BroadcastTransactionViewModel(UiContext uiContext, SmartTransaction transaction)
	{
		UiContext = uiContext;

		SetupCancel(enableCancel: true, enableCancelOnEscape: true, enableCancelOnPressed: true);

		EnableBack = false;

		NextCommand = ReactiveCommand.CreateFromTask(async () => await OnNextAsync(transaction));

		EnableAutoBusyOn(NextCommand);

		BroadcastInfo = UiContext.TransactionBroadcaster.GetBroadcastInfo(transaction);
	}

	public TransactionBroadcastInfo BroadcastInfo { get; }

	private async Task OnNextAsync(SmartTransaction transaction)
	{
		try
		{
			await UiContext.TransactionBroadcaster.SendAsync(transaction);
			Navigate().To().Success();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
			await ShowErrorAsync("Broadcast Transaction", ex.ToUserFriendlyString(), "It was not possible to broadcast the transaction.");
		}
	}
}
