using Avalonia;
using Avalonia.Skia;

namespace UnchainexWallet.Fluent.Controls.Rendering;

internal interface IDrawHandler : IDisposable
{
	void Draw(ISkiaSharpApiLease skia, Rect bounds);
}
