using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Menu item.
/// </summary>
[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
public class MenuItem : Button {
	private bool checkable;
	private bool isChecked;
	private Menu? menu;
	private ControlBase? submenuArrow;
	private Label? accelerator;

	/// <summary>
	/// Indicates whether the item is on a menu strip.
	/// </summary>
	public bool IsOnStrip { get { return Parent is MenuStrip; } }

	/// <summary>
	/// Determines if the menu item is checkable.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsCheckable { get { return checkable; } set { checkable = value; } }

	/// <summary>
	/// Indicates if the parent menu is open.
	/// </summary>
	public bool IsMenuOpen { get { if(menu == null) return false; return !menu.IsCollapsed; } }

	/// <summary>
	/// Gets or sets the check value.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsChecked {
		get { return isChecked; }
		set {
			if(value == isChecked)
				return;

			isChecked = value;

			if(CheckChanged != null)
				CheckChanged.Invoke(this, EventArgs.Empty);

			if(value) {
				if(Checked != null)
					Checked.Invoke(this, EventArgs.Empty);
			} else {
				if(UnChecked != null)
					UnChecked.Invoke(this, EventArgs.Empty);
			}
		}
	}

	/// <summary>
	/// Gets the parent menu.
	/// </summary>
	public Menu Menu {
		get {
			if(null == menu) {
				menu = new Menu(GetCanvas());
				menu.ParentMenuItem = this;

				if(!IsOnStrip) {
					if(submenuArrow != null)
						submenuArrow.Dispose();
					submenuArrow = new RightArrow(this);
				}
			}

			return menu;
		}
	}

	/// <summary>
	/// Invoked when the item is selected.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<ItemSelectedEventArgs>? Selected;

	/// <summary>
	/// Invoked when the item is checked.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Checked;

	/// <summary>
	/// Invoked when the item is unchecked.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? UnChecked;

	/// <summary>
	/// Invoked when the item's check value is changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? CheckChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="MenuItem"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public MenuItem(ControlBase? parent)
		: base(parent) {
		IsTabable = false;
		IsCheckable = false;
		IsChecked = false;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawMenuItem(this, IsMenuOpen, checkable ? isChecked : false);
	}

	protected override Size Measure(Size availableSize) {
		Size size = base.Measure(availableSize);
		if(accelerator != null) {
			Size accSize = accelerator.DoMeasure(availableSize);
			size.Width += accSize.Width;
		}
		if(submenuArrow != null) {
			submenuArrow.DoMeasure(availableSize);
		}

		return size;
	}

	protected override Size Arrange(Size finalSize) {
		if(submenuArrow != null)
			submenuArrow.DoArrange(new Rectangle(finalSize.Width - Padding.Right - submenuArrow.MeasuredSize.Width, (finalSize.Height - submenuArrow.MeasuredSize.Height) / 2, submenuArrow.MeasuredSize.Width, submenuArrow.MeasuredSize.Height));

		if(accelerator != null)
			accelerator.DoArrange(new Rectangle(finalSize.Width - Padding.Right - accelerator.MeasuredSize.Width, (finalSize.Height - accelerator.MeasuredSize.Height) / 2, accelerator.MeasuredSize.Width, accelerator.MeasuredSize.Height));

		return base.Arrange(finalSize);
	}

	/// <summary>
	/// Internal OnPressed implementation.
	/// </summary>
	protected override void OnClicked(int x, int y) {
		if(menu != null) {
			if(!IsMenuOpen)
				OpenMenu();
		} else if(!IsOnStrip) {
			IsChecked = !IsChecked;
			if(Selected != null)
				Selected.Invoke(this, new ItemSelectedEventArgs(this));
			GetCanvas().CloseMenus();
		}
		base.OnClicked(x, y);
	}

	/// <summary>
	/// Toggles the menu open state.
	/// </summary>
	public void ToggleMenu() {
		if(IsMenuOpen)
			CloseMenu();
		else
			OpenMenu();
	}

	/// <summary>
	/// Opens the menu.
	/// </summary>
	public void OpenMenu() {
		if(null == menu) return;

		menu.Show();
		menu.BringToFront();

		Point p = LocalPosToCanvas(Point.Zero);

		// Strip menus open downwards
		if(IsOnStrip) {
			menu.Position = new Point(p.X, p.Y + ActualHeight - 2);
		}
		// Submenus open sidewards
		else {
			menu.Position = new Point(p.X + ActualWidth, p.Y);
		}

		// TODO: Option this.
		// TODO: Make sure on screen, open the other side of the 
		// parent if it's better...
	}

	/// <summary>
	/// Closes the menu.
	/// </summary>
	public void CloseMenu() {
		if(null == menu) return;
		menu.Close();
		menu.CloseAll();
	}

	public MenuItem SetAction(GwenEventHandler<EventArgs> handler) {
		if(accelerator != null) {
			AddAccelerator(accelerator.Text, handler);
		}

		Selected += handler;
		return this;
	}

	public void SetAccelerator(string acc) {
		if(accelerator != null) {
			accelerator = null;
		}

		if(acc == String.Empty)
			return;

		accelerator = new Label(this);
		accelerator.Text = acc;
		accelerator.Margin = new Margin(0, 0, 16, 0);
	}

	public override ControlBase? FindChildByName(string name, bool recursive = false) {
		ControlBase? item = base.FindChildByName(name, recursive);
		if(item == null && menu != null) {
			item = menu.FindChildByName(name, recursive);
		}

		return item;
	}

	internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent) {
		MenuItem element = new MenuItem(parent);
		parser.ParseAttributes(element);
		if(parser.MoveToContent()) {
			ControlBase? e = parent;
			while(e != null && e.Component == null)
				e = e.Parent;

			foreach(string elementName in parser.NextElement()) {
				if(elementName == "MenuItem") {
					MenuItem? item = parser.ParseElement<MenuItem>(element);
					if(item != null) {
						element.Menu.AddItem(item);
					}
				} else if(elementName == "MenuDivider")
					element.Menu.AddDivider();

				element.Menu.Component = e?.Component;
			}
		}
		return element;
	}
}