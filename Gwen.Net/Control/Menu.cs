using System;
using System.Collections.Generic;
using System.Linq;
using Gwen.Net.Control.Internal;
using Gwen.Net.Control.Layout;

namespace Gwen.Net.Control;

/// <summary>
/// Popup menu.
/// </summary>
[Xml.XmlControl]
public class Menu : ScrollControl {
	protected StackLayout layout;

	private bool disableIconMargin;
	private bool deleteOnClose;

	private MenuItem? parentMenuItem;

	internal override bool IsMenuComponent { get { return true; } }

	/// <summary>
	/// Parent menu item that owns the menu if this is a child of the menu item.
	/// Real parent of the menu is the canvas.
	/// </summary>
	public MenuItem? ParentMenuItem { get { return parentMenuItem; } internal set { parentMenuItem = value; } }

	[Xml.XmlProperty]
	public bool IconMarginDisabled { get { return disableIconMargin; } set { disableIconMargin = value; } }

	/// <summary>
	/// Determines whether the menu should be disposed on close.
	/// </summary>
	[Xml.XmlProperty]
	public bool DeleteOnClose { get { return deleteOnClose; } set { deleteOnClose = value; } }

	/// <summary>
	/// Determines whether the menu should open on mouse hover.
	/// </summary>
	protected virtual bool ShouldHoverOpenMenu { get { return true; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="Menu"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public Menu(ControlBase? parent)
		: base(parent) {
		Padding = Padding.Two;

		Collapse(true, false);

		IconMarginDisabled = false;

		AutoHideBars = true;
		EnableScroll(false, true);
		DeleteOnClose = false;

		this.HorizontalAlignment = HorizontalAlignment.Left;
		this.VerticalAlignment = VerticalAlignment.Top;

		layout = new StackLayout(this);
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawMenu(this, IconMarginDisabled);
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
	///  Opens the menu.
	/// </summary>
	public void Open() {
		Show();
		BringToFront();
		Point mouse = Input.InputHandler.MousePosition;
		SetPosition(mouse.X, mouse.Y);
	}

	protected override Size Measure(Size availableSize) {
		availableSize.Height = Math.Min(availableSize.Height, GetCanvas().ActualHeight - this.Top);

		Size size = base.Measure(availableSize);

		size.Width = Math.Min(this.Content?.MeasuredSize.Width ?? 1 + Padding.Left + Padding.Right, availableSize.Width);
		size.Height = Math.Min(this.Content?.MeasuredSize.Height ?? 1 + Padding.Top + Padding.Bottom, availableSize.Height);

		return size;
	}

	/// <summary>
	/// Adds a new menu item.
	/// </summary>
	/// <param name="text">Item text.</param>
	/// <returns>Newly created control.</returns>
	public virtual MenuItem AddItem(string text) {
		return AddItem(text, String.Empty);
	}

	/// <summary>
	/// Adds a new menu item.
	/// </summary>
	/// <param name="text">Item text.</param>
	/// <param name="iconName">Icon texture name.</param>
	/// <param name="accelerator">Accelerator for this item.</param>
	/// <returns>Newly created control.</returns>
	public virtual MenuItem AddItem(string text, string iconName, string accelerator = "") {
		MenuItem item = new(this) {
			Padding = Padding.Three,
			Text = text
		};
		if(!String.IsNullOrWhiteSpace(iconName))
			item.SetImage(iconName, ImageAlign.Left | ImageAlign.CenterV);
		if(!String.IsNullOrWhiteSpace(accelerator))
			item.SetAccelerator(accelerator);

		OnAddItem(item);

		return item;
	}

	/// <summary>
	/// Adds a menu item.
	/// </summary>
	/// <param name="item">Item.</param>
	public virtual void AddItem(MenuItem item) {
		item.Parent = this;

		item.Padding = Padding.Three;

		OnAddItem(item);
	}
	public MenuItem AddItemPath(string text) {
		return AddItemPath(text, String.Empty);
	}

	public MenuItem AddItemPath(string text, string iconName, string accelerator = "") {
		MenuItem item = new(this) {
			Text = text,
			Padding = Padding.Three
		};
		if(!String.IsNullOrWhiteSpace(iconName))
			item.SetImage(iconName, ImageAlign.Left | ImageAlign.CenterV);
		if(!String.IsNullOrWhiteSpace(accelerator))
			item.SetAccelerator(accelerator);

		AddItemPath(item);
		return item;
	}

	public void AddItemPath(MenuItem item) {

		string[] path = item.Text.Split('\\', '/');
		Menu m = this;
		for(int i = 0; i < path.Length - 1; i++) {
			MenuItem[] items = m.FindItems(path[i]);
			if(items.Length == 0) {
				m = m.AddItem(path[i]).Menu;
			} else if(items.Length == 1) {
				m = items[0].Menu;
			} else {
				for(int j = 0; j < items.Length; j++) {
					if(items[j].Parent == m) m = items[j].Menu;
				}
			}
		}
		item.Text = path.Last();
		m.AddItem(item);
	}

	/// <summary>
	/// Add item handler.
	/// </summary>
	/// <param name="item">Item added.</param>
	protected virtual void OnAddItem(MenuItem item) {
		item.TextPadding = new Padding(IconMarginDisabled ? 0 : 24, 0, 16, 0);
		item.Alignment = Alignment.CenterV | Alignment.Left;
		item.HoverEnter += OnHoverItem;
	}

	/// <summary>
	/// Closes all submenus.
	/// </summary>
	public virtual void CloseAll() {
		foreach(var child in Children) {
			if(child is MenuItem menuItem)
				menuItem.CloseMenu();
		}
	}

	/// <summary>
	/// Indicates whether any (sub)menu is open.
	/// </summary>
	/// <returns></returns>
	public virtual bool IsMenuOpen() {
		return Children.Any(child => { if(child is MenuItem menuItem) return menuItem.IsMenuOpen; return false; });
	}

	/// <summary>
	/// Mouse hover handler.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnHoverItem(ControlBase control, EventArgs args) {
		if(!ShouldHoverOpenMenu) return;

		if(control is not MenuItem item) return;
		if(item.IsMenuOpen) return;

		CloseAll();
		item.OpenMenu();
	}

	/// <summary>
	/// Closes the current menu.
	/// </summary>
	public virtual void Close() {
		IsCollapsed = true;
		if(DeleteOnClose) {
			DelayedDelete();
		}
	}
	/// <summary>
	/// Finds all items by name in current menu.
	/// </summary>
	public MenuItem[] FindItems(string name) {
		List<MenuItem> mi = new();
		for(int i = 0; i < Children.Count; i++) {
			if(Children[i]is MenuItem menuItem) {
				if(menuItem.Text == name)
					mi.Add(menuItem);
			}
		}
		return mi.ToArray();
	}

	/// <summary>
	/// Closes all submenus and the current menu.
	/// </summary>
	public override void CloseMenus() {
		base.CloseMenus();
		CloseAll();
		Close();
	}

	/// <summary>
	/// Adds a divider menu item.
	/// </summary>
	public virtual void AddDivider() {
		_ = new MenuDivider(this) {
			Margin = new Margin(IconMarginDisabled ? 0 : 24, 0, 4, 0)
		};
	}

	/// <summary>
	/// Removes all items.
	/// </summary>
	public void RemoveAll() {
		layout.DeleteAllChildren();
	}
}