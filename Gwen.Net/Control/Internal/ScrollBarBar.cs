namespace Gwen.Net.Control.Internal;

/// <summary>
/// Scrollbar bar.
/// </summary>
public class ScrollBarBar : Dragger {
	private bool horizontal;

	/// <summary>
	/// Indicates whether the bar is horizontal.
	/// </summary>
	public bool IsHorizontal { get { return horizontal; } set { horizontal = value; } }

	/// <summary>
	/// Indicates whether the bar is vertical.
	/// </summary>
	public bool IsVertical { get { return !horizontal; } set { horizontal = !value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="ScrollBarBar"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ScrollBarBar(ControlBase parent)
		: base(parent) {
		RestrictToParent = true;
		Target = this;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawScrollBarBar(this, IsHeld, IsHovered, horizontal);
		base.Render(skin);
	}

	/// <summary>
	/// Handler invoked on mouse moved event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="dx">X change.</param>
	/// <param name="dy">Y change.</param>
	protected override void OnMouseMoved(int x, int y, int dx, int dy) {
		base.OnMouseMoved(x, y, dx, dy);

		if(Parent is not ScrollBar scrollBar) {
			return;
		}

		int buttonSize = scrollBar.ButtonSize;
		if(horizontal) {
			if(ActualLeft < buttonSize)
				MoveTo(buttonSize, ActualTop);
			else if(ActualLeft > Parent.ActualWidth - ActualWidth - buttonSize)
				MoveTo(Parent.ActualWidth - ActualWidth - buttonSize, ActualTop);
		} else {
			if(ActualTop < buttonSize)
				MoveTo(ActualLeft, buttonSize);
			else if(ActualTop > Parent.ActualHeight - ActualHeight - buttonSize)
				MoveTo(ActualLeft, Parent.ActualHeight - ActualHeight - buttonSize);
		}
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {
		base.OnMouseClickedLeft(x, y, down);
	}
}