using System;
using Gwen.Net.Control.Internal;
using Gwen.Net.Input;

namespace Gwen.Net.Control;

/// <summary>
/// Image alignment inside the button
/// </summary>
[Flags]
public enum ImageAlign {
	Left = 1 << 0,
	Right = 1 << 1,
	Top = 1 << 2,
	Bottom = 1 << 3,
	CenterV = 1 << 4,
	CenterH = 1 << 5,
	Fill = 1 << 6,
	LeftSide = 1 << 7,
	Above = 1 << 8,
	Center = CenterV | CenterH,
}

/// <summary>
/// Button control.
/// </summary>
[Xml.XmlControl]
public class Button : ButtonBase {
	private Alignment align;
	private Padding textPadding;
	private Text text;
	private ImageAlign imageAlign;
	private ImagePanel? image;

	/// <inheritdoc cref="Text.Content"/>
	[Xml.XmlProperty]
	public virtual string Text {
		get => text.Content;
		set { 
			EnsureText(); 
			text.Content = value; 
		}
	}

	/// <inheritdoc cref="Text.Font"/>
	[Xml.XmlProperty]
	public Font? Font {
		get => text.Font;
		set { 
			EnsureText(); 
			text.Font = value; 
		} 
	}

	/// <inheritdoc cref="Text.TextColor"/>
	[Xml.XmlProperty]
	public Color TextColor {
		get => text.TextColor;
		set { 
			EnsureText(); 
			text.TextColor = value; 
		} 
	}

	/// <inheritdoc cref="Text.TextColorOverride"/>
	[Xml.XmlProperty]
	public Color TextColorOverride {
		get => text.TextColorOverride;
		set { 
			EnsureText(); 
			text.TextColorOverride = value; 
		} 
	}

	/// <summary>
	/// The padding space around the text.
	/// </summary>
	[Xml.XmlProperty]
	public Padding TextPadding {
		get => textPadding;
		set { 
			if(value == textPadding) return; 
			textPadding = value; 
			Invalidate(); 
		} 
	}

	/// <summary>
	/// Alignment to use for the text.
	/// </summary>
	[Xml.XmlProperty]
	public Alignment Alignment { 
		get => align;
		set { 
			if(value == align) return; 
			align = value; 
			Invalidate(); 
		} 
	}

	/// <summary>
	/// Determines how the image is aligned inside the button.
	/// </summary>
	[Xml.XmlProperty]
	public ImageAlign ImageAlign {
		get => imageAlign;
		set { 
			if(imageAlign == value) return; 
			imageAlign = value; 
			Invalidate(); 
		} 
	}

	/// <summary>
	/// Returns the current image name (or null if no image set).
	/// </summary>
	[Xml.XmlProperty]
	public string? ImageName => image?.ImageName;

	/// <summary>
	/// The size of the image.
	/// </summary>
	[Xml.XmlProperty]
	public Size ImageSize {
		get => image?.ImageSize ?? Size.Zero;
		set {
			if(image == null) {
				return;
			}

			image.ImageSize = value;
		}
	}

	/// <inheritdoc cref="ImagePanel.TextureRect"/>
	[Xml.XmlProperty]
	public Rectangle ImageTextureRect {
		get => image?.TextureRect ?? Rectangle.Empty;
		set {
			if(image == null) {
				return;
			}

			image.TextureRect = value;
		}
	}

	/// <inheritdoc cref="ImagePanel.ImageColor"/>
	[Xml.XmlProperty]
	public Color ImageColor {
		get => image?.ImageColor ?? Color.White;
		set {
			if(image == null) {
				return;
			}

			image.ImageColor = value;
		}
	}

	/// <summary>
	/// Control constructor.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public Button(ControlBase? parent)
		: base(parent) {
		Alignment = Alignment.Center;
		TextPadding = new Padding(3, 3, 3, 3);
		text = new Text(this);
	}

	private void EnsureText() {
		text ??= new Text(this);
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		base.Render(skin);

		if(ShouldDrawBackground) {
			bool drawDepressed = IsDepressed && IsHovered;
			if(IsToggle)
				drawDepressed = drawDepressed || ToggleState;

			bool bDrawHovered = IsHovered && ShouldDrawHover;

			skin.DrawButton(this, drawDepressed, bDrawHovered, IsDisabled);
		}
	}

	/// <summary>
	/// Sets the button's image.
	/// </summary>
	/// <param name="textureName">Texture name. Null to remove.</param>
	/// <param name="imageAlign">Determines how the image should be aligned.</param>
	public virtual void SetImage(string textureName, ImageAlign imageAlign = ImageAlign.LeftSide) {
		if(String.IsNullOrEmpty(textureName)) {
			image?.Dispose();
			image = null;
			return;
		}

		image ??= new ImagePanel(this);

		image.ImageName = textureName;
		image.MouseInputEnabled = false;
		this.imageAlign = imageAlign;
		image.SendToBack();

		Invalidate();
	}

	protected override Size Measure(Size availableSize) {
		if(image == null) {
			Size size = Size.Zero;
			if(text != null)
				size = text.DoMeasure(availableSize);

			size += textPadding + Padding;

			return size;
		} else {
			Size imageSize = image.DoMeasure(availableSize);
			Size textSize = text != null ? text.DoMeasure(availableSize) + textPadding : Size.Zero;

			Size totalSize;
			switch(imageAlign) {
				case ImageAlign.LeftSide:
					totalSize = new Size(textSize.Width + imageSize.Width, Math.Max(imageSize.Height, textSize.Height));
					break;
				case ImageAlign.Above:
					totalSize = new Size(Math.Max(imageSize.Width, textSize.Width), textSize.Height + imageSize.Height);
					break;
				default:
					totalSize = Size.Max(imageSize, textSize);
					break;
			}

			totalSize += Padding;

			return totalSize;
		}
	}

	protected override Size Arrange(Size finalSize) {
		if(image == null) {
			if(text != null) {
				Size innerSize = finalSize - Padding;
				Size textSize = text.MeasuredSize + textPadding;
				Rectangle rect = new Rectangle(Point.Zero, textSize);

				if((align & Alignment.CenterH) != 0)
					rect.X = (innerSize.Width - rect.Width) / 2;
				else if((align & Alignment.Right) != 0)
					rect.X = innerSize.Width - rect.Width;

				if((align & Alignment.CenterV) != 0)
					rect.Y = (innerSize.Height - rect.Height) / 2;
				else if((align & Alignment.Bottom) != 0)
					rect.Y = innerSize.Height - rect.Height;

				rect.Offset(textPadding + Padding);

				text.DoArrange(rect);
			}
		} else {
			Size innerSize = finalSize - Padding;

			Size imageSize = image.MeasuredSize;
			Size textSize = text != null ? text.MeasuredSize + textPadding : Size.Zero;

			Rectangle rect;
			switch(imageAlign) {
				case ImageAlign.LeftSide:
					rect = new Rectangle(Point.Zero, textSize.Width + imageSize.Width, Math.Max(imageSize.Height, textSize.Height));
					break;
				case ImageAlign.Above:
					rect = new Rectangle(Point.Zero, Math.Max(imageSize.Width, textSize.Width), textSize.Height + imageSize.Height);
					break;
				default:
					rect = new Rectangle(Point.Zero, textSize);
					break;
			}

			if((align & Alignment.Right) != 0)
				rect.X = innerSize.Width - rect.Width;
			else if((align & Alignment.CenterH) != 0)
				rect.X = (innerSize.Width - rect.Width) / 2;
			if((align & Alignment.Bottom) != 0)
				rect.Y = innerSize.Height - rect.Height;
			else if((align & Alignment.CenterV) != 0)
				rect.Y = (innerSize.Height - rect.Height) / 2;

			Rectangle imageRect = new Rectangle(Point.Zero, imageSize);
			Rectangle textRect = new Rectangle(rect.Location, text != null ? text.MeasuredSize : Size.Zero);

			switch(imageAlign) {
				case ImageAlign.LeftSide:
					imageRect.Location = new Point(rect.X, rect.Y + (rect.Height - imageSize.Height) / 2);
					textRect.Location = new Point(rect.X + imageSize.Width, rect.Y + (rect.Height - textSize.Height) / 2);
					break;
				case ImageAlign.Above:
					imageRect.Location = new Point(rect.X + (rect.Width - imageSize.Width) / 2, rect.Y);
					textRect.Location = new Point(rect.X + (rect.Width - textSize.Width) / 2, rect.Y + imageSize.Height);
					break;
				case ImageAlign.Fill:
					imageRect.Size = innerSize;
					break;
				default:
					if((imageAlign & ImageAlign.Right) != 0)
						imageRect.X = innerSize.Width - imageRect.Width;
					else if((imageAlign & ImageAlign.CenterH) != 0)
						imageRect.X = (innerSize.Width - imageRect.Width) / 2;
					if((imageAlign & ImageAlign.Bottom) != 0)
						imageRect.Y = innerSize.Height - imageRect.Height;
					else if((imageAlign & ImageAlign.CenterV) != 0)
						imageRect.Y = (innerSize.Height - imageRect.Height) / 2;
					break;
			}

			imageRect.Offset(Padding);
			image.DoArrange(imageRect);

			if(text != null) {
				textRect.Offset(Padding + textPadding);
				text.DoArrange(textRect);
			}
		}

		return finalSize;
	}

	/// <summary>
	/// Updates control colors.
	/// </summary>
	public override void UpdateColors() {
		if(text == null)
			return;

		if(IsDisabled) {
			TextColor = Skin.Colors.Button.Disabled;
			return;
		}

		if(IsDepressed || ToggleState) {
			TextColor = Skin.Colors.Button.Down;
			return;
		}

		if(IsHovered) {
			TextColor = Skin.Colors.Button.Hover;
			return;
		}

		TextColor = Skin.Colors.Button.Normal;
	}
}