using System;
using Gwen.Net.Input;

namespace Gwen.Net.Control;

/// <summary>
/// RadioButton with label.
/// </summary>
[Xml.XmlControl]
public class LabeledRadioButton : ControlBase {
	private readonly RadioButton radioButton;
	private readonly Label label;

	/// <summary>
	/// Label text.
	/// </summary>
	[Xml.XmlProperty]
	public string Text { get { return label.Text; } set { label.Text = value; } }

	/// <summary>
	/// Invoked when the radiobutton has been checked.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs> Checked {
		add {
			radioButton.Checked += value;
		}
		remove {
			radioButton.Checked -= value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LabeledRadioButton"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public LabeledRadioButton(ControlBase? parent)
		: base(parent) {
		MouseInputEnabled = true;

		radioButton = new RadioButton(this);
		radioButton.IsTabable = false;
		radioButton.KeyboardInputEnabled = false;

		label = new Label(this);
		label.Alignment = Alignment.CenterV | Alignment.Left;
		label.Text = "Radio Button";
		label.Clicked += delegate (ControlBase control, ClickedEventArgs args) { radioButton.Press(control); };
		label.IsTabable = false;
		label.KeyboardInputEnabled = false;
	}

	protected override Size Measure(Size availableSize) {
		Size labelSize = label.DoMeasure(availableSize);
		Size radioButtonSize = radioButton.DoMeasure(availableSize);

		return new Size(labelSize.Width + 4 + radioButtonSize.Width, Math.Max(labelSize.Height, radioButtonSize.Height));
	}

	protected override Size Arrange(Size finalSize) {
		if(radioButton.MeasuredSize.Height > label.MeasuredSize.Height) {
			radioButton.DoArrange(new Rectangle(0, 0, radioButton.MeasuredSize.Width, radioButton.MeasuredSize.Height));
			label.DoArrange(new Rectangle(radioButton.MeasuredSize.Width + 4, (radioButton.MeasuredSize.Height - label.MeasuredSize.Height) / 2, label.MeasuredSize.Width, label.MeasuredSize.Height));
		} else {
			radioButton.DoArrange(new Rectangle(0, (label.MeasuredSize.Height - radioButton.MeasuredSize.Height) / 2, radioButton.MeasuredSize.Width, radioButton.MeasuredSize.Height));
			label.DoArrange(new Rectangle(radioButton.MeasuredSize.Width + 4, 0, label.MeasuredSize.Width, label.MeasuredSize.Height));
		}

		return finalSize;
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

	// todo: would be nice to remove that
	internal RadioButton RadioButton { get { return radioButton; } }

	/// <summary>
	/// Handler for Space keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeySpace(bool down) {
		if(down)
			radioButton.IsChecked = !radioButton.IsChecked;
		return true;
	}

	/// <summary>
	/// Selects the radio button.
	/// </summary>
	public virtual void Select() {
		radioButton.IsChecked = true;
	}
}