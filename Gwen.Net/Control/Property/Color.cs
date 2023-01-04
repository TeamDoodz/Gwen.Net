using System;
using Gwen.Net.Control.Internal;
using Gwen.Net.Input;

namespace Gwen.Net.Control.Property;

/// <summary>
/// Color property.
/// </summary>
public class ColorProperty : Text {
	protected readonly ColorButton button;

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorProperty"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ColorProperty(Control.ControlBase parent) : base(parent) {
		button = new ColorButton(textBox);
		button.Dock = Dock.Right;
		button.Width = 20;
		button.Margin = new Margin(1, 1, 1, 2);
		button.Clicked += OnButtonPressed;
	}

	/// <inheritdoc/>
	protected virtual void OnButtonPressed(Control.ControlBase control, EventArgs args) {
		Canvas canvas = GetCanvas();

		canvas.CloseMenus();

		Popup popup = new(canvas);
		popup.DeleteOnClose = true;
		popup.IsHidden = false;
		popup.BringToFront();

		HSVColorPicker picker = new HSVColorPicker(popup);
		picker.SetColor(GetColorFromText(), false, true);
		picker.ColorChanged += OnColorChanged;

		Point p = button.LocalPosToCanvas(Point.Zero);

		popup.DoMeasure(canvas.ActualSize);
		popup.DoArrange(new Rectangle(p.X + button.ActualWidth - popup.MeasuredSize.Width, p.Y + ActualHeight, popup.MeasuredSize.Width, popup.MeasuredSize.Height));

		popup.Open(new Point(p.X + button.ActualWidth - popup.MeasuredSize.Width, p.Y + ActualHeight));
	}

	/// <summary>
	/// Color changed handler.
	/// </summary>
	/// <param name="control">Event source.</param>
	/// <param name="args">Event args.</param>
	protected virtual void OnColorChanged(Control.ControlBase control, EventArgs args) {
		if (control is HSVColorPicker picker) {
			SetTextFromColor(picker.SelectedColorRGB);
		}
		DoChanged();
	}

	/// <inheritdoc/>
	public override string Value {
		get => textBox.Text;
		set { base.Value = value; }
	}

	/// <summary>
	/// Sets the property value.
	/// </summary>
	/// <param name="value">Value to set.</param>
	/// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
	public override void SetValue(string value, bool fireEvents = false) {
		textBox.SetText(value, fireEvents);
	}

	/// <summary>
	/// Indicates whether the property value is being edited.
	/// </summary>
	public override bool IsEditing => InputHandler.KeyboardFocus == textBox;

	private void SetTextFromColor(Net.Color color) {
		textBox.Text = String.Format("{0} {1} {2}", color.R, color.G, color.B);
	}

	private Net.Color GetColorFromText() {
		string[] split = textBox.Text.Split(' ');

		byte red = 0;
		byte green = 0;
		byte blue = 0;
		byte alpha = 255;

		if (split.Length > 0 && split[0].Length > 0) {
			Byte.TryParse(split[0], out red);
		}

		if (split.Length > 1 && split[1].Length > 0) {
			Byte.TryParse(split[1], out green);
		}

		if (split.Length > 2 && split[2].Length > 0) {
			Byte.TryParse(split[2], out blue);
		}

		return new Gwen.Net.Color(alpha, red, green, blue);
	}

	protected override void DoChanged() {
		base.DoChanged();
		button.Color = GetColorFromText();
	}
}
