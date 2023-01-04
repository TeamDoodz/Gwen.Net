//#define DEBUG_TEXT_MEASURE

using System;

namespace Gwen.Net.Control.Internal;

/// <summary>
/// Displays text. Always sized to contents.
/// </summary>
public class Text : ControlBase {
	private string content;
	private Font? font;

	private string fitToText;

	/// <summary>
	/// Font used to display the text.
	/// </summary>
	/// <remarks>
	/// The font is not being disposed by this class.
	/// </remarks>
	public Font? Font { get { return font; } set { font = value; Invalidate(); } }

	/// <summary>
	/// Text to display.
	/// </summary>
	public string Content {
		get { return content; }
		set {
			if(value == content)
				return;

			content = value;
			if(AutoSizeToContents)
				Invalidate();
		}
	}

	/// <summary>
	/// The color to display the text in.
	/// </summary>
	public Color TextColor { get; set; }

	/// <summary>
	/// Determines whether the control should be automatically resized to fit the text.
	/// </summary>
	public bool AutoSizeToContents { get; set; }

	/// <summary>
	/// Text length in characters.
	/// </summary>
	public int Length { get { return Content.Length; } }

	/// <summary>
	/// Text color override - used by tooltips.
	/// </summary>
	public Color TextColorOverride { get; set; }

	/// <summary>
	/// Text override - used to display different string.
	/// </summary>
	public string? TextOverride { get; set; }

	/// <summary>
	/// Set the minimum size of the control to be able to show the text of this property.
	/// </summary>
	public string FitToText { get { return fitToText; } set { if(fitToText == value) return; fitToText = value; Invalidate(); } }

	/// <summary>
	/// Initializes a new instance of the <see cref="Text"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public Text(ControlBase? parent)
		: base(parent) {
		AutoSizeToContents = true;
		font = Skin.DefaultFont;
		content = String.Empty;
		fitToText = "";
		TextColor = Skin.Colors.Label.Default;
		MouseInputEnabled = false;
		TextColorOverride = new Color(0, 255, 255, 255); // A==0, override disabled
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		if(Length == 0 || Font == null) return;

		if(TextColorOverride.A == 0)
			skin.Renderer.DrawColor = TextColor;
		else
			skin.Renderer.DrawColor = TextColorOverride;

		skin.Renderer.RenderText(Font, Point.Zero, TextOverride ?? Content);

#if DEBUG_TEXT_MEASURE
			{
				Point lastPos = Point.Zero;

				for (int i = 0; i < m_String.Length + 1; i++)
				{
					String sub = (TextOverride ?? String).Substring(0, i);
					Point p = Skin.Renderer.MeasureText(Font, sub);

					Rectangle rect = new Rectangle();
					rect.Location = lastPos;
					rect.Size = new Size(p.X - lastPos.X, p.Y);
					skin.Renderer.DrawColor = Color.FromArgb(64, 0, 0, 0);
					skin.Renderer.DrawLinedRect(rect);

					lastPos = new Point(rect.Right, 0);
				}
			}
#endif
	}

	protected override Size Measure(Size availableSize) {
		if(Content == null)
			return Size.Zero;

		if(Font == null)
			throw new InvalidOperationException("Text control font not set.");

		Size size = Size.Zero;

		string text = TextOverride ?? Content;

		if(AutoSizeToContents && text.Length == 0)
			size = Skin.Renderer.MeasureText(Font, " ");
		else if(!AutoSizeToContents && !String.IsNullOrWhiteSpace(fitToText))
			size = Skin.Renderer.MeasureText(Font, fitToText);
		else
			size = Skin.Renderer.MeasureText(Font, text);

		return size;
	}

	protected override Size Arrange(Size finalSize) {
		return MeasuredSize;
	}

	/// <summary>
	/// Gets the coordinates of specified character in the text.
	/// </summary>
	/// <param name="index">Character index.</param>
	/// <returns>Character position in local coordinates.</returns>
	public Point GetCharacterPosition(int index) {
		if(Length == 0 || index == 0) {
			return new Point(0, 0);
		}

		string sub = (TextOverride ?? Content).Substring(0, index);
		Size s = Font == null ? Size.One : Skin.Renderer.MeasureText(Font, sub);

		return new Point(s.Width, 0);
	}

	/// <summary>
	/// Searches for a character closest to given point.
	/// </summary>
	/// <param name="p">Point.</param>
	/// <returns>Character index.</returns>
	public int GetClosestCharacter(Point p) {
		int px = p.X;
		int left = 0;
		int right = Content.Length;

		int center;
		int cx;
		while(true) {
			center = (right + left) / 2;
			cx = GetCharacterPosition(center).X;

			if(px < cx) {
				right = center;
			} else if(px > cx) {
				left = center;
			} else {
				left = center;
				right = center;
				break;
			}

			if(right - left < 2) {
				break;
			}
		}

		int lx = cx, rx = cx;
		if(left == center)
			rx = GetCharacterPosition(right).X;
		else if(right == center)
			lx = GetCharacterPosition(left).X;

		if(px - lx < rx - px)
			center = left;
		else
			center = right;

		return center;
	}
}