namespace UnchainexWallet.Fluent.Models;

public enum HealthMonitorState
{
	Loading,
	Ready,
	BackendNotCompatible,
	UpdateAvailable,
	ConnectionIssueDetected,
	BitcoinCoreIssueDetected,
	BitcoinCoreSynchronizingOrConnecting,
}
