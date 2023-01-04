using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// ComboBox control.
/// </summary>
[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
public class ComboBox : ComboBoxBase {
	private readonly Button button;
	private readonly DownArrow downArrow;

	internal bool IsDepressed { get { return button.IsDepressed; } }
	public override bool IsHovered { get { return button.IsHovered; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="ComboBox"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ComboBox(ControlBase? parent)
		: base(parent) {
		button = new Button(this);
		button.Alignment = Alignment.Left | Alignment.CenterV;
		button.Text = String.Empty;
		button.TextPadding = Padding.Three;
		button.Clicked += OnClicked;

		downArrow = new DownArrow(this);

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
	/// Removes all items.
	/// </summary>
	public override void RemoveAll() {
		button.Text = String.Empty;
		base.RemoveAll();
	}

	/// <summary>
	/// Internal handler for item selected event.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected override void OnItemSelected(ControlBase control, ItemSelectedEventArgs args) {
		if(!IsDisabled) {
			if(control is not MenuItem item) return;

			button.Text = item.Text;
		}

		base.OnItemSelected(control, args);
	}

	protected override Size Measure(Size availableSize) {
		return Size.Max(button.DoMeasure(availableSize), downArrow.DoMeasure(availableSize));
	}

	protected override Size Arrange(Size finalSize) {
		button.DoArrange(new Rectangle(Point.Zero, finalSize));

		downArrow.DoArrange(new Rectangle(finalSize.Width - button.TextPadding.Right - downArrow.MeasuredSize.Width, (finalSize.Height - downArrow.MeasuredSize.Height) / 2, downArrow.MeasuredSize.Width, downArrow.MeasuredSize.Height));

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
		ComboBox element = new ComboBox(parent);
		parser.ParseAttributes(element);
		if(parser.MoveToContent()) {
			foreach(string elementName in parser.NextElement()) {
				if(elementName == "Option") {
					if(parser.ParseElement<MenuItem>(element) is MenuItem item) {
						element.AddItem(item);
					}
				}
			}
		}
		return element;
	}
}