using System;
using Gwen.Net.Control.Layout;

namespace Gwen.Net.Control;
/// <summary>
/// CollapsibleList control. Groups CollapsibleCategory controls.
/// </summary>
public class CollapsibleList : ScrollControl {
	private VerticalLayout items;

	/// <summary>
	/// Invoked when an entry has been selected.
	/// </summary>
	public event GwenEventHandler<ItemSelectedEventArgs>? ItemSelected;

	/// <summary>
	/// Invoked when a category collapsed state has been changed (header button has been pressed).
	/// </summary>
	public event GwenEventHandler<EventArgs>? CategoryCollapsed;

	/// <summary>
	/// Initializes a new instance of the <see cref="CollapsibleList"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public CollapsibleList(ControlBase? parent)
		: base(parent) {
		Padding = Padding.One;

		MouseInputEnabled = true;
		EnableScroll(false, true);
		AutoHideBars = true;

		items = new VerticalLayout(this);
	}

	// todo: iterator, make this as function? check if works

	/// <summary>
	/// Selected entry.
	/// </summary>
	public Button? GetSelectedButton() {
		foreach(ControlBase child in Children) {
			if(child is not CollapsibleCategory cat) {
				continue;
			}

			Button? button = cat.GetSelectedButton();

			if(button != null)
				return button;
		}

		return null;
	}

	/// <summary>
	/// Adds a category to the list.
	/// </summary>
	/// <param name="category">Category control to add.</param>
	protected virtual void Add(CollapsibleCategory category) {
		category.Parent = items;
		category.Margin = new Margin(1, 1, 1, 0);
		category.Selected += OnCategorySelected;
		category.Collapsed += OnCategoryCollapsed;

		Invalidate();
	}

	/// <summary>
	/// Adds a new category to the list.
	/// </summary>
	/// <param name="categoryName">Name of the category.</param>
	/// <returns>Newly created control.</returns>
	public virtual CollapsibleCategory Add(string categoryName, string name = "", object? userData = null) {
		CollapsibleCategory cat = new CollapsibleCategory(this);
		cat.Text = categoryName;
		cat.Name = name;
		cat.UserData = userData;
		Add(cat);
		return cat;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawCategoryHolder(this);
	}

	/// <summary>
	/// Unselects all entries.
	/// </summary>
	public virtual void UnselectAll() {
		foreach(ControlBase child in items.Children) {
			if(child is not CollapsibleCategory cat)
				continue;

			cat.UnselectAll();
		}
	}

	/// <summary>
	/// Handler for ItemSelected event.
	/// </summary>
	/// <param name="control">Event source: <see cref="CollapsibleList"/>.</param>
	/// <param name="args">Event args.</param>
	protected virtual void OnCategorySelected(ControlBase control, EventArgs args) {
		if(control is not CollapsibleCategory cat) return;

		ItemSelected?.Invoke(this, new ItemSelectedEventArgs(cat));
	}

	/// <summary>
	/// Handler for category collapsed event.
	/// </summary>
	/// <param name="control">Event source: <see cref="CollapsibleCategory"/>.</param>
	/// <param name="args">Event args.</param>
	protected virtual void OnCategoryCollapsed(ControlBase control, EventArgs args) {
		if(control is not CollapsibleCategory cat) return;

		CategoryCollapsed?.Invoke(control, EventArgs.Empty);
	}
}