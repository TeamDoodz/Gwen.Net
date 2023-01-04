using System;

namespace Gwen.Net;

/// <summary>
/// Represents font resource.
/// </summary>
public class Font : IDisposable {
	private FontMetrics? fontMetrics;

	/// <summary>
	/// Font face name. Exact meaning depends on renderer.
	/// </summary>
	public string FaceName { get; set; }

	/// <summary>
	/// Font size.
	/// </summary>
	public int Size { get; set; }

	/// <summary>
	/// Enables or disables font smoothing (default: disabled).
	/// </summary>
	public bool Smooth { get; set; }

	public bool Bold { get; set; }
	public bool Italic { get; set; }
	public bool Underline { get; set; }
	public bool Strikeout { get; set; }

	//public bool DropShadow { get; set; }

	/// <summary>
	/// This should be set by the renderer if it tries to use a font where it's null.
	/// </summary>
	public object? RendererData { get; set; }

	/// <summary>
	/// This is the real font size, after it's been scaled by Renderer.Scale()
	/// </summary>
	public float RealSize { get; set; }

	/// <summary>
	/// Gets the font metrics.
	/// </summary>
	public FontMetrics FontMetrics {
		get {
			fontMetrics ??= renderer.GetFontMetrics(this);
			return (FontMetrics)fontMetrics;
		}
	}

	private readonly Renderer.RendererBase renderer;

	/// <summary>
	/// Initializes a new instance of the <see cref="Font"/> class.
	/// </summary>
	public Font(Renderer.RendererBase renderer)
		: this(renderer, "Arial", 10) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Font"/> class.
	/// </summary>
	/// <param name="renderer">Renderer to use.</param>
	/// <param name="faceName">Face name.</param>
	/// <param name="size">Font size.</param>
	public Font(Renderer.RendererBase renderer, string faceName, int size = 10) {
		this.renderer = renderer;
		fontMetrics = null;
		FaceName = faceName;
		Size = size;
		Smooth = false;
		Bold = false;
		Italic = false;
		Underline = false;
		Strikeout = false;
		//DropShadow = false;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose() {
		renderer.FreeFont(this);
		GC.SuppressFinalize(this);
	}

#if DEBUG
	~Font() {
		throw new InvalidOperationException(String.Format("IDisposable object finalized: {0}", GetType()));
		//Debug.Print(String.Format("IDisposable object finalized: {0}", GetType()));
	}
#endif

	/// <summary>
	/// Duplicates font data (except renderer data which must be reinitialized).
	/// </summary>
	/// <returns></returns>
	public Font Copy() {
		Font f = new Font(renderer, FaceName, Size);
		f.RealSize = RealSize;
		f.RendererData = null; // must be reinitialized
		f.Bold = Bold;
		f.Italic = Italic;
		f.Underline = Underline;
		f.Strikeout = Strikeout;
		//f.DropShadow = DropShadow;

		return f;
	}

	/// <summary>
	/// Create a new font instance. This function uses a font cache to load the font.
	/// This is preferable method to create a font. User don't need to worry about
	/// disposing the font.
	/// </summary>
	/// <param name="renderer">Renderer to use.</param>
	/// <param name="faceName">Face name.</param>
	/// <param name="size">Font size.</param>
	/// <param name="style">Font style.</param>
	/// <returns>Font.</returns>
	public static Font Create(Renderer.RendererBase renderer, string faceName, int size = 10, FontStyle style = 0) {
		return FontCache.GetFont(renderer, faceName, size, style);
	}
}