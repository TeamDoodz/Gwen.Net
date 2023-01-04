using System;

namespace Gwen.Net.Control.Internal;
/// <summary>
/// Grab point for resizing.
/// </summary>
public class Resizer : Dragger {
	private Dock resizeDir;

	/// <summary>
	/// Invoked when the control has been resized.
	/// </summary>
	public event GwenEventHandler<EventArgs>? Resized;

	/// <summary>
	/// Initializes a new instance of the <see cref="Resizer"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public Resizer(ControlBase parent)
		: base(parent) {
		resizeDir = Dock.Left;
		MouseInputEnabled = true;
		Target = parent;
	}

	/// <summary>
	/// Handler invoked on mouse moved event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="dx">X change.</param>
	/// <param name="dy">Y change.</param>
	protected override void OnMouseMoved(int x, int y, int dx, int dy) {
		if(null == Target) return;
		if(!IsHeld) return;

		Rectangle oldBounds = Target.Bounds;
		Rectangle bounds = Target.Bounds;

		Size min = Target.MinimumSize;

		Point delta = Target.LocalPosToCanvas(holdPos);
		delta.X -= x;
		delta.Y -= y;

		if(0 != (resizeDir & Dock.Left)) {
			bounds.X -= delta.X;
			bounds.Width += delta.X;

			if(bounds.X < 0) {
				bounds.Width += bounds.X;
				bounds.X = 0;
			}

			// Conform to minimum size here so we don't
			// go all weird when we snap it in the base conrt

			if(bounds.Width < min.Width) {
				int diff = min.Width - bounds.Width;
				bounds.Width += diff;
				bounds.X -= diff;
			}

			Target.Left = bounds.Left;
			Target.Width = bounds.Width;
		}

		if(0 != (resizeDir & Dock.Top)) {
			bounds.Y -= delta.Y;
			bounds.Height += delta.Y;

			if(bounds.Y < 0) {
				bounds.Height += bounds.Y;
				bounds.Y = 0;
			}

			// Conform to minimum size here so we don't
			// go all weird when we snap it in the base conrt

			if(bounds.Height < min.Height) {
				int diff = min.Height - bounds.Height;
				bounds.Height += diff;
				bounds.Y -= diff;
			}

			Target.Top = bounds.Top;
			Target.Height = bounds.Height;
		}

		if(0 != (resizeDir & Dock.Right)) {
			bounds.Width -= delta.X;

			if(bounds.Width < min.Width) bounds.Width = min.Width;

			holdPos.X += bounds.Width - oldBounds.Width;

			Target.Left = bounds.Left;
			Target.Width = bounds.Width;
		}

		if(0 != (resizeDir & Dock.Bottom)) {
			bounds.Height -= delta.Y;

			if(bounds.Height < min.Height) bounds.Height = min.Height;

			holdPos.Y += bounds.Height - oldBounds.Height;

			Target.Top = bounds.Top;
			Target.Height = bounds.Height;
		}

		// Lets set quickly new bounds and let the layout measure and arrange child controls later
		Target.SetBounds(bounds);
		// Set bounds that are checked by SetBounds() implementations
		if(!Util.IsIgnore(Target.Width)) Target.Width = Target.Bounds.Width;
		if(!Util.IsIgnore(Target.Height)) Target.Height = Target.Bounds.Height;

		Target.Invalidate();

		if(Resized != null)
			Resized.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Gets or sets the sizing direction.
	/// </summary>
	public Dock ResizeDir {
		set {
			resizeDir = value;

			if((0 != (value & Dock.Left) && 0 != (value & Dock.Top)) || (0 != (value & Dock.Right) && 0 != (value & Dock.Bottom))) {
				Cursor = Cursor.SizeNWSE;
				return;
			}
			if((0 != (value & Dock.Right) && 0 != (value & Dock.Top)) || (0 != (value & Dock.Left) && 0 != (value & Dock.Bottom))) {
				Cursor = Cursor.SizeNESW;
				return;
			}
			if(0 != (value & Dock.Right) || 0 != (value & Dock.Left)) {
				Cursor = Cursor.SizeWE;
				return;
			}
			if(0 != (value & Dock.Top) || 0 != (value & Dock.Bottom)) {
				Cursor = Cursor.SizeNS;
				return;
			}
		}
	}
}