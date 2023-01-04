using System;
using Gwen.Net.Control.Internal;
using Gwen.Net.Control.Layout;

namespace Gwen.Net.Control;

/// <summary>
/// Control with multiple tabs that can be reordered and dragged.
/// </summary>
[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
public class TabControl : ContentControl {
	private readonly TabStrip tabStrip;
	private readonly ScrollBarButton[] scroll;
	private TabButton? currentButton;

	private Padding actualPadding;

	/// <summary>
	/// Invoked when a tab has been added.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? TabAdded;

	/// <summary>
	/// Invoked when a tab has been removed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? TabRemoved;

	/// <summary>
	/// Determines if tabs can be reordered by dragging.
	/// </summary>
	[Xml.XmlProperty]
	public bool AllowReorder { get { return tabStrip.AllowReorder; } set { tabStrip.AllowReorder = value; } }

	/// <summary>
	/// Currently active tab button.
	/// </summary>
	public TabButton? CurrentButton { get { return currentButton; } }

	/// <summary>
	/// Current tab strip position.
	/// </summary>
	[Xml.XmlProperty]
	public Dock TabStripPosition { get { return tabStrip.StripPosition; } set { tabStrip.StripPosition = value; } }

	/// <summary>
	/// Tab strip.
	/// </summary>
	public TabStrip TabStrip { get { return tabStrip; } }

	/// <summary>
	/// Number of tabs in the control.
	/// </summary>
	public int TabCount { get { return tabStrip.Children.Count; } }

	// Ugly way to implement padding but other ways would be more complicated
	[Xml.XmlProperty]
	public override Padding Padding {
		get {
			return actualPadding;
		}
		set {
			actualPadding = value;

			foreach(ControlBase tab in tabStrip.Children) {
				tab.Margin = (Margin)value;
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TabControl"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public TabControl(ControlBase? parent)
		: base(parent) {
		tabStrip = new TabStrip(this);
		tabStrip.StripPosition = Dock.Top;

		// Actually these should be inside the TabStrip but it would make things complicated
		// because TabStrip contains only TabButtons. ScrollButtons being here we don't need
		// an inner panel for TabButtons on the TabStrip.
		scroll = new ScrollBarButton[2];

		scroll[0] = new ScrollBarButton(this);
		scroll[0].SetDirectionLeft();
		scroll[0].Clicked += ScrollPressedLeft;
		scroll[0].Size = new Size(BaseUnit);

		scroll[1] = new ScrollBarButton(this);
		scroll[1].SetDirectionRight();
		scroll[1].Clicked += ScrollPressedRight;
		scroll[1].Size = new Size(BaseUnit);

		innerPanel = new TabControlInner(this);
		innerPanel.Dock = Dock.Fill;
		innerPanel.SendToBack();

		IsTabable = false;

		actualPadding = new Padding(6, 6, 6, 6);
	}

	/// <summary>
	/// Adds a new page/tab.
	/// </summary>
	/// <param name="label">Tab label.</param>
	/// <param name="page">Page contents.</param>
	/// <returns>Newly created control.</returns>
	public TabButton AddPage(string label, ControlBase page) {
		if(null == page) {
			page = new DockLayout(this);
		} else {
			page.Parent = this;
		}

		TabButton button = new TabButton(tabStrip);
		button.Text = label;
		button.Page = page;
		button.IsTabable = false;

		AddPage(button);
		return button;
	}

	/// <summary>
	/// Adds a new page/tab.
	/// </summary>
	/// <param name="label">Tab label.</param>
	/// <returns>Newly created control.</returns>
	public TabButton AddPage(string label) {
		return AddPage(label, new DockLayout(this));
	}

	/// <summary>
	/// Adds a page/tab.
	/// </summary>
	/// <param name="button">Page to add. (well, it's a TabButton which is a parent to the page).</param>
	internal void AddPage(TabButton button) {
		ControlBase? page = button.Page;
		if(page != null) {
			page.Parent = this;
			page.IsHidden = true;
			page.Dock = Dock.Fill;
			page.Margin = (Margin)this.Padding;
		}

		button.Parent = tabStrip;
		button.TabControl?.UnsubscribeTabEvent(button);
		button.TabControl = this;
		button.Clicked += OnTabPressed;

		if(null == currentButton) {
			button.Press();
		}

		if(TabAdded != null)
			TabAdded.Invoke(this, EventArgs.Empty);

		Invalidate();
	}

	private void UnsubscribeTabEvent(TabButton button) {
		button.Clicked -= OnTabPressed;
	}

	/// <summary>
	/// Handler for tab selection.
	/// </summary>
	/// <param name="control">Event source (TabButton).</param>
	internal virtual void OnTabPressed(ControlBase control, EventArgs args) {
		if(control is not TabButton button) return;

		ControlBase? page = button.Page;
		if(null == page) return;

		if(currentButton == button)
			return;

		if(null != currentButton) {
			ControlBase? page2 = currentButton.Page;
			if(page2 != null) {
				page2.IsHidden = true;
			}
			currentButton.Redraw();
			currentButton = null;
		}

		currentButton = button;

		page.IsHidden = false;
	}

	protected override Size Arrange(Size finalSize) {
		Size size = base.Arrange(finalSize);

		// At this point we know TabStrip location so lets move ScrollButtons
		int buttonSize = scroll[0].Size.Width;
		switch(tabStrip.StripPosition) {
			case Dock.Top:
				scroll[0].SetPosition(tabStrip.ActualRight - 5 - buttonSize - buttonSize, tabStrip.ActualTop + 5);
				scroll[1].SetPosition(tabStrip.ActualRight - 5 - buttonSize, tabStrip.ActualTop + 5);
				scroll[0].SetDirectionLeft();
				scroll[1].SetDirectionRight();
				break;
			case Dock.Bottom:
				scroll[0].SetPosition(tabStrip.ActualRight - 5 - buttonSize - buttonSize, tabStrip.ActualBottom - 5 - buttonSize);
				scroll[1].SetPosition(tabStrip.ActualRight - 5 - buttonSize, tabStrip.ActualBottom - 5 - buttonSize);
				scroll[0].SetDirectionLeft();
				scroll[1].SetDirectionRight();
				break;
			case Dock.Left:
				scroll[0].SetPosition(tabStrip.ActualLeft + 5, tabStrip.ActualBottom - 5 - buttonSize - buttonSize);
				scroll[1].SetPosition(tabStrip.ActualLeft + 5, tabStrip.ActualBottom - 5 - buttonSize);
				scroll[0].SetDirectionUp();
				scroll[1].SetDirectionDown();
				break;
			case Dock.Right:
				scroll[0].SetPosition(tabStrip.ActualRight - 5 - buttonSize, tabStrip.ActualBottom - 5 - buttonSize - buttonSize);
				scroll[1].SetPosition(tabStrip.ActualRight - 5 - buttonSize, tabStrip.ActualBottom - 5 - buttonSize);
				scroll[0].SetDirectionUp();
				scroll[1].SetDirectionDown();
				break;
		}

		return size;
	}

	/// <summary>
	/// Handler for tab removing.
	/// </summary>
	/// <param name="button"></param>
	internal virtual void OnLoseTab(TabButton button) {
		if(currentButton == button)
			currentButton = null;

		//TODO: Select a tab if any exist.

		if(TabRemoved != null)
			TabRemoved.Invoke(this, EventArgs.Empty);

		Invalidate();
	}

	protected override void OnBoundsChanged(Rectangle oldBounds) {
		bool needed = false;

		switch(TabStripPosition) {
			case Dock.Top:
			case Dock.Bottom:
				needed = tabStrip.TotalSize.Width > ActualWidth;
				break;
			case Dock.Left:
			case Dock.Right:
				needed = tabStrip.TotalSize.Height > ActualHeight;
				break;
		}

		scroll[0].IsHidden = !needed;
		scroll[1].IsHidden = !needed;

		base.OnBoundsChanged(oldBounds);
	}

	protected virtual void ScrollPressedLeft(ControlBase control, EventArgs args) {
		tabStrip.ScrollOffset--;
	}

	protected virtual void ScrollPressedRight(ControlBase control, EventArgs args) {
		tabStrip.ScrollOffset++;
	}

	internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent) {
		TabControl element = new TabControl(parent);
		parser.ParseAttributes(element);
		if(parser.MoveToContent()) {
			foreach(string elementName in parser.NextElement()) {
				if(elementName == "TabPage") {
					string pageLabel = parser.GetAttribute("Text") ?? "";

					string pageName = parser.GetAttribute("Name") ?? "";

					TabButton button = element.AddPage(pageLabel);
					button.Name = pageName;

					ControlBase? page = button.Page;
					if(page != null) {
						parser.ParseContainerContent(page);
					}
				}
			}
		}
		return element;
	}
}