using System.IO;
using System.Text.Json.Nodes;
using UnchainexWallet.Helpers;
using UnchainexWallet.Serialization;
using static UnchainexWallet.Serialization.Encode;
using static UnchainexWallet.Serialization.Decode;

namespace UnchainexWallet.Backend;

public static class ConfigEncode
{
	public static JsonNode Config(Config cfg) =>
		Object([
			("Network", Network(cfg.Network) ),
			("BitcoinRpcConnectionString", String(cfg.BitcoinRpcConnectionString) ),
			("MainNetBitcoinCoreRpcEndPoint", EndPoint(cfg.MainNetBitcoinCoreRpcEndPoint, Constants.DefaultMainNetBitcoinCoreRpcPort) ),
			("TestNetBitcoinCoreRpcEndPoint", EndPoint(cfg.TestNetBitcoinCoreRpcEndPoint, Constants.DefaultTestNetBitcoinCoreRpcPort) ),
			("RegTestBitcoinCoreRpcEndPoint", EndPoint(cfg.RegTestBitcoinCoreRpcEndPoint, Constants.DefaultRegTestBitcoinCoreRpcPort) ),
		]);
}

public static class ConfigDecode
{
	public static Decoder<Config> Config(string filePath) =>
		Object(get => new Config(
			filePath,
			get.Required("Network", Decode.Network ),
			get.Required("BitcoinRpcConnectionString", Decode.String ),
			get.Required("MainNetBitcoinCoreRpcEndPoint", Decode.EndPoint ),
			get.Required("TestNetBitcoinCoreRpcEndPoint", Decode.EndPoint ),
			get.Required("RegTestBitcoinCoreRpcEndPoint", Decode.EndPoint )
		));
}
