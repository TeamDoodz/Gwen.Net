using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// CollapsibleCategory control. Used in CollapsibleList.
/// </summary>
public class CollapsibleCategory : ControlBase {
	private readonly CategoryHeaderButton headerButton;
	private readonly CollapsibleList list;

	/// <summary>
	/// Header text.
	/// </summary>
	public string Text { get { return headerButton.Text; } set { headerButton.Text = value; } }

	/// <summary>
	/// Determines whether the category is collapsed (closed).
	/// </summary>
	public bool IsCategoryCollapsed { get { return headerButton.ToggleState; } set { headerButton.ToggleState = value; } }

	/// <summary>
	/// Invoked when an entry has been selected.
	/// </summary>
	public event GwenEventHandler<ItemSelectedEventArgs>? Selected;

	/// <summary>
	/// Invoked when the category collapsed state has been changed (header button has been pressed).
	/// </summary>
	public event GwenEventHandler<EventArgs>? Collapsed;

	/// <summary>
	/// Initializes a new instance of the <see cref="CollapsibleCategory"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public CollapsibleCategory(CollapsibleList parent) : base(parent) {
		headerButton = new CategoryHeaderButton(this);
		headerButton.Text = "Category Title";
		headerButton.Toggled += OnHeaderToggle;

		list = parent;

		Padding = new Padding(1, 0, 1, 2);
	}

	/// <summary>
	/// Gets the selected entry.
	/// </summary>
	public Button? GetSelectedButton() {
		foreach(ControlBase child in Children) {
			if(child is not CategoryButton button) {
				continue;
			}

			if(button.ToggleState)
				return button;
		}

		return null;
	}

	/// <summary>
	/// Handler for header button toggle event.
	/// </summary>
	/// <param name="control">Source control.</param>
	protected virtual void OnHeaderToggle(ControlBase control, EventArgs args) {
		Invalidate();

		if(Collapsed != null)
			Collapsed.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Handler for Selected event.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnSelected(ControlBase control, EventArgs args) {
		if(control is not CategoryButton child) return;

		if(list != null) {
			list.UnselectAll();
		} else {
			UnselectAll();
		}

		child.ToggleState = true;

		Selected?.Invoke(this, new ItemSelectedEventArgs(control));
	}

	/// <summary>
	/// Adds a new entry.
	/// </summary>
	/// <param name="name">Entry name (displayed).</param>
	/// <returns>Newly created control.</returns>
	public Button Add(string name) {
		CategoryButton button = new CategoryButton(this);
		button.Text = name;
		button.Padding = new Padding(5, 2, 2, 2);
		button.Clicked += OnSelected;

		Invalidate();

		return button;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawCategoryInner(this, headerButton.ActualHeight, headerButton.ToggleState);
		base.Render(skin);
	}

	/// <summary>
	/// Unselects all entries.
	/// </summary>
	public void UnselectAll() {
		foreach(ControlBase child in Children) {
			if(child is not CategoryButton button)
				continue;

			button.ToggleState = false;
		}
	}

	protected override Size Measure(Size availableSize) {
		Size headerSize = headerButton.DoMeasure(availableSize);

		if(IsCategoryCollapsed) {
			return headerSize;
		} else {
			int width = headerSize.Width;
			int height = headerSize.Height + Padding.Top + Padding.Bottom;

			foreach(ControlBase child in Children) {
				if(child is not CategoryButton button)
					continue;

				Size size = child.DoMeasure(availableSize);
				if(size.Width > width)
					width = child.Width;
				height += size.Height;
			}

			width += Padding.Left + Padding.Right;

			return new Size(width, height);
		}
	}

	protected override Size Arrange(Size finalSize) {
		headerButton.DoArrange(new Rectangle(0, 0, finalSize.Width, headerButton.MeasuredSize.Height));

		if(IsCategoryCollapsed) {
			return new Size(finalSize.Width, headerButton.MeasuredSize.Height);
		} else {
			int y = headerButton.MeasuredSize.Height + Padding.Top;
			int width = finalSize.Width - Padding.Left - Padding.Right;
			bool b = true;

			foreach(ControlBase child in Children) {
				if(child is not CategoryButton button)
					continue;

				button.m_Alt = b;
				button.UpdateColors();
				b = !b;

				child.DoArrange(new Rectangle(Padding.Left, y, width, child.MeasuredSize.Height));
				y += child.MeasuredSize.Height;
			}

			y += Padding.Bottom;

			return new Size(finalSize.Width, y);
		}
	}
}