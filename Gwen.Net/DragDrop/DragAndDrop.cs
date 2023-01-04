using System;
using Gwen.Net.Control;
using Gwen.Net.Input;

namespace Gwen.Net.DragDrop;
/// <summary>
/// Drag and drop handling.
/// </summary>
public static class DragAndDrop {
	public static Package? CurrentPackage { get; set; }
	public static ControlBase? HoveredControl { get; set; }
	public static ControlBase? SourceControl { get; set; }

	private static ControlBase? lastPressedControl;
	private static ControlBase? newHoveredControl;
	private static Point lastPressedPos;
	private static int mouseX;
	private static int mouseY;

	private static bool OnDrop(int x, int y) {
		if(CurrentPackage == null || SourceControl == null) {
			return false;
		}

		bool success = false;

		if(HoveredControl != null) {
			HoveredControl.DragAndDrop_HoverLeave(CurrentPackage);
			success = HoveredControl.DragAndDrop_HandleDrop(CurrentPackage, x, y);
		}

		// Report back to the source control, to tell it if we've been successful.
		SourceControl.DragAndDrop_EndDragging(success, x, y);

		CurrentPackage = null;
		SourceControl = null;
		HoveredControl = null;

		return true;
	}

	private static bool ShouldStartDraggingControl(int x, int y) {
		// We're not holding a control down..
		if(lastPressedControl == null)
			return false;

		// Not been dragged far enough
		int length = Math.Abs(x - lastPressedPos.X) + Math.Abs(y - lastPressedPos.Y);
		if(length < 5)
			return false;

		// Create the dragging package

		CurrentPackage = lastPressedControl.DragAndDrop_GetPackage(lastPressedPos.X, lastPressedPos.Y);

		// We didn't create a package!
		if(CurrentPackage == null) {
			lastPressedControl = null;
			SourceControl = null;
			HoveredControl = null;
			return false;
		}

		// Now we're dragging something!
		SourceControl = lastPressedControl;
		InputHandler.MouseFocus = null;
		lastPressedControl = null;
		CurrentPackage.DrawControl = null;

		// Some controls will want to decide whether they should be dragged at that moment.
		// This function is for them (it defaults to true)
		if(!SourceControl.DragAndDrop_ShouldStartDrag()) {
			SourceControl = null;
			CurrentPackage = null;
			HoveredControl = null;
			return false;
		}

		SourceControl.DragAndDrop_StartDragging(CurrentPackage, lastPressedPos.X, lastPressedPos.Y);

		return true;
	}

	private static void UpdateHoveredControl(ControlBase control, int x, int y) {
		if(CurrentPackage == null) {
			return;
		}

		//
		// We use this global variable to represent our hovered control
		// That way, if the new hovered control gets deleted in one of the
		// Hover callbacks, we won't be left with a hanging pointer.
		// This isn't ideal - but it's minimal.
		//
		newHoveredControl = control;

		// Check to see if the new potential control can accept this type of package.
		// If not, ignore it and show an error cursor.
		while(newHoveredControl != null && !newHoveredControl.DragAndDrop_CanAcceptPackage(CurrentPackage)) {
			// We can't drop on this control, so lets try to drop
			// onto its parent..
			newHoveredControl = newHoveredControl.Parent;

			// Its parents are dead. We can't drop it here.
			// Show the NO WAY cursor.
			if(newHoveredControl == null) {
				Platform.GwenPlatform.SetCursor(Cursor.No);
			}
		}

		// Nothing to change..
		if(HoveredControl == newHoveredControl)
			return;

		// We changed - tell the old hovered control that it's no longer hovered.
		HoveredControl?.DragAndDrop_HoverLeave(CurrentPackage);

		// If we're hovering where the control came from, just forget it.
		// By changing it to null here we're not going to show any error cursors
		// it will just do nothing if you drop it.
		if(newHoveredControl == SourceControl)
			newHoveredControl = null;

		// Become out new hovered control
		HoveredControl = newHoveredControl;

		// If we exist, tell us that we've started hovering.
		if(HoveredControl != null) {
			HoveredControl.DragAndDrop_HoverEnter(CurrentPackage, x, y);
		}

		newHoveredControl = null;
	}

	public static bool Start(ControlBase control, Package package) {
		if(CurrentPackage != null) {
			return false;
		}

		CurrentPackage = package;
		SourceControl = control;
		return true;
	}

	public static bool OnMouseButton(ControlBase hoveredControl, int x, int y, bool down) {
		if(!down) {
			lastPressedControl = null;

			// Not carrying anything, allow normal actions
			if(CurrentPackage == null)
				return false;

			// We were carrying something, drop it.
			OnDrop(x, y);
			return true;
		}

		if(hoveredControl == null)
			return false;
		if(!hoveredControl.DragAndDrop_Draggable())
			return false;

		// Store the last clicked on control. Don't do anything yet, 
		// we'll check it in OnMouseMoved, and if it moves further than
		// x pixels with the mouse down, we'll start to drag.
		lastPressedPos = new Point(x, y);
		lastPressedControl = hoveredControl;

		return false;
	}

	public static void OnMouseMoved(ControlBase hoveredControl, int x, int y) {
		// Always keep these up to date, they're used to draw the dragged control.
		mouseX = x;
		mouseY = y;

		// If we're not carrying anything, then check to see if we should
		// pick up from a control that we're holding down. If not, then forget it.
		if(CurrentPackage == null && !ShouldStartDraggingControl(x, y))
			return;

		// Swap to this new hovered control and notify them of the change.
		UpdateHoveredControl(hoveredControl, x, y);

		if(HoveredControl == null)
			return;

		// Update the hovered control every mouse move, so it can show where
		// the dropped control will land etc..
		if(CurrentPackage != null) {
			HoveredControl.DragAndDrop_Hover(CurrentPackage, x, y);
		}

		// Override the cursor - since it might have been set my underlying controls
		// Ideally this would show the 'being dragged' control. TODO
		Platform.GwenPlatform.SetCursor(Cursor.Normal);

		hoveredControl.Redraw();
	}

	public static void RenderOverlay(Canvas canvas, Skin.SkinBase skin) {
		if(CurrentPackage == null)
			return;
		if(CurrentPackage.DrawControl == null)
			return;

		Point old = skin.Renderer.RenderOffset;

		if(SourceControl != null) {
			skin.Renderer.AddRenderOffset(new Rectangle(
				mouseX - SourceControl.ActualLeft - CurrentPackage.HoldOffset.X,
				mouseY - SourceControl.ActualTop - CurrentPackage.HoldOffset.Y, 0, 0));
		}
		CurrentPackage.DrawControl.DoRender(skin);

		skin.Renderer.RenderOffset = old;
	}

	public static void ControlDeleted(ControlBase control) {
		if(SourceControl == control) {
			SourceControl = null;
			CurrentPackage = null;
			HoveredControl = null;
			lastPressedControl = null;
		}

		if(lastPressedControl == control)
			lastPressedControl = null;

		if(HoveredControl == control)
			HoveredControl = null;

		if(newHoveredControl == control)
			newHoveredControl = null;
	}
}