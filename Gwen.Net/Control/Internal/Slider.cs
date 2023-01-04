using System;
using Gwen.Net.Input;

namespace Gwen.Net.Control.Internal;

/// <summary>
/// Base slider.
/// </summary>
public class Slider : ControlBase {
	protected readonly SliderBar sliderBar;
	protected bool snapToNotches;
	protected int notchCount;
	protected float value;
	protected float min;
	protected float max;

	/// <summary>
	/// Number of notches on the slider axis.
	/// </summary>
	[Xml.XmlProperty]
	public int NotchCount { get { return notchCount; } set { notchCount = value; } }

	/// <summary>
	/// Determines whether the slider should snap to notches.
	/// </summary>
	[Xml.XmlProperty]
	public bool SnapToNotches { get { return snapToNotches; } set { snapToNotches = value; } }

	/// <summary>
	/// Minimum value.
	/// </summary>
	[Xml.XmlProperty]
	public float Min { get { return min; } set { SetRange(value, max); } }

	/// <summary>
	/// Maximum value.
	/// </summary>
	[Xml.XmlProperty]
	public float Max { get { return max; } set { SetRange(min, value); } }

	/// <summary>
	/// Current value.
	/// </summary>
	[Xml.XmlProperty]
	public float Value {
		get { return min + (value * (max - min)); }
		set {
			if(value < min) value = min;
			if(value > max) value = max;
			// Normalize Value
			value = (value - min) / (max - min);
			SetValueInternal(value);
			Redraw();
		}
	}

	/// <summary>
	/// Invoked when the value has been changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? ValueChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="Slider"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	protected Slider(ControlBase? parent)
		: base(parent) {
		sliderBar = new SliderBar(this);
		sliderBar.Dragged += OnMoved;

		min = 0.0f;
		max = 1.0f;

		snapToNotches = false;
		notchCount = 5;
		value = 0.0f;

		KeyboardInputEnabled = true;
		IsTabable = true;
	}

	/// <summary>
	/// Handler for Right Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyRight(bool down) {
		if(down)
			Value = Value + 1;
		return true;
	}

	/// <summary>
	/// Handler for Up Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyUp(bool down) {
		if(down)
			Value = Value + 1;
		return true;
	}

	/// <summary>
	/// Handler for Left Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyLeft(bool down) {
		if(down)
			Value = Value - 1;
		return true;
	}

	/// <summary>
	/// Handler for Down Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyDown(bool down) {
		if(down)
			Value = Value - 1;
		return true;
	}

	/// <summary>
	/// Handler for Home keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyHome(bool down) {
		if(down)
			Value = min;
		return true;
	}

	/// <summary>
	/// Handler for End keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyEnd(bool down) {
		if(down)
			Value = max;
		return true;
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {

	}

	protected virtual void OnMoved(ControlBase control, EventArgs args) {
		SetValueInternal(CalculateValue());
	}

	protected virtual float CalculateValue() {
		return 0;
	}

	protected virtual void UpdateBarFromValue() {

	}

	protected virtual void SetValueInternal(float val) {
		if(snapToNotches) {
			val = (float)Math.Floor((val * notchCount) + 0.5f);
			val /= notchCount;
		}

		if(value != val) {
			value = val;
			if(ValueChanged != null)
				ValueChanged.Invoke(this, EventArgs.Empty);
		}

		UpdateBarFromValue();
	}

	/// <summary>
	/// Sets the value range.
	/// </summary>
	/// <param name="min">Minimum value.</param>
	/// <param name="max">Maximum value.</param>
	public void SetRange(float min, float max) {
		this.min = min;
		this.max = max;
	}

	/// <summary>
	/// Renders the focus overlay.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void RenderFocus(Skin.SkinBase skin) {
		if(InputHandler.KeyboardFocus != this) return;
		if(!IsTabable) return;

		skin.DrawKeyboardHighlight(this, RenderBounds, 0);
	}

	protected override Size Measure(Size availableSize) {
		sliderBar.DoMeasure(availableSize);

		return sliderBar.MeasuredSize;
	}

	protected override Size Arrange(Size finalSize) {
		sliderBar.DoArrange(new Rectangle(Point.Zero, sliderBar.MeasuredSize));

		UpdateBarFromValue();

		return finalSize;
	}

	protected override void OnBoundsChanged(Rectangle oldBounds) {
		base.OnBoundsChanged(oldBounds);

		// We need to know if bounds are changed to update the bar.
		// In Arrange() we don't know yet new bounds.
		UpdateBarFromValue();
	}
}