using System.Collections.Generic;

namespace Gwen.Net.RichText;
public class Paragraph {
	private List<Part> parts = new List<Part>();

	private Margin margin;
	private int firstIndent;
	private int remainingIndent;

	public List<Part> Parts => parts;

	public Margin Margin => margin;
	public int FirstIndent => firstIndent;
	public int RemainigIndent => remainingIndent;

	public Paragraph(Margin margin = new Margin(), int firstIndent = 0, int remainingIndent = 0) {
		this.margin = margin;
		this.firstIndent = firstIndent;
		this.remainingIndent = remainingIndent;
	}

	public Paragraph Text(string text) {
		parts.Add(new TextPart(text));

		return this;
	}

	public Paragraph Text(string text, Color color) {
		parts.Add(new TextPart(text, color));

		return this;
	}

	public Paragraph Link(string text, string link, Color? color = null, Color? hoverColor = null, Font? hoverFont = null) {
		parts.Add(color == null ? new LinkPart(text, link) : new LinkPart(text, link, (Color)color, hoverColor, hoverFont));

		return this;
	}

	public Paragraph Font(Font? font = null) {
		parts.Add(new FontPart(font));

		return this;
	}

	public Paragraph LineBreak() {
		parts.Add(new LineBreakPart());

		return this;
	}
}