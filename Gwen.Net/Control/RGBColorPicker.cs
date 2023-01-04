using System;
using Gwen.Net.Control.Internal;
using Gwen.Net.Control.Layout;

namespace Gwen.Net.Control;

/// <summary>
/// RGBA color picker.
/// </summary>
public class RGBColorPicker : ControlBase, IColorPicker {
	private Color color;

	/// <summary>
	/// Selected color.
	/// </summary>
	public Color SelectedColorRGB {
		get => color;
		set {
			color = value;
			UpdateControls();
		}
	}

	public HSV SelectedColorHSV {
		get => HSV.FromColor(color);
		set {
			color = value.ToColor();
			UpdateControls();
		}
	}

	/// <summary>
	/// Red value of the selected color.
	/// </summary>
	public int R {
		get => color.R; 
		set { 
			color = new Color(color.A, value, color.G, color.B); 
		} 
	}

	/// <summary>
	/// Green value of the selected color.
	/// </summary>
	public int G {
		get => color.G;
		set { 
			color = new Color(color.A, color.R, value, color.B); 
		} 
	}

	/// <summary>
	/// Blue value of the selected color.
	/// </summary>
	public int B {
		get => color.B;
		set { 
			color = new Color(color.A, color.R, color.G, value); 
		} 
	}

	/// <summary>
	/// Alpha value of the selected color.
	/// </summary>
	public int A {
		get => color.A;
		set { 
			color = new Color(value, color.R, color.G, color.B); 
		} 
	}

	/// <summary>
	/// Invoked when the selected color has been changed.
	/// </summary>
	public event GwenEventHandler<EventArgs>? ColorChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="RGBColorPicker"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public RGBColorPicker(ControlBase? parent)
		: base(parent) {
		MouseInputEnabled = true;

		CreateControls();
		SelectedColorRGB = new Color(255, 50, 60, 70);
	}

	private void CreateControls() {
		VerticalLayout colorControlLayout = new VerticalLayout(this);
		colorControlLayout.Dock = Dock.Fill;

		CreateColorControl(colorControlLayout, "Red");
		CreateColorControl(colorControlLayout, "Green");
		CreateColorControl(colorControlLayout, "Blue");
		CreateColorControl(colorControlLayout, "Alpha");

		GroupBox finalGroup = new(this) {
			Dock = Dock.Right,
			Text = "Result",
			Name = "ResultGroupBox"
		};

		DockLayout finalLayout = new(finalGroup);

		ColorDisplay disp = new(finalLayout) {
			Dock = Dock.Fill,
			Name = "Result",
			Width = Util.Ignore,
			Height = Util.Ignore
		};
	}

	private void CreateColorControl(ControlBase parent, string name) {
		GroupBox colorGroup = new GroupBox(parent);
		colorGroup.Text = name;
		colorGroup.Name = name + "groupbox";

		DockLayout layout = new DockLayout(colorGroup);

		ColorDisplay disp = new ColorDisplay(layout);
		disp.Height = Util.Ignore;
		disp.Dock = Dock.Left;
		disp.Name = name;

		TextBoxNumeric numeric = new TextBoxNumeric(layout);
		numeric.Dock = Dock.Right;
		numeric.FitToText = "000";
		numeric.Name = name + "Box";
		numeric.SelectAllOnFocus = true;
		numeric.TextChanged += NumericTyped;

		HorizontalSlider slider = new HorizontalSlider(layout);
		slider.Dock = Dock.Fill;
		slider.VerticalAlignment = VerticalAlignment.Center;
		slider.SetRange(0, 255);
		slider.Name = name + "Slider";
		slider.ValueChanged += SlidersMoved;
	}

	private void NumericTyped(ControlBase control, EventArgs args) {
		if(control is not TextBoxNumeric box) {
			return;
		}

		if(string.IsNullOrWhiteSpace(box.Text)) {
			return;
		}

		int textValue = (int)box.Value;
		if (textValue < 0) textValue = 0;
		if (textValue > 255) textValue = 255;

		if(box.Name.Contains("Red")) {
			R = textValue;
		}

		if(box.Name.Contains("Green")) {
			G = textValue;
		}

		if(box.Name.Contains("Blue")) {
			B = textValue;
		}

		if(box.Name.Contains("Alpha")) {
			A = textValue;
		}

		UpdateControls();
	}

	private void UpdateColorControls(string name, Color col, int sliderVal) {
		if(FindChildByName(name, true) is ColorDisplay disp) {
			disp.Color = col;
		}

		if(FindChildByName(name + "Slider", true) is HorizontalSlider slider) {
			slider.Value = sliderVal;
		}

		if(FindChildByName(name + "Box", true) is TextBoxNumeric box) {
			box.Value = sliderVal;
		}
	}

	private void UpdateControls() { //This is a little weird, but whatever for now
		UpdateColorControls("Red", new Color(255, SelectedColorRGB.R, 0, 0), SelectedColorRGB.R);
		UpdateColorControls("Green", new Color(255, 0, SelectedColorRGB.G, 0), SelectedColorRGB.G);
		UpdateColorControls("Blue", new Color(255, 0, 0, SelectedColorRGB.B), SelectedColorRGB.B);
		UpdateColorControls("Alpha", new Color(SelectedColorRGB.A, 255, 255, 255), SelectedColorRGB.A);

		if(FindChildByName("Result", true) is ColorDisplay disp) {
			disp.Color = SelectedColorRGB;
		}

		if (ColorChanged != null)
			ColorChanged.Invoke(this, EventArgs.Empty);
	}

	private void SlidersMoved(ControlBase control, EventArgs args) {
		/*
		HorizontalSlider* redSlider		= gwen_cast<HorizontalSlider>(	FindChildByName( "RedSlider",   true ) );
		HorizontalSlider* greenSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "GreenSlider", true ) );
		HorizontalSlider* blueSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "BlueSlider",  true ) );
		HorizontalSlider* alphaSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "AlphaSlider", true ) );
		*/

		if(control is HorizontalSlider slider) {
			SetColorByName(GetColorFromName(slider.Name), (int)slider.Value);
		}

		UpdateControls();
		//SetColor( Gwen::Color( redSlider->GetValue(), greenSlider->GetValue(), blueSlider->GetValue(), alphaSlider->GetValue() ) );
	}

	private int GetColorByName(string colorName) {
		if (colorName == "Red")
			return SelectedColorRGB.R;
		if (colorName == "Green")
			return SelectedColorRGB.G;
		if (colorName == "Blue")
			return SelectedColorRGB.B;
		if (colorName == "Alpha")
			return SelectedColorRGB.A;
		return 0;
	}

	private static string GetColorFromName(string name) {
		if (name.Contains("Red"))
			return "Red";
		if (name.Contains("Green"))
			return "Green";
		if (name.Contains("Blue"))
			return "Blue";
		if (name.Contains("Alpha"))
			return "Alpha";
		return String.Empty;
	}

	private void SetColorByName(string colorName, int colorValue) {
		if (colorName == "Red")
			R = colorValue;
		else if (colorName == "Green")
			G = colorValue;
		else if (colorName == "Blue")
			B = colorValue;
		else if (colorName == "Alpha")
			A = colorValue;
	}

	/// <summary>
	/// Determines whether the Alpha control is visible.
	/// </summary>
	public bool AlphaVisible {
		get {
			GroupBox? gb = FindChildByName("Alphagroupbox", true) as GroupBox;
			return !gb?.IsHidden ?? false;
		}
		set {
			if(FindChildByName("Alphagroupbox", true) is not GroupBox gb) {
				return;
			}
			gb.IsHidden = !value;
			//Invalidate();
		}
	}
}