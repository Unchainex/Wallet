using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;
using NBitcoin;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Helpers;
using UnchainexWallet.Models;
using UnchainexWallet.Unchain.Client.Banning;

namespace UnchainexWallet.Serialization;

public static partial class Encode
{
	public static JsonNode KeyPath(KeyPath kp) =>
		String(kp.ToString());

	public static JsonNode ByteArray(byte[] bytes) =>
		String(Convert.ToBase64String(bytes));

	public static JsonNode? ChainCode(byte[]? bytes) =>
		Optional(bytes, ByteArray);

	public static JsonNode PubKey(PubKey pk) =>
		String(pk.ToHex());

	public static JsonNode LabelsArray(LabelsArray a) =>
		String(a.ToString());

	public static JsonNode HdPubKey(HdPubKey hd) =>
		Object([
			("PubKey", PubKey(hd.PubKey)),
			("FullKeyPath", KeyPath(hd.FullKeyPath) ),
			("Label", LabelsArray(hd.Labels) ),
			("KeyState", Int((int)hd.KeyState) ),
		]);

	public static JsonNode? HDFingerprint(HDFingerprint? fp) =>
		Optional(fp?.ToString(), String);

	public static JsonNode? BitcoinEncryptedSecretNoEC(BitcoinEncryptedSecretNoEC? s) =>
		Optional(s?.ToWif(), String);

	public static JsonNode WalletHeight(Height height) =>
		String(Math.Max(0, height.Value - 101).ToString());

	public static JsonNode BlockchainState(BlockchainState s) =>
		Object([
			("Network", Network(s.Network)),
			("Height", WalletHeight(s.Height)),
			("TurboSyncHeight", WalletHeight(s.TurboSyncHeight))
		]);

	public static JsonNode RuntimeParams(RuntimeParams p) =>
		Object([
			("NetworkNodeTimeout", Int(p.NetworkNodeTimeout))
		]);

	public static JsonNode PreferredScriptPubKeyType(PreferredScriptPubKeyType t) =>
		t switch
		{
			PreferredScriptPubKeyType.Specified {ScriptType: NBitcoin.ScriptPubKeyType.Segwit} => String("Segwit"),
			PreferredScriptPubKeyType.Specified {ScriptType: NBitcoin.ScriptPubKeyType.TaprootBIP86} => String("Taproot"),
			PreferredScriptPubKeyType.Unspecified _ => String("Random"),
			_ => throw new ArgumentOutOfRangeException(nameof(t))
		};

	public static JsonNode ScriptPubKeyType(ScriptPubKeyType w) =>
		w switch
		{
			NBitcoin.ScriptPubKeyType.Segwit => "Segwit",
			NBitcoin.ScriptPubKeyType.TaprootBIP86 => "Taproot",
			_ => throw new ArgumentOutOfRangeException(nameof(w))
		};

	public static JsonNode SendWorkflow(SendWorkflow w) =>
		w switch
		{
			Models.SendWorkflow.Automatic => "Automatic",
			Models.SendWorkflow.Manual => "Manual",
			_ => throw new ArgumentOutOfRangeException(nameof(w))
		};

	public static JsonNode SerializableException(SerializableException e) =>
		Object([
			("ExceptionType", Optional(e.ExceptionType, String)),
			("Message", String(e.Message)),
			("StackTrace", Optional(e.StackTrace, String)),
			("InnerException", Optional(e.InnerException, SerializableException))
		]);

	public static JsonNode PrisonedCoinRecord(PrisonedCoinRecord r) =>
		Object([
			("Outpoint", Outpoint(r.Outpoint)),
			("BannedUntil", DatetimeOffset(r.BannedUntil))
		]);

	public static JsonNode ClientPrison(IEnumerable<PrisonedCoinRecord> p) =>
		Array(p.Select(PrisonedCoinRecord));
}


public static partial class Decode
{
	public static readonly Decoder<byte[]> ByteArray =
		String.Map(Convert.FromBase64String);

	public static readonly Decoder<HDFingerprint> HDFingerprint =
		String.Map(NBitcoin.HDFingerprint.Parse);

	public static readonly Decoder<Height> WalletHeight =
		Int.Map(h => new Height(h));

	public static readonly Decoder<KeyPath> KeyPath =
		String.Map(NBitcoin.KeyPath.Parse);

	public static readonly Decoder<PubKey> PubKey =
		String.Map(s => new PubKey(s));

	public static readonly Decoder<LabelsArray> LabelsArray =
		String.Map(s => new LabelsArray(s));

	public static readonly Decoder<HdPubKey> HdPubKey =
		Object(get => new HdPubKey(
			get.Required("PubKey", PubKey),
			get.Required("FullKeyPath", KeyPath),
			get.Required("Label", LabelsArray),
			get.Required("KeyState", Int.Map(x => (KeyState)x))
		));

	public static readonly Decoder<BlockchainState> BlockchainState =
		Object(get => new BlockchainState(
			get.Required("Network", Network),
			get.Required("Height", WalletHeight),
			get.Required("TurboSyncHeight", WalletHeight)
			));

	public static readonly Decoder<BitcoinEncryptedSecretNoEC> BitcoinEncryptedSecretNoEC =
		String.Map(s => new BitcoinEncryptedSecretNoEC(s, NBitcoin.Network.Main));

	public static readonly Decoder<PreferredScriptPubKeyType> PreferredScriptPubKeyType =
		String.Map(s => s switch
		{
			"Segwit" => new PreferredScriptPubKeyType.Specified(NBitcoin.ScriptPubKeyType.Segwit),
			"Taproot" => new PreferredScriptPubKeyType.Specified(NBitcoin.ScriptPubKeyType.TaprootBIP86),
			"Random" => (PreferredScriptPubKeyType)UnchainexWallet.Models.PreferredScriptPubKeyType.Unspecified.Instance,
			_ => throw new Exception($"Unknown ScriptPubKeyType '{s}'")
		}).Catch();

	public static readonly Decoder<ScriptPubKeyType> ScriptPubKeyType =
		String.Map(s => s switch
		{
			"Segwit" => NBitcoin.ScriptPubKeyType.Segwit,
			"Taproot" => NBitcoin.ScriptPubKeyType.TaprootBIP86,
			_ => throw new Exception($"Unknown ScriptPubKeyType '{s}'")
		}).Catch();

	public static readonly Decoder<SendWorkflow> SendWorkflow =
		String.Map(s => s switch
		{
			"Automatic" => Models.SendWorkflow.Automatic,
			"Manual" => Models.SendWorkflow.Manual,
			_ => throw new Exception($"Unknown SendWorkflow value '{s}'")
		}).Catch();

	public static readonly Decoder<RuntimeParams> RuntimeParams =
		Object(get => new RuntimeParams{
			NetworkNodeTimeout = get.Required("NetworkNodeTimeout", Int)
		});

	public static readonly Decoder<SerializableException> SerializableException =
		Object(get => new SerializableException(
			get.Required("ExceptionType", String),
			get.Required("Message", String),
			get.Required("StackTrace", String),
			get.Optional("InnerException", SerializableException)
		));

	public static readonly Decoder<PrisonedCoinRecord> PrisonedCoinRecord =
		Object(get => new PrisonedCoinRecord(
			get.Required("Outpoint", OutPoint),
			get.Required("BannedUntil", DateTimeOffset)
		));
}
