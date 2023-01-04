namespace Gwen.Net.Control;

public enum BorderType {
	ToolTip,
	StatusBar,
	MenuStrip,
	Selection,
	PanelNormal,
	PanelBright,
	PanelDark,
	PanelHighlight,
	ListBox,
	TreeControl,
	CategoryList
}

[Xml.XmlControl]
public class Border : ControlBase {
	private BorderType borderType;

	[Xml.XmlProperty]
	public BorderType BorderType { get { return borderType; } set { if(borderType == value) return; borderType = value; } }

	public Border(ControlBase? parent)
		: base(parent) {
		borderType = BorderType.PanelNormal;
	}

	protected override void Render(Skin.SkinBase skin) {
		skin.DrawBorder(this, borderType);
	}
}