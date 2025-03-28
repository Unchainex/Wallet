using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels.Dialogs;
using AddressFunc = System.Func<UnchainexWallet.Fluent.Models.Wallets.IAddress, System.Threading.Tasks.Task>;
using AddressAction = System.Action<UnchainexWallet.Fluent.Models.Wallets.IAddress>;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Receive;

public partial class AddressViewModel : ViewModelBase, IDisposable
{
	private readonly CompositeDisposable _disposables = new();

	[AutoNotify] private string _addressText;
	[AutoNotify] private ScriptType _scriptType;
	[AutoNotify] private LabelsArray _labels;

	public AddressViewModel(UiContext context, AddressFunc onEdit, AddressAction onShow, IAddress address)
	{
		UiContext = context;
		Address = address;
		_addressText = address.ShortenedText;

		_scriptType = address.ScriptType;

		this.WhenAnyValue(x => x.Address.Labels)
			.BindTo(this, viewModel => viewModel.Labels)
			.DisposeWith(_disposables);

		CopyAddressCommand = ReactiveCommand.CreateFromTask(() => UiContext.Clipboard.SetTextAsync(Address.Text));
		HideAddressCommand = ReactiveCommand.CreateFromTask(PromptHideAddressAsync);
		EditLabelCommand = ReactiveCommand.CreateFromTask(() => onEdit(address));
		NavigateCommand = ReactiveCommand.Create(() => onShow(address));
	}

	private IAddress Address { get; }

	public ICommand CopyAddressCommand { get; }

	public ICommand HideAddressCommand { get; }

	public ICommand EditLabelCommand { get; }

	public ReactiveCommand<Unit, Unit> NavigateCommand { get; }

	private async Task PromptHideAddressAsync()
	{
		var result = await UiContext.Navigate().NavigateDialogAsync(new ConfirmHideAddressViewModel(Address.Labels));

		if (result.Result == false)
		{
			return;
		}

		Address.Hide();

		var isAddressCopied = await UiContext.Clipboard.GetTextAsync() == Address.Text;

		if (isAddressCopied)
		{
			await UiContext.Clipboard.ClearAsync();
		}
	}

	public void Dispose()
	{
		_disposables.Dispose();
	}
}
