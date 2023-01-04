using System;

namespace Gwen.Net.Control;

/// <summary>
/// CheckBox with label.
/// </summary>
[Xml.XmlControl]
public class LabeledCheckBox : ControlBase {
	private readonly CheckBox checkBox;
	private readonly Label label;

	/// <summary>
	/// Invoked when the control has been checked.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Checked;

	/// <summary>
	/// Invoked when the control has been unchecked.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? UnChecked;

	/// <summary>
	/// Invoked when the control's check has been changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? CheckChanged;

	/// <summary>
	/// Indicates whether the control is checked.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsChecked { get { return checkBox.IsChecked; } set { checkBox.IsChecked = value; } }

	/// <summary>
	/// Label text.
	/// </summary>
	[Xml.XmlProperty]
	public string Text { get { return label.Text; } set { label.Text = value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="LabeledCheckBox"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public LabeledCheckBox(ControlBase? parent)
		: base(parent) {
		checkBox = new CheckBox(this);
		checkBox.IsTabable = false;
		checkBox.CheckChanged += OnCheckChanged;

		label = new Label(this);
		label.Clicked += delegate (ControlBase Control, ClickedEventArgs args) { checkBox.Press(Control); };
		label.IsTabable = false;

		IsTabable = false;
	}

	protected override Size Measure(Size availableSize) {
		Size labelSize = label.DoMeasure(availableSize);
		Size radioButtonSize = checkBox.DoMeasure(availableSize);

		return new Size(labelSize.Width + 4 + radioButtonSize.Width, Math.Max(labelSize.Height, radioButtonSize.Height));
	}

	protected override Size Arrange(Size finalSize) {
		if(checkBox.MeasuredSize.Height > label.MeasuredSize.Height) {
			checkBox.DoArrange(new Rectangle(0, 0, checkBox.MeasuredSize.Width, checkBox.MeasuredSize.Height));
			label.DoArrange(new Rectangle(checkBox.MeasuredSize.Width + 4, (checkBox.MeasuredSize.Height - label.MeasuredSize.Height) / 2, label.MeasuredSize.Width, label.MeasuredSize.Height));
		} else {
			checkBox.DoArrange(new Rectangle(0, (label.MeasuredSize.Height - checkBox.MeasuredSize.Height) / 2, checkBox.MeasuredSize.Width, checkBox.MeasuredSize.Height));
			label.DoArrange(new Rectangle(checkBox.MeasuredSize.Width + 4, 0, label.MeasuredSize.Width, label.MeasuredSize.Height));
		}

		return MeasuredSize;
	}

	/// <summary>
	/// Handler for CheckChanged event.
	/// </summary>
	protected virtual void OnCheckChanged(ControlBase control, EventArgs Args) {
		if(checkBox.IsChecked) {
			if(Checked != null)
				Checked.Invoke(this, EventArgs.Empty);
		} else {
			if(UnChecked != null)
				UnChecked.Invoke(this, EventArgs.Empty);
		}

		if(CheckChanged != null)
			CheckChanged.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Handler for Space keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeySpace(bool down) {
		base.OnKeySpace(down);
		if(!down)
			checkBox.IsChecked = !checkBox.IsChecked;
		return true;
	}
}