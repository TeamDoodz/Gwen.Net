using System;
using Gwen.Net.Control.Internal;
using Gwen.Net.Control.Layout;

namespace Gwen.Net.Control;

/// <summary>
/// HSV color picker with "before" and "after" color boxes.
/// </summary>
[Xml.XmlControl(ElementName = nameof(HSVColorPicker))]
public class HSVColorPicker : ControlBase, IColorPicker {
	private readonly ColorLerpBox lerpBox;
	private readonly ColorSlider colorSlider;
	private readonly ColorDisplay before;
	private readonly ColorDisplay after;
	private readonly NumericUpDown red;
	private readonly NumericUpDown green;
	private readonly NumericUpDown blue;

	private bool enableDefaultColor;

	/// <summary>
	/// Invoked when the selected color has changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? ColorChanged;

	/// <summary>
	/// The "before" color.
	/// </summary>
	[Xml.XmlProperty]
	public Color DefaultColor { get { return before.Color; } set { before.Color = value; } }

	public Color SelectedColorRGB => lerpBox.SelectedColor.ToColor();

	public HSV SelectedColorHSV => lerpBox.SelectedColor;

	/// <summary>
	/// Show / hide default color box
	/// </summary>
	[Xml.XmlProperty]
	public bool EnableDefaultColor { get { return enableDefaultColor; } set { enableDefaultColor = value; UpdateChildControlVisibility(); } }
	/// <summary>
	/// Initializes a new instance of the <see cref="HSVColorPicker"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public HSVColorPicker(ControlBase? parent)
		: base(parent) {
		MouseInputEnabled = true;

		int baseSize = BaseUnit;

		lerpBox = new ColorLerpBox(this);
		lerpBox.Margin = Margin.Two;
		lerpBox.ColorChanged += ColorBoxChanged;
		lerpBox.Dock = Dock.Fill;

		ControlBase values = new VerticalLayout(this);
		values.Dock = Dock.Right;
		{
			after = new ColorDisplay(values);
			after.Size = new Size(baseSize * 5, baseSize * 2);

			before = new ColorDisplay(values);
			before.Margin = new Margin(2, 0, 2, 2);
			before.Size = new Size(baseSize * 5, baseSize * 2);

			GridLayout grid = new GridLayout(values);
			grid.Margin = new Margin(2, 0, 2, 2);
			grid.SetColumnWidths(GridLayout.AutoSize, GridLayout.Fill);
			{
				{
					Label label = new Label(grid);
					label.Text = "R: ";
					label.Alignment = Alignment.Left | Alignment.CenterV;

					red = new NumericUpDown(grid);
					red.Min = 0;
					red.Max = 255;
					red.SelectAllOnFocus = true;
					red.ValueChanged += NumericTyped;
				}

				{
					Label label = new Label(grid);
					label.Text = "G: ";
					label.Alignment = Alignment.Left | Alignment.CenterV;

					green = new NumericUpDown(grid);
					green.Min = 0;
					green.Max = 255;
					green.SelectAllOnFocus = true;
					green.ValueChanged += NumericTyped;
				}

				{
					Label label = new Label(grid);
					label.Text = "B: ";
					label.Alignment = Alignment.Left | Alignment.CenterV;

					blue = new NumericUpDown(grid);
					blue.Min = 0;
					blue.Max = 255;
					blue.SelectAllOnFocus = true;
					blue.ValueChanged += NumericTyped;
				}
			}
		}

		colorSlider = new ColorSlider(this);
		colorSlider.Margin = Margin.Two;
		colorSlider.ColorChanged += ColorSliderChanged;
		colorSlider.Dock = Dock.Right;

		EnableDefaultColor = false;

		SetColor(DefaultColor);
	}

	private void NumericTyped(ControlBase control, EventArgs args) {
		if(control is not NumericUpDown box) {
			return;
		}

		int value = (int)box.Value;
		if(value < 0) value = 0;
		if(value > 255) value = 255;

		Color selectedRgb = SelectedColorRGB;
		Color newColor = selectedRgb;

		if(box == red)
			newColor = new Color(selectedRgb.A, value, selectedRgb.G, selectedRgb.B);
		else if(box == green)
			newColor = new Color(selectedRgb.A, selectedRgb.R, value, selectedRgb.B);
		else if(box == blue)
			newColor = new Color(selectedRgb.A, selectedRgb.R, selectedRgb.G, value);
		//else if (box.Name.Contains("Alpha"))
		//    newColor = Color.FromArgb(textValue, SelectedColor.R, SelectedColor.G, SelectedColor.B);

		colorSlider.SetColor(newColor, false);
		lerpBox.SetColor(newColor, false, false);
		after.Color = newColor;

		ColorChanged?.Invoke(this, EventArgs.Empty);
	}

	private void UpdateControls(Color color) {
		red.SetValue(color.R, false);
		green.SetValue(color.G, false);
		blue.SetValue(color.B, false);
		after.Color = color;
	}

	/// <summary>
	/// Sets the selected color.
	/// </summary>
	/// <param name="color">Color to set.</param>
	/// <param name="onlyHue">Determines whether only the hue should be set.</param>
	/// <param name="reset">Determines whether the "before" color should be set as well.</param>
	public void SetColor(Color color, bool onlyHue = false, bool reset = false) {
		UpdateControls(color);

		if(reset)
			before.Color = color;

		colorSlider.SetColor(color, false);
		lerpBox.SetColor(color, onlyHue, false);
		after.Color = color;

		if(ColorChanged != null)
			ColorChanged.Invoke(this, EventArgs.Empty);
	}

	private void ColorBoxChanged(ControlBase control, EventArgs args) {
		UpdateControls(SelectedColorRGB);
		//Invalidate();

		if(ColorChanged != null)
			ColorChanged.Invoke(this, EventArgs.Empty);
	}

	private void ColorSliderChanged(ControlBase control, EventArgs args) {
		lerpBox.SetColor(colorSlider.SelectedColor, true, false);
		UpdateControls(SelectedColorRGB);
		//Invalidate();

		if(ColorChanged != null)
			ColorChanged.Invoke(this, EventArgs.Empty);
	}

	private void UpdateChildControlVisibility() {
		if(enableDefaultColor) {
			after.Margin = new Margin(2, 2, 2, 0);
			before.Margin = new Margin(2, 0, 2, 2);
			after.Height = BaseUnit * 2;
			before.Show();
		} else {
			after.Margin = Margin.Two;
			before.Collapse();
			after.Height = BaseUnit * 4;
		}
	}
}