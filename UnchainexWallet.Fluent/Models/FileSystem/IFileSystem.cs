using System.Threading.Tasks;

namespace UnchainexWallet.Fluent.Models.FileSystem;

public interface IFileSystem
{
	void OpenFolderInFileExplorer(string dirPath);

	Task OpenFileInTextEditorAsync(string filePath);

	Task OpenBrowserAsync(string url);
}
