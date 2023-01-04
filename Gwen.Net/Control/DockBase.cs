using System;
using Gwen.Net.Control.Internal;
using Gwen.Net.DragDrop;

namespace Gwen.Net.Control;
/// <summary>
/// Base for dockable containers.
/// </summary>
public class DockBase : ControlBase {
	private DockBase? left;
	private DockBase? right;
	private DockBase? top;
	private DockBase? bottom;
	private Resizer? sizer;

	// Only CHILD dockpanels have a tabcontrol.
	private DockedTabControl? dockedTabControl;

	private bool drawHover;
	private bool dropFar;
	private Rectangle hoverRect;

	// todo: dock events?

	/// <summary>
	/// Control docked on the left side.
	/// </summary>
	public DockBase LeftDock { get { return GetChildDock(Dock.Left); } }

	/// <summary>
	/// Control docked on the right side.
	/// </summary>
	public DockBase RightDock { get { return GetChildDock(Dock.Right); } }

	/// <summary>
	/// Control docked on the top side.
	/// </summary>
	public DockBase TopDock { get { return GetChildDock(Dock.Top); } }

	/// <summary>
	/// Control docked on the bottom side.
	/// </summary>
	public DockBase BottomDock { get { return GetChildDock(Dock.Bottom); } }

	public TabControl? TabControl => dockedTabControl;

	/// <summary>
	/// Initializes a new instance of the <see cref="DockBase"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public DockBase(ControlBase? parent)
		: base(parent) {
		Padding = Padding.One;
		MinimumSize = new Size(30, 30);
		MouseInputEnabled = true;
	}

	/// <summary>
	/// Handler for Space keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeySpace(bool down) {
		// No action on space (default button action is to press)
		return false;
	}

	/// <summary>
	/// Initializes an inner docked control for the specified position.
	/// </summary>
	/// <param name="pos">Dock position.</param>
	protected virtual void SetupChildDock(Dock pos) {
		if(dockedTabControl == null) {
			dockedTabControl = new DockedTabControl(this);
			dockedTabControl.TabRemoved += OnTabRemoved;
			dockedTabControl.TabStripPosition = Dock.Bottom;
			dockedTabControl.TitleBarVisible = true;
		}

		Dock = pos;

		Dock sizeDir;
		if(pos == Dock.Right) sizeDir = Dock.Left;
		else if(pos == Dock.Left) sizeDir = Dock.Right;
		else if(pos == Dock.Top) sizeDir = Dock.Bottom;
		else if(pos == Dock.Bottom) sizeDir = Dock.Top;
		else throw new ArgumentException("Invalid dock", "pos");

		if(sizer != null)
			sizer.Dispose();

		sizer = new Resizer(this);
		sizer.Dock = sizeDir;
		sizer.ResizeDir = sizeDir;
		if(sizeDir == Dock.Left || sizeDir == Dock.Right)
			sizer.Width = 2;
		else
			sizer.Height = 2;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {

	}

	/// <summary>
	/// Gets an inner docked control for the specified position.
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	protected virtual DockBase GetChildDock(Dock pos) {
		DockBase? dock = null;
		switch(pos) {
			case Dock.Left:
				if(left == null) {
					left = new DockBase(this);
					left.Width = 200;
					left.SetupChildDock(pos);
				}
				dock = left;
				break;

			case Dock.Right:
				if(right == null) {
					right = new DockBase(this);
					right.Width = 200;
					right.SetupChildDock(pos);
				}
				dock = right;
				break;

			case Dock.Top:
				if(top == null) {
					top = new DockBase(this);
					top.Height = 200;
					top.SetupChildDock(pos);
				}
				dock = top;
				break;

			case Dock.Bottom:
				if(bottom == null) {
					bottom = new DockBase(this);
					bottom.Height = 200;
					bottom.SetupChildDock(pos);
				}
				dock = bottom;
				break;
		}

		if(dock != null) {
			dock.IsCollapsed = false;
		} else {
			throw new ArgumentException("Invalid enum value provided.", nameof(pos));
		}

		return dock;
	}

	/// <summary>
	/// Calculates dock direction from dragdrop coordinates.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <returns>Dock direction.</returns>
	protected virtual Dock GetDroppedTabDirection(int x, int y) {
		int w = ActualWidth;
		int h = ActualHeight;
		float top = y / (float)h;
		float left = x / (float)w;
		float right = (w - x) / (float)w;
		float bottom = (h - y) / (float)h;
		float minimum = Math.Min(Math.Min(Math.Min(top, left), right), bottom);

		dropFar = (minimum < 0.2f);

		if(minimum > 0.3f)
			return Dock.Fill;

		if(top == minimum && (null == this.top || this.top.IsCollapsed))
			return Dock.Top;
		if(left == minimum && (null == this.left || this.left.IsCollapsed))
			return Dock.Left;
		if(right == minimum && (null == this.right || this.right.IsCollapsed))
			return Dock.Right;
		if(bottom == minimum && (null == this.bottom || this.bottom.IsCollapsed))
			return Dock.Bottom;

		return Dock.Fill;
	}

	public override bool DragAndDrop_CanAcceptPackage(Package p) {
		// A TAB button dropped 
		if(p.Name == "TabButtonMove")
			return true;

		// a TAB window dropped
		if(p.Name == "TabWindowMove")
			return true;

		return false;
	}

	public override bool DragAndDrop_HandleDrop(Package p, int x, int y) {
		Point pos = CanvasPosToLocal(new Point(x, y));
		Dock dir = GetDroppedTabDirection(pos.X, pos.Y);

		Invalidate();

		DockedTabControl? addTo = dockedTabControl;
		if(dir == Dock.Fill && addTo == null)
			return false;

		if(dir != Dock.Fill) {
			DockBase dock = GetChildDock(dir);
			addTo = dock.dockedTabControl;

			if(!dropFar)
				dock.BringToFront();
			else
				dock.SendToBack();
		}

		if(addTo == null) {
			return false;
		}

		if(p.Name == "TabButtonMove") {
			if(DragAndDrop.SourceControl is not TabButton tabButton)
				return false;

			addTo.AddPage(tabButton);
		}

		if(p.Name == "TabWindowMove") {
			if(DragAndDrop.SourceControl is not DockedTabControl tabControl)
				return false;
			if(tabControl == addTo)
				return false;

			tabControl.MoveTabsTo(addTo);
		}

		return true;
	}

	/// <summary>
	/// Indicates whether the control contains any docked children.
	/// </summary>
	public virtual bool IsEmpty {
		get {
			if(dockedTabControl != null && dockedTabControl.TabCount > 0) return false;

			if(left != null && !left.IsEmpty) return false;
			if(right != null && !right.IsEmpty) return false;
			if(top != null && !top.IsEmpty) return false;
			if(bottom != null && !bottom.IsEmpty) return false;

			return true;
		}
	}

	protected virtual void OnTabRemoved(ControlBase control, EventArgs args) {
		DoRedundancyCheck();
		DoConsolidateCheck();
	}

	protected virtual void DoRedundancyCheck() {
		if(!IsEmpty) return;

		if(Parent is not DockBase pDockParent) {
			return;
		}

		pDockParent.OnRedundantChildDock(this);
	}

	protected virtual void DoConsolidateCheck() {
		if(IsEmpty) return;
		if(null == dockedTabControl) return;
		if(dockedTabControl.TabCount > 0) return;

		if(!bottom?.IsEmpty ?? false) {
			bottom?.dockedTabControl?.MoveTabsTo(dockedTabControl);
			return;
		}

		if(!top?.IsEmpty ?? false) {
			top?.dockedTabControl?.MoveTabsTo(dockedTabControl);
			return;
		}

		if(!left?.IsEmpty ?? false) {
			left?.dockedTabControl?.MoveTabsTo(dockedTabControl);
			return;
		}

		if(!right?.IsEmpty ?? false) {
			right?.dockedTabControl?.MoveTabsTo(dockedTabControl);
			return;
		}
	}

	protected virtual void OnRedundantChildDock(DockBase dock) {
		dock.IsCollapsed = true;
		DoRedundancyCheck();
		DoConsolidateCheck();
	}

	public override void DragAndDrop_HoverEnter(Package p, int x, int y) {
		drawHover = true;
	}

	public override void DragAndDrop_HoverLeave(Package p) {
		drawHover = false;
	}

	public override void DragAndDrop_Hover(Package p, int x, int y) {
		Point pos = CanvasPosToLocal(new Point(x, y));
		Dock dir = GetDroppedTabDirection(pos.X, pos.Y);

		if(dir == Dock.Fill) {
			if(null == dockedTabControl) {
				hoverRect = Rectangle.Empty;
				return;
			}

			hoverRect = InnerBounds;
			return;
		}

		hoverRect = RenderBounds;

		int helpBarWidth;

		if(dir == Dock.Left) {
			helpBarWidth = (int)(hoverRect.Width * 0.25f);
			hoverRect.Width = helpBarWidth;
		}

		if(dir == Dock.Right) {
			helpBarWidth = (int)(hoverRect.Width * 0.25f);
			hoverRect.X = hoverRect.Width - helpBarWidth;
			hoverRect.Width = helpBarWidth;
		}

		if(dir == Dock.Top) {
			helpBarWidth = (int)(hoverRect.Height * 0.25f);
			hoverRect.Height = helpBarWidth;
		}

		if(dir == Dock.Bottom) {
			helpBarWidth = (int)(hoverRect.Height * 0.25f);
			hoverRect.Y = hoverRect.Height - helpBarWidth;
			hoverRect.Height = helpBarWidth;
		}

		if((dir == Dock.Top || dir == Dock.Bottom) && !dropFar) {
			if(left != null && !left.IsCollapsed) {
				hoverRect.X += left.ActualWidth;
				hoverRect.Width -= left.ActualWidth;
			}

			if(right != null && !right.IsCollapsed) {
				hoverRect.Width -= right.ActualWidth;
			}
		}

		if((dir == Dock.Left || dir == Dock.Right) && !dropFar) {
			if(top != null && !top.IsCollapsed) {
				hoverRect.Y += top.ActualHeight;
				hoverRect.Height -= top.ActualHeight;
			}

			if(bottom != null && !bottom.IsCollapsed) {
				hoverRect.Height -= bottom.ActualHeight;
			}
		}
	}

	/// <summary>
	/// Renders over the actual control (overlays).
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void RenderOver(Skin.SkinBase skin) {
		if(!drawHover)
			return;

		Renderer.RendererBase render = skin.Renderer;
		render.DrawColor = new Color(20, 255, 200, 255);
		render.DrawFilledRect(RenderBounds);

		if(hoverRect.Width == 0)
			return;

		render.DrawColor = new Color(100, 255, 200, 255);
		render.DrawFilledRect(hoverRect);

		render.DrawColor = new Color(200, 255, 200, 255);
		render.DrawLinedRect(hoverRect);
	}
}