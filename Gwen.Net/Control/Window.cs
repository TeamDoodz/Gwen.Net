using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Movable window with title bar.
/// </summary>
[Xml.XmlControl]
public class Window : WindowBase {
	private readonly WindowTitleBar titleBar;
	private Modal? modal;

	/// <summary>
	/// Window caption.
	/// </summary>
	[Xml.XmlProperty]
	public string Title { get { return titleBar.Title.Text; } set { titleBar.Title.Text = value; } }

	/// <summary>
	/// Determines whether the window has close button.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsClosable { get { return !titleBar.CloseButton.IsCollapsed; } set { titleBar.CloseButton.IsCollapsed = !value; } }

	/// <summary>
	/// Make window modal and set background color. If alpha value is zero, make background dimmed.
	/// </summary>
	[Xml.XmlProperty]
	public Color ModalBackground { set { if(value.A == 0) MakeModal(true); else MakeModal(true, value); } }

	/// <summary>
	/// Set true to make window modal.
	/// </summary>
	[Xml.XmlProperty]
	public bool Modal { get { return modal != null; } set { MakeModal(); } }

	/// <summary>
	/// Initializes a new instance of the <see cref="Window"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public Window(ControlBase? parent)
		: base(parent) {
		titleBar = new WindowTitleBar(this);
		titleBar.Height = BaseUnit + 9;
		titleBar.Title.TextColor = Skin.Colors.Window.TitleInactive;
		titleBar.CloseButton.Clicked += CloseButtonPressed;
		titleBar.SendToBack();
		titleBar.Dragged += OnDragged;

		dragBar = titleBar;

		innerPanel = new InnerContentControl(this);
		innerPanel.SendToBack();
	}

	public override void Close() {
		if(modal != null) {
			modal.DelayedDelete();
			modal = null;
		}

		base.Close();
	}

	protected virtual void CloseButtonPressed(ControlBase control, EventArgs args) {
		Close();
	}

	/// <summary>
	/// Makes the window modal: covers the whole canvas and gets all input.
	/// </summary>
	/// <param name="dim">Determines whether all the background should be dimmed.</param>
	/// <param name="backgroundColor">Determines background color.</param>
	public void MakeModal(bool dim = false, Color? backgroundColor = null) {
		if(modal != null)
			return;

		modal = new Modal(GetCanvas());
		Parent = modal;

		if(dim)
			modal.ShouldDrawBackground = true;
		else
			modal.ShouldDrawBackground = false;

		if(backgroundColor != null) {
			modal.ShouldDrawBackground = true;
			modal.BackgroundColor = backgroundColor;
		}
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		bool hasFocus = IsOnTop;

		if(hasFocus)
			titleBar.Title.TextColor = Skin.Colors.Window.TitleActive;
		else
			titleBar.Title.TextColor = Skin.Colors.Window.TitleInactive;

		skin.DrawWindow(this, titleBar.ActualHeight, hasFocus);
	}

	/// <summary>
	/// Renders under the actual control (shadows etc).
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void RenderUnder(Skin.SkinBase skin) {
		base.RenderUnder(skin);
		skin.DrawShadow(this);
	}

	/// <summary>
	/// Renders the focus overlay.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void RenderFocus(Skin.SkinBase skin) {

	}

	protected override Size Measure(Size availableSize) {
		Size titleBarSize = titleBar.DoMeasure(new Size(availableSize.Width, availableSize.Height));

		innerPanel?.DoMeasure(new Size(availableSize.Width, availableSize.Height - titleBarSize.Height));

		return base.Measure(new Size(innerPanel?.MeasuredSize.Width ?? 1, innerPanel?.MeasuredSize.Height ?? 1 + titleBarSize.Height));
	}

	protected override Size Arrange(Size finalSize) {
		titleBar.DoArrange(new Rectangle(0, 0, finalSize.Width, titleBar.MeasuredSize.Height));

		if(innerPanel != null)
			innerPanel.DoArrange(new Rectangle(0, titleBar.MeasuredSize.Height, finalSize.Width, finalSize.Height - titleBar.MeasuredSize.Height));

		return base.Arrange(finalSize);
	}

	public override void EnableResizing(bool left = true, bool top = true, bool right = true, bool bottom = true) {
		base.EnableResizing(left, false, right, bottom);
	}

	public override void Dispose() {
		if(modal != null) {
			modal.DelayedDelete();
			modal = null;
		} else {
			base.Dispose();
		}
	}
}