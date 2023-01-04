using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Static text label.
/// </summary>
[Xml.XmlControl]
public class Label : ControlBase {
	protected readonly Text text;
	private Alignment align;
	private Padding textPadding;
	private bool autoSizeToContent;

	/// <summary>
	/// Text alignment.
	/// </summary>
	[Xml.XmlProperty]
	public Alignment Alignment { get { return align; } set { align = value; Invalidate(); } }

	/// <summary>
	/// Text.
	/// </summary>
	[Xml.XmlProperty]
	public virtual string Text { get { return text.Content; } set { text.Content = value; } }

	/// <summary>
	/// Font.
	/// </summary>
	[Xml.XmlProperty]
	public Font? Font { get { return text.Font; } set { text.Font = value; Invalidate(); } }

	/// <summary>
	/// Text color.
	/// </summary>
	[Xml.XmlProperty]
	public Color TextColor { get { return text.TextColor; } set { text.TextColor = value; } }

	/// <summary>
	/// Override text color (used by tooltips).
	/// </summary>
	[Xml.XmlProperty]
	public Color TextColorOverride { get { return text.TextColorOverride; } set { text.TextColorOverride = value; } }

	/// <summary>
	/// Text override - used to display different string.
	/// </summary>
	[Xml.XmlProperty]
	public string? TextOverride { get { return text.TextOverride; } set { text.TextOverride = value; } }

	/// <summary>
	/// Determines if the control should autosize to its text.
	/// </summary>
	[Xml.XmlProperty]
	public bool AutoSizeToContents { get { return autoSizeToContent; } set { autoSizeToContent = value; IsVirtualControl = !value; if(value) Invalidate(); } }

	/// <summary>
	/// Text padding.
	/// </summary>
	[Xml.XmlProperty]
	public Padding TextPadding { get { return textPadding; } set { textPadding = value; Invalidate(); } }

	[Xml.XmlEvent]
	public override event ControlBase.GwenEventHandler<ClickedEventArgs>? Clicked {
		add {
			base.Clicked += value;
			MouseInputEnabled = ClickEventAssigned;
		}
		remove {
			base.Clicked -= value;
			MouseInputEnabled = ClickEventAssigned;
		}
	}

	[Xml.XmlEvent]
	public override event ControlBase.GwenEventHandler<ClickedEventArgs>? DoubleClicked {
		add {
			base.DoubleClicked += value;
			MouseInputEnabled = ClickEventAssigned;
		}
		remove {
			base.DoubleClicked -= value;
			MouseInputEnabled = ClickEventAssigned;
		}
	}

	[Xml.XmlEvent]
	public override event ControlBase.GwenEventHandler<ClickedEventArgs>? RightClicked {
		add {
			base.RightClicked += value;
			MouseInputEnabled = ClickEventAssigned;
		}
		remove {
			base.RightClicked -= value;
			MouseInputEnabled = ClickEventAssigned;
		}
	}

	[Xml.XmlEvent]
	public override event ControlBase.GwenEventHandler<ClickedEventArgs>? DoubleRightClicked {
		add {
			base.DoubleRightClicked += value;
			MouseInputEnabled = ClickEventAssigned;
		}
		remove {
			base.DoubleRightClicked -= value;
			MouseInputEnabled = ClickEventAssigned;
		}
	}


	/// <summary>
	/// Initializes a new instance of the <see cref="Label"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public Label(ControlBase? parent) : base(parent) {
		text = new Text(this);
		//m_Text.Font = Skin.DefaultFont;

		autoSizeToContent = true;

		MouseInputEnabled = false;
		Alignment = Alignment.Left | Alignment.Top;
	}

	/// <summary>
	/// Returns index of the character closest to specified point (in canvas coordinates).
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	protected virtual Point GetClosestCharacter(int x, int y) {
		return new Point(text.GetClosestCharacter(text.CanvasPosToLocal(new Point(x, y))), 0);
	}

	/// <summary>
	/// Handler for text changed event.
	/// </summary>
	protected virtual void OnTextChanged() { }

	protected override Size Measure(Size availableSize) {
		return text.DoMeasure(availableSize) + textPadding + Padding;
	}

	protected override Size Arrange(Size finalSize) {
		Size innerSize = finalSize - textPadding - Padding;
		Rectangle rect = new Rectangle(Point.Zero, Size.Min(text.MeasuredSize, innerSize));

		if((align & Alignment.CenterH) != 0)
			rect.X = (innerSize.Width - text.MeasuredSize.Width) / 2;
		else if((align & Alignment.Right) != 0)
			rect.X = innerSize.Width - text.MeasuredSize.Width;

		if((align & Alignment.CenterV) != 0)
			rect.Y = (innerSize.Height - text.MeasuredSize.Height) / 2;
		else if((align & Alignment.Bottom) != 0)
			rect.Y = innerSize.Height - text.MeasuredSize.Height;

		rect.Offset(textPadding + Padding);

		text.DoArrange(rect);

		return finalSize;
	}

	/// <summary>
	/// Gets the coordinates of specified character.
	/// </summary>
	/// <param name="index">Character index.</param>
	/// <returns>Character coordinates (local).</returns>
	public virtual Point GetCharacterPosition(int index) {
		Point p = text.GetCharacterPosition(index);
		return new Point(p.X + text.ActualLeft, p.Y + text.ActualTop);
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
	}
}