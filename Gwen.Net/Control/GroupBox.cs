using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Group box (container).
/// </summary>
[Xml.XmlControl]
public class GroupBox : ContentControl {
	private readonly Text text;

	/// <summary>
	/// Text.
	/// </summary>
	[Xml.XmlProperty]
	public virtual string Text { get { return text.Content; } set { text.Content = value; } }

	[Xml.XmlProperty]
	public override Padding Padding { 
		get => innerPanel?.Padding ?? Padding.Zero;
		set {
			if(innerPanel != null) {
				innerPanel.Padding = value;
			}
		} 
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GroupBox"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public GroupBox(ControlBase? parent)
		: base(parent) {
		text = new Text(this);

		innerPanel = new InnerContentControl(this);
	}

	protected override Size Measure(Size availableSize) {
		Size titleSize = text.DoMeasure(availableSize);

		Size innerSize = Size.Zero;
		if(innerPanel != null)
			innerSize = innerPanel.DoMeasure(new Size(availableSize.Width - 5 - 5, availableSize.Height - titleSize.Height - 5));

		return new Size(Math.Max(10 + titleSize.Width + 10, 5 + innerSize.Width + 5), titleSize.Height + innerSize.Height + 5);
	}

	protected override Size Arrange(Size finalSize) {
		Size size = finalSize;

		text.DoArrange(new Rectangle(10, 0, text.MeasuredSize.Width, text.MeasuredSize.Height));

		if(innerPanel != null)
			innerPanel.DoArrange(new Rectangle(5, text.MeasuredSize.Height, finalSize.Width - 5 - 5, finalSize.Height - text.MeasuredSize.Height - 5));

		return size;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawGroupBox(this, 10, text.ActualHeight, text.ActualWidth);
	}
}