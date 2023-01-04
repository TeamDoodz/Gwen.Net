namespace Gwen.Net.Control;

/// <summary>
/// Image container.
/// </summary>
public class ImagePanel : ControlBase {
	private readonly Texture texture;
	private readonly float[] uv;
	private Color drawColor;
	private Size imageSize;

	/// <summary>
	/// Texture name.
	/// </summary>
	public string ImageName {
		get { return texture.Name; }
		set { texture.Load(value); }
	}

	/// <summary>
	/// Gets or sets the size of the image.
	/// </summary>
	public Size ImageSize {
		get { return imageSize; }
		set { if(value == imageSize) return; imageSize = value; Invalidate(); }
	}

	/// <summary>
	/// Gets or sets the texture coordinates of the image in pixels.
	/// </summary>
	public Rectangle TextureRect {
		get {
			if(texture == null)
				return Rectangle.Empty;

			int x1 = (int)(uv[0] * texture.Width);
			int y1 = (int)(uv[1] * texture.Height);
			int x2 = Util.Ceil(uv[2] * texture.Width);
			int y2 = Util.Ceil(uv[3] * texture.Height);
			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}
		set {
			if(texture == null)
				return;

			uv[0] = (float)value.X / (float)texture.Width;
			uv[1] = (float)value.Y / (float)texture.Height;
			uv[2] = uv[0] + (float)value.Width / (float)texture.Width;
			uv[3] = uv[1] + (float)value.Height / (float)texture.Height;
		}
	}

	/// <summary>
	/// Gets or sets the color of the image.
	/// </summary>
	public Color ImageColor {
		get { return drawColor; }
		set { drawColor = value; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ImagePanel"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ImagePanel(ControlBase? parent)
		: base(parent) {
		uv = new float[4];
		texture = new Texture(Skin.Renderer);
		imageSize = Size.Zero;
		SetUV(0, 0, 1, 1);
		MouseInputEnabled = true;
		drawColor = Color.White;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public override void Dispose() {
		texture.Dispose();
		base.Dispose();
	}

	/// <summary>
	/// Sets the texture coordinates of the image in uv-coordinates.
	/// </summary>
	public virtual void SetUV(float u1, float v1, float u2, float v2) {
		uv[0] = u1;
		uv[1] = v1;
		uv[2] = u2;
		uv[3] = v2;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		base.Render(skin);
		skin.Renderer.DrawColor = drawColor;
		skin.Renderer.DrawTexturedRect(texture, RenderBounds, uv[0], uv[1], uv[2], uv[3]);
	}

	/// <summary>
	/// Control has been clicked - invoked by input system. Windows use it to propagate activation.
	/// </summary>
	public override void Touch() {
		base.Touch();
	}

	protected override Size Measure(Size availableSize) {
		if(texture == null)
			return Size.Zero;

		float scale = this.Scale;

		Size size = imageSize;
		if(size.Width == 0) size.Width = texture.Width;
		if(size.Height == 0) size.Height = texture.Height;

		return new Size(Util.Ceil(size.Width * scale), Util.Ceil(size.Height * scale));
	}

	protected override Size Arrange(Size finalSize) {
		return finalSize;
	}

	/// <summary>
	/// Handler for Space keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeySpace(bool down) {
		if(down)
			base.OnMouseClickedLeft(0, 0, true);
		return true;
	}
}