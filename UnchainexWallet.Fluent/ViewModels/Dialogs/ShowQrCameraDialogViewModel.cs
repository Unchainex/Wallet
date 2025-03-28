using Avalonia.Media.Imaging;
using Avalonia.Threading;
using NBitcoin;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.ViewModels.Dialogs.Base;
using UnchainexWallet.Userfacing;
using UnchainexWallet.Userfacing.Bip21;

namespace UnchainexWallet.Fluent.ViewModels.Dialogs;

[NavigationMetaData(Title = "Camera", NavigationTarget = NavigationTarget.CompactDialogScreen)]
public partial class ShowQrCameraDialogViewModel : DialogViewModelBase<string?>
{
	private readonly Network _network;
	[AutoNotify] private Bitmap? _qrImage;
	[AutoNotify] private string _errorMessage = "";
	[AutoNotify] private string _qrContent = "";

	public ShowQrCameraDialogViewModel(UiContext context, Network network)
	{
		_network = network;

		SetupCancel(enableCancel: true, enableCancelOnEscape: true, enableCancelOnPressed: true);
		UiContext = context;
	}

	protected override void OnNavigatedTo(bool isInHistory, CompositeDisposable disposables)
	{
		base.OnNavigatedTo(isInHistory, disposables);

		UiContext.QrCodeReader
			.Read()
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(
				onNext: result =>
					AddressParser.Parse(result.decoded, _network).MatchDo(
						_ => Close(DialogResultKind.Normal, result.decoded),
						errorMessage =>
						{
							// Remember last error message and last QR content.
							if (!string.IsNullOrEmpty(result.decoded))
							{
								ErrorMessage = errorMessage;
							}

							if (!string.IsNullOrEmpty(result.decoded))
							{
								QrContent = result.decoded;
							}

							// ... but show always the current bitmap.
							QrImage = result.bitmap;
						}),
				onError: error => Dispatcher.UIThread.Post(async () =>
					{
						Close();
						await ShowErrorAsync(Title, error.Message, "Something went wrong", NavigationTarget.CompactDialogScreen);
					}))
			.DisposeWith(disposables);
	}
}
