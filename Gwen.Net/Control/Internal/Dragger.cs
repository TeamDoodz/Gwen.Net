using System;
using Gwen.Net.Input;

namespace Gwen.Net.Control.Internal;
/// <summary>
/// Base for controls that can be dragged by mouse.
/// </summary>
public class Dragger : ControlBase {
	protected Point holdPos;

	protected internal ControlBase? Target { get; set; }

	/// <summary>
	/// Indicates if the control is being dragged.
	/// </summary>
	public bool IsHeld { get; protected set; }

	/// <summary>
	/// Event invoked when the control position has been changed.
	/// </summary>
	public event GwenEventHandler<EventArgs>? Dragged;

	/// <summary>
	/// Initializes a new instance of the <see cref="Dragger"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public Dragger(ControlBase? parent) : base(parent) {
		MouseInputEnabled = true;
		IsHeld = false;
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {
		if(null == Target) return;

		if(down) {
			IsHeld = true;
			holdPos = Target.CanvasPosToLocal(new Point(x, y));
			InputHandler.MouseFocus = this;
		} else {
			IsHeld = false;

			InputHandler.MouseFocus = null;
		}
	}

	/// <summary>
	/// Handler invoked on mouse moved event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="dx">X change.</param>
	/// <param name="dy">Y change.</param>
	protected override void OnMouseMoved(int x, int y, int dx, int dy) {
		if(null == Target) return;
		if(!IsHeld) return;

		Point p = new Point(x - holdPos.X, y - holdPos.Y);

		// Translate to parent
		if(Target.Parent != null)
			p = Target.Parent.CanvasPosToLocal(p);

		//m_Target->SetPosition( p.x, p.y );
		Target.MoveTo(p.X, p.Y);
		Dragged?.Invoke(this, EventArgs.Empty);
	}

	protected override Size Measure(Size availableSize) {
		return availableSize;
	}

	protected override Size Arrange(Size finalSize) {
		return finalSize;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {

	}
}