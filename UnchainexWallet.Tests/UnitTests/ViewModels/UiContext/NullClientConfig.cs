using UnchainexWallet.Fluent.Models.ClientConfig;

namespace UnchainexWallet.Tests.UnitTests.ViewModels.UIContext;

public class NullClientConfig : IClientConfig
{
	public string DataDir => "";

	public string WalletsDir => "";

	public string WalletsBackupDir => "";

	public string ConfigFilePath => "";

	public string TorLogFilePath => "";

	public string LoggerFilePath => "";
}
