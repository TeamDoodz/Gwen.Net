using System;
using Gwen.Net.Input;

namespace Gwen.Net.Control.Internal;
/// <summary>
/// Tab header.
/// </summary>
public class TabButton : Button {
	private ControlBase? page;
	private TabControl? control;

	/// <summary>
	/// Indicates whether the tab is active.
	/// </summary>
	public bool IsActive { get { return page != null && page.IsVisible; } }

	// todo: remove public access
	public TabControl? TabControl {
		get { return control; }
		set {
			if(value == control) return;
			if(control != null)
				control.OnLoseTab(this);
			control = value;
		}
	}

	/// <summary>
	/// Interior of the tab.
	/// </summary>
	public ControlBase? Page { get { return page; } set { page = value; } }

	/// <summary>
	/// Determines whether the control should be clipped to its bounds while rendering.
	/// </summary>
	protected override bool ShouldClip {
		get { return false; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TabButton"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public TabButton(ControlBase parent)
		: base(parent) {
		DragAndDrop_SetPackage(true, "TabButtonMove");
		Alignment = Alignment.Top | Alignment.Left;
		TextPadding = new Padding(5, 3, 3, 3);
		Padding = Padding.Two;
		KeyboardInputEnabled = true;
	}

	public override void DragAndDrop_StartDragging(DragDrop.Package package, int x, int y) {
		IsCollapsed = true;
	}

	public override void DragAndDrop_EndDragging(bool success, int x, int y) {
		IsCollapsed = false;
		IsDepressed = false;
	}

	public override bool DragAndDrop_ShouldStartDrag() {
		return control?.AllowReorder ?? false;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		if(control == null) {
			return;
		}
		skin.DrawTabButton(this, IsActive, control.TabStrip.Dock);
	}

	/// <summary>
	/// Handler for Down Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyDown(bool down) {
		OnKeyRight(down);
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
		OnKeyLeft(down);
		return true;
	}

	/// <summary>
	/// Handler for Right Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyRight(bool down) {
		if(Parent == null) {
			return true;
		}

		if(down) {
			var count = Parent.Children.Count;
			int me = Parent.Children.IndexOf(this);
			if(me + 1 < count) {
				var nextTab = Parent.Children[me + 1];
				TabControl?.OnTabPressed(nextTab, EventArgs.Empty);
				InputHandler.KeyboardFocus = nextTab;
			}
		}

		return true;
	}

	/// <summary>
	/// Handler for Left Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyLeft(bool down) {
		if(Parent == null) {
			return true;
		}

		if(down) {
			var count = Parent.Children.Count;
			int me = Parent.Children.IndexOf(this);
			if(me - 1 >= 0) {
				var prevTab = Parent.Children[me - 1];
				TabControl?.OnTabPressed(prevTab, EventArgs.Empty);
				InputHandler.KeyboardFocus = prevTab;
			}
		}

		return true;
	}

	/// <summary>
	/// Updates control colors.
	/// </summary>
	public override void UpdateColors() {
		if(IsActive) {
			if(IsDisabled) {
				TextColor = Skin.Colors.Tab.Active.Disabled;
				return;
			}
			if(IsDepressed) {
				TextColor = Skin.Colors.Tab.Active.Down;
				return;
			}
			if(IsHovered) {
				TextColor = Skin.Colors.Tab.Active.Hover;
				return;
			}

			TextColor = Skin.Colors.Tab.Active.Normal;
		}

		if(IsDisabled) {
			TextColor = Skin.Colors.Tab.Inactive.Disabled;
			return;
		}
		if(IsDepressed) {
			TextColor = Skin.Colors.Tab.Inactive.Down;
			return;
		}
		if(IsHovered) {
			TextColor = Skin.Colors.Tab.Inactive.Hover;
			return;
		}

		TextColor = Skin.Colors.Tab.Inactive.Normal;
	}
}