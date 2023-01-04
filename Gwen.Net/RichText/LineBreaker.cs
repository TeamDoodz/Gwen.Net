using System.Collections.Generic;
using Gwen.Net.Renderer;

namespace Gwen.Net.RichText;

internal abstract class LineBreaker {
	private readonly RendererBase renderer;
	private readonly Font defaultFont;

	public RendererBase Renderer => renderer;
	public Font DefaultFont => defaultFont;

	public LineBreaker(RendererBase renderer, Font defaultFont) {
		this.renderer = renderer;
		this.defaultFont = defaultFont;
	}

	public abstract List<TextBlock> LineBreak(Paragraph paragraph, int width);
}