using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Vertical slider.
/// </summary>
[Xml.XmlControl]
public class VerticalSlider : Slider {
	/// <summary>
	/// Initializes a new instance of the <see cref="VerticalSlider"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public VerticalSlider(ControlBase? parent)
		: base(parent) {
		Width = BaseUnit;

		sliderBar.IsHorizontal = false;
	}

	protected override float CalculateValue() {
		return 1 - sliderBar.ActualTop / (float)(ActualHeight - sliderBar.ActualHeight);
	}

	protected override void UpdateBarFromValue() {
		sliderBar.MoveTo((this.ActualWidth - sliderBar.ActualWidth) / 2, (int)((ActualHeight - sliderBar.ActualHeight) * (1 - value)));
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {
		base.OnMouseClickedLeft(x, y, down);
		sliderBar.MoveTo((this.ActualWidth - sliderBar.ActualWidth) / 2, (int)(CanvasPosToLocal(new Point(x, y)).Y - sliderBar.ActualHeight * 0.5));
		sliderBar.InputMouseClickedLeft(x, y, down);
		OnMoved(sliderBar, EventArgs.Empty);
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawSlider(this, false, snapToNotches ? notchCount : 0, sliderBar.ActualHeight);
	}
}