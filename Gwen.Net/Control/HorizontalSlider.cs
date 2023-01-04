using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Horizontal slider.
/// </summary>
[Xml.XmlControl]
public class HorizontalSlider : Slider {
	/// <summary>
	/// Initializes a new instance of the <see cref="HorizontalSlider"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public HorizontalSlider(ControlBase? parent)
		: base(parent) {
		Height = BaseUnit;

		sliderBar.IsHorizontal = true;
	}

	protected override float CalculateValue() {
		return (float)sliderBar.ActualLeft / (ActualWidth - sliderBar.ActualWidth);
	}

	protected override void UpdateBarFromValue() {
		sliderBar.MoveTo((int)((ActualWidth - sliderBar.ActualWidth) * (value)), (this.ActualHeight - sliderBar.ActualHeight) / 2);
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {
		base.OnMouseClickedLeft(x, y, down);
		sliderBar.MoveTo((int)(CanvasPosToLocal(new Point(x, y)).X - sliderBar.ActualWidth / 2), (this.ActualHeight - sliderBar.ActualHeight) / 2);
		sliderBar.InputMouseClickedLeft(x, y, down);
		OnMoved(sliderBar, EventArgs.Empty);
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawSlider(this, true, snapToNotches ? notchCount : 0, sliderBar.ActualWidth);
	}
}