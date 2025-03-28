using UnchainexWallet.Announcements;
using UnchainexWallet.Fluent.Models.ClientConfig;
using UnchainexWallet.Fluent.Models.FileSystem;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels;
using UnchainexWallet.Fluent.ViewModels.Navigation;
using UnchainexWallet.Fluent.ViewModels.SearchBar.Sources;

namespace UnchainexWallet.Fluent.Models.UI;

public class UiContext
{
	/// <summary>
	///     The use of this property is a temporary workaround until we finalize the refactoring of all ViewModels (to be
	///     testable)
	/// </summary>
	public static UiContext Default;

	private INavigate? _navigate;

	public UiContext(
		IQrCodeGenerator qrCodeGenerator,
		IQrCodeReader qrCodeReader,
		IUiClipboard clipboard,
		IWalletRepository walletRepository,
		ICoinjoinModel coinJoinModel,
		IHardwareWalletInterface hardwareWalletInterface,
		IFileSystem fileSystem,
		IClientConfig config,
		IApplicationSettings applicationSettings,
		ITransactionBroadcasterModel transactionBroadcaster,
		IAmountProvider amountProvider,
		IEditableSearchSource editableSearchSource,
		ITorStatusCheckerModel torStatusChecker,
		IHealthMonitor healthMonitor,
		ReleaseHighlights releaseHighlights)
	{
		QrCodeGenerator = qrCodeGenerator ?? throw new ArgumentNullException(nameof(qrCodeGenerator));
		QrCodeReader = qrCodeReader ?? throw new ArgumentNullException(nameof(qrCodeReader));
		Clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
		WalletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
		CoinjoinModel = coinJoinModel ?? throw new ArgumentNullException(nameof(coinJoinModel));
		HardwareWalletInterface = hardwareWalletInterface ?? throw new ArgumentNullException(nameof(hardwareWalletInterface));
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		Config = config ?? throw new ArgumentNullException(nameof(config));
		ApplicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
		TransactionBroadcaster = transactionBroadcaster ?? throw new ArgumentNullException(nameof(transactionBroadcaster));
		AmountProvider = amountProvider ?? throw new ArgumentNullException(nameof(amountProvider));
		EditableSearchSource = editableSearchSource ?? throw new ArgumentNullException(nameof(editableSearchSource));
		TorStatusChecker = torStatusChecker ?? throw new ArgumentNullException(nameof(torStatusChecker));
		HealthMonitor = healthMonitor ?? throw new ArgumentNullException(nameof(healthMonitor));
		ReleaseHighlights = releaseHighlights ?? throw new ArgumentNullException(nameof(releaseHighlights));
	}

	public IUiClipboard Clipboard { get; }
	public IQrCodeGenerator QrCodeGenerator { get; }
	public IWalletRepository WalletRepository { get; }
	public ICoinjoinModel CoinjoinModel { get; }
	public IQrCodeReader QrCodeReader { get; }
	public IHardwareWalletInterface HardwareWalletInterface { get; }
	public IFileSystem FileSystem { get; }
	public IClientConfig Config { get; }
	public IApplicationSettings ApplicationSettings { get; }
	public ITransactionBroadcasterModel TransactionBroadcaster { get; }
	public IAmountProvider AmountProvider { get; }
	public IEditableSearchSource EditableSearchSource { get; }
	public ITorStatusCheckerModel TorStatusChecker { get; }
	public IHealthMonitor HealthMonitor { get; }
	public ReleaseHighlights ReleaseHighlights { get; }
	public MainViewModel? MainViewModel { get; private set; }

	public void RegisterNavigation(INavigate navigate)
	{
		_navigate ??= navigate;
	}

	public INavigate Navigate()
	{
		return _navigate ?? throw new InvalidOperationException($"{GetType().Name} {nameof(Navigate)} hasn't been initialized.");
	}

	public INavigationStack<RoutableViewModel> Navigate(NavigationTarget target)
	{
		return
			_navigate?.Navigate(target)
			?? throw new InvalidOperationException($"{GetType().Name} {nameof(Navigate)} hasn't been initialized.");
	}

	public void SetMainViewModel(MainViewModel viewModel)
	{
		MainViewModel ??= viewModel;
	}
}
