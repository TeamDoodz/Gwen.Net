namespace Gwen.Net.RichText;

public class ImageParagraph : Paragraph {
	private string? imageName;
	private Size? imageSize;
	private Rectangle? textureRect;
	private Color? imageColor;

	public string? ImageName => imageName;
	public Size? ImageSize => imageSize;
	public Rectangle? TextureRect => textureRect;
	public Color? ImageColor => imageColor;

	public ImageParagraph(Margin margin = new Margin(), int indent = 0)
		: base(margin, indent, indent) {
	}

	public ImageParagraph Image(string imageName, Size? imageSize = null, Rectangle? textureRect = null, Color? imageColor = null) {
		this.imageName = imageName;
		if(imageSize != null)
			this.imageSize = imageSize;
		if(textureRect != null)
			this.textureRect = textureRect;
		if(imageColor != null)
			this.imageColor = imageColor;

		return this;
	}
}