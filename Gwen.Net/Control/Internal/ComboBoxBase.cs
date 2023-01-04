using System;

namespace Gwen.Net.Control.Internal;

public abstract class ComboBoxBase : ControlBase {
	private readonly Menu menu;
	private MenuItem? selectedItem;

	/// <summary>
	/// Invoked when the selected item has changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<ItemSelectedEventArgs>? ItemSelected;

	/// <summary>
	/// Index of the selected radio button.
	/// </summary>
	public int SelectedIndex { 
		get => selectedItem == null ? -1 : Children.IndexOf(selectedItem);
		set { 
			SetSelection(value); 
		} 
	}

	/// <summary>
	/// Selected item.
	/// </summary>
	/// <remarks>Not just String property, because items also have internal names.</remarks>
	public MenuItem? SelectedItem {
		get => selectedItem;
		set {
			if(value != null && value.Parent == menu) {
				selectedItem = value;
				OnItemSelected(selectedItem, new ItemSelectedEventArgs(value));
			}
		}
	}

	/// <summary>
	/// Indicates whether the combo menu is open.
	/// </summary>
	public bool IsOpen { get { return menu != null && !menu.IsCollapsed; } }

	internal override bool IsMenuComponent {
		get { return true; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ComboBoxBase"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ComboBoxBase(ControlBase? parent)
		: base(parent) {
		menu = new Menu(GetCanvas());
		menu.IconMarginDisabled = true;
		menu.IsTabable = false;
		menu.HorizontalAlignment = HorizontalAlignment.Stretch;

		IsTabable = true;
		KeyboardInputEnabled = true;
	}

	/// <summary>
	/// Adds a new item.
	/// </summary>
	/// <param name="label">Item label (displayed).</param>
	/// <param name="name">Item name.</param>
	/// <returns>Newly created control.</returns>
	public virtual MenuItem AddItem(string label, string name = "", object? UserData = null) {
		MenuItem item = menu.AddItem(label, String.Empty);
		item.Name = name;
		item.Selected += OnItemSelected;
		item.UserData = UserData;

		if(selectedItem == null)
			OnItemSelected(item, new ItemSelectedEventArgs(null));

		return item;
	}

	/// <summary>
	/// Adds an item.
	/// </summary>
	/// <param name="item">Item.</param>
	public virtual void AddItem(MenuItem item) {
		item.Parent = menu;

		menu.AddItem(item);
		item.Selected += OnItemSelected;

		if(selectedItem == null)
			OnItemSelected(item, new ItemSelectedEventArgs(null));
	}

	public override void Disable() {
		base.Disable();
		GetCanvas().CloseMenus();
	}

	/// <summary>
	/// Removes all items.
	/// </summary>
	public virtual void RemoveAll() {
		if(menu != null)
			menu.RemoveAll();

		selectedItem = null;
	}

	/// <summary>
	/// Internal handler for item selected event.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnItemSelected(ControlBase control, ItemSelectedEventArgs args) {
		if(!IsDisabled) {
			//Convert selected to a menu item
			if(control is not MenuItem item) return;

			selectedItem = item;
			menu.IsCollapsed = true;

			if(ItemSelected != null)
				ItemSelected.Invoke(this, args);

			Focus();
		}
	}

	/// <summary>
	/// Opens the combo.
	/// </summary>
	public virtual void Open() {
		if(!IsDisabled) {
			GetCanvas().CloseMenus();

			if(null == menu) return;

			Point p = LocalPosToCanvas(Point.Zero);

			menu.Width = this.ActualWidth;

			int canvasHeight = GetCanvas().ActualHeight;
			if(p.Y > canvasHeight - 100) {
				// We need to do layout for the menu here to know the height of it.
				menu.DoArrange(new Rectangle(Point.Zero, menu.DoMeasure(Size.Infinity)));
				menu.Position = new Point(p.X, p.Y - menu.ActualHeight);
			} else {
				menu.Position = new Point(p.X, p.Y + ActualHeight);
			}

			menu.Show();
			menu.BringToFront();
		}
	}

	/// <summary>
	/// Closes the combo.
	/// </summary>
	public virtual void Close() {
		if(menu == null)
			return;

		menu.Collapse();
	}

	/// <summary>
	/// Handler for Down Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyDown(bool down) {
		if(selectedItem == null) {
			if(menu.Children.Count == 0) {
				return true;
			}
			OnItemSelected(this, new ItemSelectedEventArgs(menu.Children[0]));
			return true;
		}

		if(down) {
			var it = menu.Children.IndexOf(selectedItem);
			if(it + 1 < menu.Children.Count)
				OnItemSelected(this, new ItemSelectedEventArgs(menu.Children[it + 1]));
		}
		return true;
	}

	/// <summary>
	/// Handler for Up Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyUp(bool down) {
		if(selectedItem == null) {
			if(menu.Children.Count == 0) {
				return true;
			}
			OnItemSelected(this, new ItemSelectedEventArgs(menu.Children[0]));
			return true;
		}

		if(down) {
			var it = menu.Children.LastIndexOf(selectedItem);
			if(it - 1 >= 0)
				OnItemSelected(this, new ItemSelectedEventArgs(menu.Children[it - 1]));
		}
		return true;
	}

	/// <summary>
	/// Selects the specified option.
	/// </summary>
	/// <param name="index">Option to select.</param>
	public void SetSelection(int index) {
		if(index < 0 || index >= Children.Count)
			return;

		SelectedItem = Children[index] as MenuItem;
	}

	/// <summary>
	/// Selects the first menu item with the given text it finds. 
	/// If a menu item can not be found that matches input, nothing happens.
	/// </summary>
	/// <param name="text">The label to look for, this is what is shown to the user.</param>
	public void SelectByText(string text) {
		foreach(ControlBase control in menu.Children) {
			if(control is not MenuItem item) {
				continue;
			}
			if(item.Text == text) {
				SelectedItem = item;
				return;
			}
		}
	}

	/// <summary>
	/// Selects the first menu item with the given internal name it finds.
	/// If a menu item can not be found that matches input, nothing happens.
	/// </summary>
	/// <param name="name">The internal name to look for. To select by what is displayed to the user, use "SelectByText".</param>
	public void SelectByName(string name) {
		foreach(MenuItem item in menu.Children) {
			if(item.Name == name) {
				SelectedItem = item;
				return;
			}
		}
	}

	/// <summary>
	/// Selects the first menu item with the given user data it finds.
	/// If a menu item can not be found that matches input, nothing happens.
	/// </summary>
	/// <param name="userdata">The UserData to look for. The equivalency check uses "param.Equals(item.UserData)".
	/// If null is passed in, it will look for null/unset UserData.</param>
	public void SelectByUserData(object userdata) {
		foreach(MenuItem item in menu.Children) {
			if(userdata == null) {
				if(item.UserData == null) {
					SelectedItem = item;
					return;
				}
			} else if(userdata.Equals(item.UserData)) {
				SelectedItem = item;
				return;
			}
		}
	}
}