using System.Net.Http;
using UnchainexWallet.Blockchain.Blocks;
using UnchainexWallet.Blockchain.TransactionBroadcasting;
using UnchainexWallet.Daemon;
using UnchainexWallet.Helpers;
using UnchainexWallet.Services;
using UnchainexWallet.Services.Terminate;
using UnchainexWallet.Stores;
using UnchainexWallet.Tor;
using UnchainexWallet.Tor.StatusChecker;
using UnchainexWallet.Wallets;
using UnchainexWallet.WebClients.Unchainex;

namespace UnchainexWallet.Fluent;

public static class Services
{
	public static string DataDir { get; private set; } = null!;

	public static TorSettings TorSettings { get; private set; } = null!;

	public static BitcoinStore BitcoinStore { get; private set; } = null!;

	public static SmartHeaderChain SmartHeaderChain => BitcoinStore.SmartHeaderChain;

	public static IHttpClientFactory HttpClientFactory { get; private set; } = null!;

	public static string PersistentConfigFilePath { get; private set; } = null!;

	public static PersistentConfig PersistentConfig { get; private set; } = null!;

	public static WalletManager WalletManager { get; private set; } = null!;

	public static TransactionBroadcaster TransactionBroadcaster { get; private set; } = null!;

	public static HostedServices HostedServices { get; private set; } = null!;

	public static UiConfig UiConfig { get; private set; } = null!;

	public static SingleInstanceChecker SingleInstanceChecker { get; private set; } = null!;

	public static TerminateService TerminateService { get; private set; } = null!;

	public static Config Config { get; set; } = null!;

	public static UpdateManager UpdateManager { get; private set; } = null!;

	public static EventBus EventBus { get;  set; } = null;
	public static bool IsInitialized { get; private set; }

	/// <summary>
	/// Initializes global services used by fluent project.
	/// </summary>
	public static void Initialize(Global global, UiConfig uiConfig, SingleInstanceChecker singleInstanceChecker, TerminateService terminateService)
	{
		Guard.NotNull(nameof(global.DataDir), global.DataDir);
		Guard.NotNull(nameof(global.TorSettings), global.TorSettings);
		Guard.NotNull(nameof(global.BitcoinStore), global.BitcoinStore);
		Guard.NotNull(nameof(global.ExternalSourcesHttpClientFactory), global.ExternalSourcesHttpClientFactory);
		Guard.NotNull(nameof(global.Config), global.Config);
		Guard.NotNull(nameof(global.WalletManager), global.WalletManager);
		Guard.NotNull(nameof(global.TransactionBroadcaster), global.TransactionBroadcaster);
		Guard.NotNull(nameof(global.HostedServices), global.HostedServices);
		Guard.NotNull(nameof(global.UpdateManager), global.UpdateManager);
		Guard.NotNull(nameof(uiConfig), uiConfig);
		Guard.NotNull(nameof(terminateService), terminateService);

		DataDir = global.DataDir;
		TorSettings = global.TorSettings;
		BitcoinStore = global.BitcoinStore;
		HttpClientFactory = global.ExternalSourcesHttpClientFactory;
		PersistentConfigFilePath = global.ConfigFilePath;
		PersistentConfig = global.Config.PersistentConfig;
		WalletManager = global.WalletManager;
		TransactionBroadcaster = global.TransactionBroadcaster;
		HostedServices = global.HostedServices;
		UiConfig = uiConfig;
		SingleInstanceChecker = singleInstanceChecker;
		UpdateManager = global.UpdateManager;
		Config = global.Config;
		TerminateService = terminateService;
		EventBus = global.EventBus;

		IsInitialized = true;
	}
}
