using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Docked tab control.
/// </summary>
public class DockedTabControl : TabControl {
	private readonly TabTitleBar titleBar;

	/// <summary>
	/// Determines whether the title bar is visible.
	/// </summary>
	public bool TitleBarVisible { get { return !titleBar.IsCollapsed; } set { titleBar.IsCollapsed = !value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="DockedTabControl"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public DockedTabControl(ControlBase? parent)
		: base(parent) {
		Dock = Dock.Fill;

		titleBar = new TabTitleBar(this);
		titleBar.Dock = Dock.Top;
		titleBar.IsCollapsed = true;

		AllowReorder = true;
	}

	protected override Size Measure(Size availableSize) {
		TabStrip.Collapse(TabCount <= 1, false);
		UpdateTitleBar();

		return base.Measure(availableSize);
	}

	private void UpdateTitleBar() {
		if(CurrentButton == null)
			return;

		titleBar.UpdateFromTab(CurrentButton);
	}

	public override void DragAndDrop_StartDragging(DragDrop.Package package, int x, int y) {
		base.DragAndDrop_StartDragging(package, x, y);

		IsCollapsed = true;
		// This hiding our parent thing is kind of lousy.
		if(Parent != null) {
			Parent.IsCollapsed = true;
		}
	}

	public override void DragAndDrop_EndDragging(bool success, int x, int y) {
		IsCollapsed = false;

		if(!success && Parent != null) {
			Parent.IsCollapsed = false;
		}
	}

	public void MoveTabsTo(DockedTabControl target) {
		var children = TabStrip.Children.ToArray(); // copy because collection will be modified
		foreach(ControlBase child in children) {
			if(child is not TabButton button)
				continue;
			target.AddPage(button);
		}
		Invalidate();
	}
}