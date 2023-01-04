using System;

namespace Gwen.Net.Control;

/// <summary>
/// Text box with masked text.
/// </summary>
/// <remarks>
/// This class doesn't prevent programatic access to the text in any way.
/// </remarks>
[Xml.XmlControl]
public class TextBoxPassword : TextBox {
	private string mask = "";
	private char maskChar;

	/// <summary>
	/// Character used in place of actual characters for display.
	/// </summary>
	[Xml.XmlProperty]
	public char MaskCharacter { get { return maskChar; } set { maskChar = value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="TextBoxPassword"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public TextBoxPassword(ControlBase? parent)
		: base(parent) {
		maskChar = '*';
	}

	/// <summary>
	/// Handler for text changed event.
	/// </summary>
	protected override void OnTextChanged() {
		mask = new String(MaskCharacter, Text.Length);
		TextOverride = mask;
		base.OnTextChanged();
	}
}