﻿using System;
using Gwen.Net.DragDrop;

namespace Gwen.Net.Control.Internal;
/// <summary>
/// Tab strip - groups TabButtons and allows reordering.
/// </summary>
public class TabStrip : Layout.StackLayout {
	private ControlBase? tabDragControl;
	private bool allowReorder;
	private int scrollOffset;
	private Size totalSize;

	/// <summary>
	/// Determines whether it is possible to reorder tabs by mouse dragging.
	/// </summary>
	public bool AllowReorder { get { return allowReorder; } set { allowReorder = value; } }

	internal int ScrollOffset {
		get { return scrollOffset; }
		set { SetScrollOffset(value); }
	}

	internal Size TotalSize { get { return totalSize; } }

	/// <summary>
	/// Determines whether the control should be clipped to its bounds while rendering.
	/// </summary>
	protected override bool ShouldClip {
		get { return false; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TabStrip"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public TabStrip(ControlBase parent)
		: base(parent) {
		allowReorder = false;
		scrollOffset = 0;
	}

	/// <summary>
	/// Strip position (top/left/right/bottom).
	/// </summary>
	public Dock StripPosition {
		get { return Dock; }
		set {
			Dock = value;

			switch(value) {
				case Dock.Top:
					Padding = new Padding(5, 0, 0, 0);
					Horizontal = true;
					break;
				case Dock.Left:
					Padding = new Padding(0, 5, 0, 0);
					Horizontal = false;
					break;
				case Dock.Bottom:
					Padding = new Padding(5, 0, 0, 0);
					Horizontal = true;
					break;
				case Dock.Right:
					Padding = new Padding(0, 5, 0, 0);
					Horizontal = false;
					break;
			}
		}
	}

	private void SetScrollOffset(int value) {
		for(int i = 0; i < Children.Count; i++) {
			if(i < value && i < (Children.Count - 1))
				Children[i].Collapse(true, false);
			else
				Children[i].Collapse(false, false);
		}

		scrollOffset = value;
		scrollOffset = Math.Min(scrollOffset, Children.Count - 1);
		scrollOffset = Math.Max(scrollOffset, 0);

		Invalidate();
	}

	protected override Size Measure(Size availableSize) {
		int num = 0;
		foreach(var child in Children) {
			if(child is not TabButton button) continue;

			Margin m = new Margin();
			int notFirst = num > 0 ? -1 : 0;

			switch(this.StripPosition) {
				case Dock.Top:
				case Dock.Bottom:
					m.Left = notFirst;
					break;
				case Dock.Left:
				case Dock.Right:
					m.Top = notFirst;
					break;
			}

			button.Margin = m;
			num++;
		}

		totalSize = base.Measure(Size.Infinity);

		return totalSize;
	}

	public override void DragAndDrop_HoverEnter(Package p, int x, int y) {
		if(tabDragControl != null) {
			throw new InvalidOperationException("ERROR! TabStrip::DragAndDrop_HoverEnter");
		}

		tabDragControl = new Highlight(GetCanvas());
		tabDragControl.MouseInputEnabled = false;
		tabDragControl.Size = new Size(3, ActualHeight);
		Invalidate();
	}

	public override void DragAndDrop_HoverLeave(Package p) {
		if(tabDragControl != null) {
			tabDragControl.Parent?.RemoveChild(tabDragControl, false); // [omeg] need to do that explicitely
			tabDragControl.Dispose();
		}
		tabDragControl = null;
	}

	public override void DragAndDrop_Hover(Package p, int x, int y) {
		if(tabDragControl == null) {
			return;
		}

		Point localPos = CanvasPosToLocal(new Point(x, y));

		ControlBase? droppedOn = GetControlAt(localPos.X, localPos.Y);
		if(droppedOn != null && droppedOn != this) {
			Point dropPos = droppedOn.CanvasPosToLocal(new Point(x, y));
			tabDragControl.BringToFront();
			int pos = droppedOn.ActualLeft - 1;

			if(dropPos.X > droppedOn.ActualWidth / 2)
				pos += droppedOn.ActualWidth - 1;

			Point canvasPos = LocalPosToCanvas(new Point(pos, 0));
			tabDragControl.MoveTo(canvasPos.X, canvasPos.Y);
		} else {
			tabDragControl.BringToFront();
		}
	}

	public override bool DragAndDrop_HandleDrop(Package p, int x, int y) {
		if(DragAndDrop.SourceControl == null) {
			return false;
		}

		Point LocalPos = CanvasPosToLocal(new Point(x, y));

		if(Parent is TabControl tabControl && DragAndDrop.SourceControl is TabButton button) {
			if(button.TabControl != tabControl) {
				// We've moved tab controls!
				tabControl.AddPage(button);
			}
		}

		ControlBase? droppedOn = GetControlAt(LocalPos.X, LocalPos.Y);
		if(droppedOn != null && droppedOn != this) {
			Point dropPos = droppedOn.CanvasPosToLocal(new Point(x, y));
			DragAndDrop.SourceControl.BringNextToControl(droppedOn, dropPos.X > droppedOn.ActualWidth / 2);
		} else {
			DragAndDrop.SourceControl.BringToFront();
		}
		return true;
	}

	public override bool DragAndDrop_CanAcceptPackage(Package p) {
		if(!allowReorder)
			return false;

		if(p.Name == "TabButtonMove")
			return true;

		return false;
	}
}