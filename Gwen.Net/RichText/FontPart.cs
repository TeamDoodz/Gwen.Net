using System;

namespace Gwen.Net.RichText;

public class FontPart : Part {
	private Font? font;

	public FontPart(Font? font = null) {
		this.font = font;
	}

	public override string[] Split(ref Font? font) {
		font = this.font;
		return Array.Empty<string>();
	}
}