namespace Gwen.Net.RichText;

internal class TextBlock {
	public string Text { get; set; } = "";
	public Point Position { get; set; }
	public Size Size { get; set; }
	public Part? Part { get; set; }
}