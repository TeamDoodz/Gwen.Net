using System;
using Gwen.Net.Control.Internal;
using Gwen.Net.Control.Layout;

namespace Gwen.Net.Control;

/// <summary>
/// Numeric up/down.
/// </summary>
[Xml.XmlControl]
public class NumericUpDown : TextBoxNumeric {
	private float max;
	private float min;
	private float step;

	private readonly Splitter splitter;
	private readonly UpDownButton_Up up;
	private readonly UpDownButton_Down down;

	/// <summary>
	/// Minimum value.
	/// </summary>
	[Xml.XmlProperty]
	public float Min { get { return min; } set { min = value; } }

	/// <summary>
	/// Maximum value.
	/// </summary>
	[Xml.XmlProperty]
	public float Max { get { return max; } set { max = value; } }

	[Xml.XmlProperty]
	public float Step { get { return step; } set { step = value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="NumericUpDown"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public NumericUpDown(ControlBase? parent)
		: base(parent) {
		splitter = new Splitter(this);
		splitter.Dock = Dock.Right;

		up = new UpDownButton_Up(splitter);
		up.Clicked += OnButtonUp;
		up.IsTabable = false;
		splitter.SetPanel(0, up, false);

		down = new UpDownButton_Down(splitter);
		down.Clicked += OnButtonDown;
		down.IsTabable = false;
		down.Padding = new Padding(0, 1, 1, 0);
		splitter.SetPanel(1, down, false);

		max = 100f;
		min = 0f;
		value = 0f;
		step = 1f;

		Text = "0";
	}

	/// <summary>
	/// Invoked when the value has been changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? ValueChanged;

	/// <summary>
	/// Handler for Up Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyUp(bool down) {
		if(down) OnButtonUp(null!, EventArgs.Empty); // Method doesn't read from the control argument so we should be fine
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
		if(down) OnButtonDown(null!, new ClickedEventArgs(0, 0, true)); // Method doesn't read from the control argument so we should be fine
		return true;
	}

	/// <summary>
	/// Handler for the button up event.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnButtonUp(ControlBase control, EventArgs args) {
		Value = value + step;
	}

	/// <summary>
	/// Handler for the button down event.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnButtonDown(ControlBase control, ClickedEventArgs args) {
		Value = value - step;
	}

	/// <summary>
	/// Determines whether the text can be assighed to the control.
	/// </summary>
	/// <param name="str">Text to evaluate.</param>
	/// <returns>True if the text is allowed.</returns>
	protected override bool IsTextAllowed(string str) {
		float d;
		if(!float.TryParse(str, out d))
			return false;
		if(d < min) return false;
		if(d > max) return false;
		return true;
	}

	/// <summary>
	/// Numeric value of the control.
	/// </summary>
	[Xml.XmlProperty]
	public override float Value {
		get {
			return base.Value;
		}
		set {
			if(value < min) value = min;
			if(value > max) value = max;
			if(value == base.value) return;

			base.Value = value;
		}
	}

	/// <summary>
	/// Handler for the text changed event.
	/// </summary>
	protected override void OnTextChanged() {
		base.OnTextChanged();
		if(ValueChanged != null)
			ValueChanged.Invoke(this, EventArgs.Empty);
	}

	public override void SetValue(float value, bool doEvents = true) {
		if(value < min) value = min;
		if(value > max) value = max;
		if(value == base.value) return;

		base.SetValue(value, doEvents);
	}
}