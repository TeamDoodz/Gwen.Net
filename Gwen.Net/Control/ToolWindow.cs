using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;
[Xml.XmlControl]
public class ToolWindow : WindowBase {
	private bool vertical;

	[Xml.XmlProperty]
	public bool Vertical {
		get {
			return vertical;
		}
		set {
			vertical = value;
			if(dragBar != null) {
				if(vertical) {
					dragBar.Height = BaseUnit + 2;
					dragBar.Width = Util.Ignore;
				} else {
					dragBar.Width = BaseUnit + 2;
					dragBar.Height = Util.Ignore;
				}
			}
			EnableResizing();
			Invalidate();
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ToolWindow"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ToolWindow(ControlBase? parent)
		: base(parent) {
		dragBar = new Dragger(this);
		dragBar.Target = this;
		dragBar.SendToBack();

		Vertical = false;

		innerPanel = new InnerContentControl(this);
		innerPanel.SendToBack();
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		if(dragBar == null) {
			return;
		}

		skin.DrawToolWindow(this, vertical, vertical ? dragBar.ActualHeight : dragBar.ActualWidth);
	}

	/// <summary>
	/// Renders under the actual control (shadows etc).
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void RenderUnder(Skin.SkinBase skin) {
		base.RenderUnder(skin);
		skin.DrawShadow(this);
	}

	/// <summary>
	/// Renders the focus overlay.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void RenderFocus(Skin.SkinBase skin) {

	}

	protected override Size Measure(Size availableSize) {
		Size titleBarSize = dragBar?.DoMeasure(new Size(availableSize.Width, availableSize.Height)) ?? Size.One;

		if(innerPanel != null) {
			if(vertical)
				innerPanel.DoMeasure(new Size(availableSize.Width, availableSize.Height - titleBarSize.Height));
			else
				innerPanel.DoMeasure(new Size(availableSize.Width - titleBarSize.Width, availableSize.Height));
		}

		if(vertical)
			return base.Measure(new Size(innerPanel?.MeasuredSize.Width ?? 1, innerPanel?.MeasuredSize.Height ?? 1 + titleBarSize.Height));
		else
			return base.Measure(new Size(innerPanel?.MeasuredSize.Width ?? 1 + titleBarSize.Width, innerPanel?.MeasuredSize.Height ?? 1));
	}

	protected override Size Arrange(Size finalSize) {
		if(vertical)
			dragBar?.DoArrange(new Rectangle(0, 0, finalSize.Width, dragBar.MeasuredSize.Height));
		else
			dragBar?.DoArrange(new Rectangle(0, 0, dragBar.MeasuredSize.Width, finalSize.Height));

		if(innerPanel != null) {
			if(vertical)
				innerPanel.DoArrange(new Rectangle(0, dragBar?.MeasuredSize.Height ?? 1, finalSize.Width, finalSize.Height - dragBar?.MeasuredSize.Height ?? 1));
			else
				innerPanel.DoArrange(new Rectangle(dragBar?.MeasuredSize.Width ?? 1, 0, finalSize.Width - dragBar?.MeasuredSize.Width ?? 1, finalSize.Height));
		}

		return base.Arrange(finalSize);
	}

	public override void EnableResizing(bool left = true, bool top = true, bool right = true, bool bottom = true) {
		base.EnableResizing(vertical && left, !vertical && top, right, bottom);
	}
}