using System;

namespace Gwen.Net.Control;

public interface IColorPicker {
	/// <summary>
	/// The selected color in RGB form.
	/// </summary>
	Color SelectedColorRGB { get; }
	/// <summary>
	/// The selected color in HSV form.
	/// </summary>
	HSV SelectedColorHSV { get; }
}