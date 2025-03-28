using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using UnchainexWallet.Blockchain.Transactions;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.ViewModels.Dialogs.Base;
using UnchainexWallet.Logging;

namespace UnchainexWallet.Fluent.ViewModels.TransactionBroadcasting;

[NavigationMetaData(Title = "Transaction Broadcaster")]
public partial class LoadTransactionViewModel : DialogViewModelBase<SmartTransaction?>
{
	[AutoNotify] private SmartTransaction? _finalTransaction;

	public LoadTransactionViewModel(UiContext uiContext)
	{
		UiContext = uiContext;

		SetupCancel(enableCancel: true, enableCancelOnEscape: true, enableCancelOnPressed: true);

		EnableBack = false;

		this.WhenAnyValue(x => x.FinalTransaction)
			.WhereNotNull()
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(finalTransaction => Close(result: finalTransaction));

		ImportTransactionCommand = ReactiveCommand.CreateFromTask(OnImportTransactionAsync, outputScheduler: RxApp.MainThreadScheduler);

		PasteCommand = ReactiveCommand.CreateFromTask(OnPasteAsync);
	}

	public ICommand PasteCommand { get; }

	public ICommand ImportTransactionCommand { get; }

	private async Task OnImportTransactionAsync()
	{
		try
		{
			var file = await FileDialogHelper.OpenFileAsync("Import Transaction", new[] { "psbt", "txn", "*" });
			if (file is { })
			{
				var filePath = file.Path.LocalPath;
				FinalTransaction = await UiContext.TransactionBroadcaster.LoadFromFileAsync(filePath);
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
			await ShowErrorAsync(Title, ex.ToUserFriendlyString(), "It was not possible to load the transaction.");
		}
	}

	private async Task OnPasteAsync()
	{
		try
		{
			var textToPaste = await UiContext.Clipboard.GetTextAsync();

			if (string.IsNullOrWhiteSpace(textToPaste))
			{
				throw new InvalidDataException("The clipboard is empty!");
			}

			FinalTransaction = UiContext.TransactionBroadcaster.Parse(textToPaste);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
			await ShowErrorAsync(Title, ex.ToUserFriendlyString(), "It was not possible to paste the transaction.");
		}
	}
}
