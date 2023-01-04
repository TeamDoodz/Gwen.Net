namespace Gwen.Net.Control;

/// <summary>
/// Status bar.
/// </summary>
[Xml.XmlControl]
public class StatusBar : ControlBase {
	private Label label;

	[Xml.XmlProperty]
	public string Text { get { return label.Text; } set { label.Text = value; } }

	[Xml.XmlProperty]
	public Color TextColor { get { return label.TextColor; } set { label.TextColor = value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="StatusBar"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public StatusBar(ControlBase? parent) : base(parent) {
		Height = BaseUnit + 11;
		Dock = Dock.Bottom;
		Padding = new Padding(6, 2, 6, 1);

		this.HorizontalAlignment = HorizontalAlignment.Stretch;
		this.VerticalAlignment = VerticalAlignment.Bottom;

		label = new Label(this);
		label.AutoSizeToContents = false;
		label.Alignment = Alignment.Left | Alignment.CenterV;
		label.Dock = Dock.Fill;
	}

	/// <summary>
	/// Adds a control to the bar.
	/// </summary>
	/// <param name="control">Control to add.</param>
	/// <param name="right">Determines whether the control should be added to the right side of the bar.</param>
	public void AddControl(ControlBase control, bool right) {
		control.Parent = this;
		control.Dock = right ? Dock.Right : Dock.Left;
		control.VerticalAlignment = VerticalAlignment.Center;
	}

	protected override void OnChildAdded(ControlBase child) {
		child.VerticalAlignment = VerticalAlignment.Center;
		if(child.Dock != Dock.Left)
			child.Dock = Dock.Right;

		base.OnChildAdded(child);
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawStatusBar(this);
	}
}