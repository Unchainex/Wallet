using NBitcoin;
using ReactiveUI;
using System.Reactive.Linq;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.Infrastructure;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.Models.Wallets;

[AppLifetime]
[AutoInterface]
public partial class WalletSettingsModel : ReactiveObject
{
	private readonly KeyManager _keyManager;
	private bool _isDirty;

	[AutoNotify] private bool _isNewWallet;
	[AutoNotify] private bool _autoCoinjoin;
	[AutoNotify] private bool _preferPsbtWorkflow;
	[AutoNotify] private Money _plebStopThreshold;
	[AutoNotify] private int _anonScoreTarget;
	[AutoNotify] private bool _nonPrivateCoinIsolation;
	[AutoNotify] private int _feeRateMedianTimeFrameHours;
	[AutoNotify] private WalletId? _outputWalletId;
	[AutoNotify] private ScriptType _defaultReceiveScriptType;
	[AutoNotify] private UnchainexWallet.Models.PreferredScriptPubKeyType _changeScriptPubKeyType;
	[AutoNotify] private UnchainexWallet.Models.SendWorkflow _defaultSendWorkflow;

	public WalletSettingsModel(KeyManager keyManager, bool isNewWallet = false, bool isCoinJoinPaused = false)
	{
		_keyManager = keyManager;

		_isNewWallet = isNewWallet;
		_isDirty = isNewWallet;
		IsCoinJoinPaused = isCoinJoinPaused;

		_autoCoinjoin = _keyManager.AutoCoinJoin;
		_preferPsbtWorkflow = _keyManager.PreferPsbtWorkflow;
		_plebStopThreshold = _keyManager.PlebStopThreshold ?? KeyManager.DefaultPlebStopThreshold;
		_anonScoreTarget = _keyManager.AnonScoreTarget;
		_nonPrivateCoinIsolation = _keyManager.NonPrivateCoinIsolation;
		_feeRateMedianTimeFrameHours = _keyManager.FeeRateMedianTimeFrameHours;

		if (!isNewWallet)
		{
			_outputWalletId = Services.WalletManager.GetWalletByName(_keyManager.WalletName).WalletId;
		}

		_defaultReceiveScriptType = ScriptType.FromEnum(_keyManager.DefaultReceiveScriptType);
		_changeScriptPubKeyType = _keyManager.ChangeScriptPubKeyType;
		_defaultSendWorkflow = _keyManager.DefaultSendWorkflow;

		WalletType = WalletHelpers.GetType(_keyManager);

		this.WhenAnyValue(
				x => x.AutoCoinjoin,
				x => x.PreferPsbtWorkflow,
				x => x.PlebStopThreshold,
				x => x.AnonScoreTarget,
				x => x.NonPrivateCoinIsolation,
				x => x.FeeRateMedianTimeFrameHours)
			.Skip(1)
			.Do(_ => SetValues())
			.Subscribe();

		this.WhenAnyValue(
				x => x.DefaultSendWorkflow,
				x => x.DefaultReceiveScriptType,
				x => x.ChangeScriptPubKeyType)
			.Do(_ => SetValues())
			.Subscribe();
	}

	public WalletType WalletType { get; }

	public bool IsCoinJoinPaused { get; set; }

	/// <summary>
	/// Saves to current configuration to file.
	/// </summary>
	/// <returns>The unique ID of the wallet.</returns>
	public WalletId Save()
	{
		if (_isDirty)
		{
			_keyManager.ToFile();

			if (IsNewWallet)
			{
				Services.WalletManager.AddWallet(_keyManager);
				IsNewWallet = false;
				OutputWalletId = Services.WalletManager.GetWalletByName(_keyManager.WalletName).WalletId;
			}

			_isDirty = false;
		}

		return Services.WalletManager.GetWalletByName(_keyManager.WalletName).WalletId;
	}

	private void SetValues()
	{
		_keyManager.AutoCoinJoin = AutoCoinjoin;
		_keyManager.PreferPsbtWorkflow = PreferPsbtWorkflow;
		_keyManager.PlebStopThreshold = PlebStopThreshold;
		_keyManager.AnonScoreTarget = AnonScoreTarget;
		_keyManager.NonPrivateCoinIsolation = NonPrivateCoinIsolation;
		_keyManager.SetFeeRateMedianTimeFrame(FeeRateMedianTimeFrameHours);
		_keyManager.DefaultSendWorkflow = DefaultSendWorkflow;
		_keyManager.DefaultReceiveScriptType = ScriptType.ToScriptPubKeyType(DefaultReceiveScriptType);
		_keyManager.ChangeScriptPubKeyType = ChangeScriptPubKeyType;
		_isDirty = true;
	}

	public void RescanWallet(int startingHeight = 0)
	{
		_keyManager.SetBestHeights(startingHeight, startingHeight);
	}
}
