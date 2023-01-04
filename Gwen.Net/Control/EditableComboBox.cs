using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Editable ComboBox control.
/// </summary>
[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
public class EditableComboBox : ComboBoxBase {
	private readonly TextBox textBox;
	private readonly ComboBoxButton button;

	/// <summary>
	/// Invoked when the text has changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? TextChanged {
		add {
			if(value == null) {
				return;
			}
			textBox.TextChanged += value;
		}
		remove {
			if(value == null) {
				return;
			}
			textBox.TextChanged -= value;
		}
	}

	/// <summary>
	/// Invoked when the submit key has been pressed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? SubmitPressed {
		add {
			if(value == null) {
				return;
			}
			textBox.SubmitPressed += value;
		}
		remove {
			if(value == null) {
				return;
			}
			textBox.SubmitPressed -= value;
		}
	}

	/// <summary>
	/// Text.
	/// </summary>
	[Xml.XmlProperty]
	public virtual string Text { get { return textBox.Text; } set { textBox.SetText(value); } }

	/// <summary>
	/// Text color.
	/// </summary>
	[Xml.XmlProperty]
	public Color TextColor { get { return textBox.TextColor; } set { textBox.TextColor = value; } }

	/// <summary>
	/// Font.
	/// </summary>
	[Xml.XmlProperty]
	public Font? Font { get { return textBox.Font; } set { textBox.Font = value; } }

	internal bool IsDepressed { get { return button.IsDepressed; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="EditableComboBox"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public EditableComboBox(ControlBase? parent)
		: base(parent) {
		textBox = new TextBox(this);

		button = new ComboBoxButton(textBox, this);
		button.Dock = Dock.Right;
		button.Clicked += OnClicked;

		IsTabable = true;
		KeyboardInputEnabled = true;
	}

	/// <summary>
	/// Internal Pressed implementation.
	/// </summary>
	private void OnClicked(ControlBase sender, ClickedEventArgs args) {
		if(IsOpen) {
			Close();
		} else {
			Open();
		}
	}

	/// <summary>
	/// Internal handler for item selected event.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected override void OnItemSelected(ControlBase control, ItemSelectedEventArgs args) {
		if(!IsDisabled) {
			if(control is not MenuItem item) return;

			textBox.Text = item.Text;
		}

		base.OnItemSelected(control, args);
	}

	protected override Size Measure(Size availableSize) {
		return textBox.DoMeasure(availableSize);
	}

	protected override Size Arrange(Size finalSize) {
		textBox.DoArrange(new Rectangle(Point.Zero, finalSize));

		return finalSize;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawComboBox(this, button.IsDepressed, IsOpen);
	}

	/// <summary>
	/// Renders the focus overlay.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void RenderFocus(Skin.SkinBase skin) {

	}

	internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent) {
		EditableComboBox element = new EditableComboBox(parent);
		parser.ParseAttributes(element);
		if(parser.MoveToContent()) {
			foreach(string elementName in parser.NextElement()) {
				if(elementName == "Option") {
					MenuItem? item = parser.ParseElement<MenuItem>(element);
					if(item != null) {
						element.AddItem(item);
					}
				}
			}
		}
		return element;
	}
}