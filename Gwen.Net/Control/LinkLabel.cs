using System;

namespace Gwen.Net.Control;

public class LinkClickedEventArgs : EventArgs {
	public string Link { get; private set; }

	internal LinkClickedEventArgs(string link) {
		this.Link = link;
	}
}

[Xml.XmlControl]
public class LinkLabel : Label {
	private Color normalColor;
	private Font? normalFont;
	private Color? hoverColor;

	[Xml.XmlProperty]
	public string Link { get; set; } = "";

	[Xml.XmlProperty]
	public Color HoverColor { get { return hoverColor != null ? (Color)hoverColor : this.TextColor; } set { hoverColor = value; } }

	[Xml.XmlProperty]
	public Font? HoverFont { get; set; }

	[Xml.XmlEvent]
	public event ControlBase.GwenEventHandler<LinkClickedEventArgs>? LinkClicked;

	public LinkLabel(ControlBase? parent)
		: base(parent) {
		hoverColor = null;
		HoverFont = null;

		base.HoverEnter += OnHoverEnter;
		base.HoverLeave += OnHoverLeave;
		base.Clicked += OnClicked;
	}

	private void OnHoverEnter(ControlBase control, EventArgs args) {
		Cursor = Cursor.Finger;

		normalColor = text.TextColor;
		text.TextColor = this.HoverColor;

		if(this.HoverFont != null) {
			normalFont = text.Font;
			text.Font = this.HoverFont;
		}
	}

	private void OnHoverLeave(ControlBase control, EventArgs args) {
		text.TextColor = normalColor;

		if(this.HoverFont != null) {
			text.Font = normalFont;
		}
	}

	private void OnClicked(ControlBase control, ClickedEventArgs args) {
		if(LinkClicked != null)
			LinkClicked(this, new LinkClickedEventArgs(this.Link));
	}
}