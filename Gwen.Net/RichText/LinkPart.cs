namespace Gwen.Net.RichText;

public class LinkPart : TextPart {
	private readonly string link = "https://www.example.com";
	private readonly Color? hoverColor = null;
	private readonly Font? hoverFont = null;

	public string Link { get { return link; } }
	public Color? HoverColor { get { return hoverColor; } }
	public Font? HoverFont { get { return hoverFont; } }

	public LinkPart(string text, string link)
		: base(text) {
		this.link = link;
	}

	public LinkPart(string text, string link, Color color, Color? hoverColor = null, Font? hoverFont = null)
		: base(text, color) {
		this.link = link;

		if(hoverColor != null)
			this.hoverColor = hoverColor;
		if(hoverFont != null)
			this.hoverFont = hoverFont;
	}

	public override string[] Split(ref Font? font) {
		this.Font = font;

		return new string[] { Text.Trim() };
	}
}