using System;
using System.Collections.Generic;
using System.Linq;
using Gwen.Net.Anim;
using Gwen.Net.Control.Internal;
using Gwen.Net.DragDrop;
using Gwen.Net.Input;
using Gwen.Net.Skin;

namespace Gwen.Net.Control;

// TODO: There is a bug with this class; it ignores the mouse's Y movement. I don't plan on using this control myself so it's low priority, but keep in mind that this class is broken.

/// <summary>
/// Base control class.
/// </summary>
public abstract class ControlBase : IDisposable {
	/// <summary>
	/// Delegate used for all control event handlers.
	/// </summary>
	/// <param name="sender">Event source.</param>
	/// <param name="arguments" >Additional arguments. May be empty (EventArgs.Empty).</param>
	public delegate void GwenEventHandler<in T>(ControlBase sender, T arguments) where T : System.EventArgs;

	private bool disposed;

	private ControlBase? parent;

	/// <summary>
	/// This is the panel's actual parent - most likely the logical 
	/// parent's InnerPanel (if it has one). You should rarely need this.
	/// </summary>
	private ControlBase? actualParent;

	private ControlBase? toolTip;

	private SkinBase? skin;

	private Rectangle bounds;
	private Rectangle renderBounds;
	private Rectangle innerBounds;

	private Rectangle desiredBounds;

	private Rectangle anchorBounds;
	private Anchor anchor;

	private Size measuredSize;

	private Size minimumSize = Size.One;
	private Size maximumSize = Size.Infinity;

	protected Padding padding;
	private Margin margin;

	private string name;

	private Cursor cursor;

	private bool cacheTextureDirty;
	private bool cacheToTexture;

	private Package? dragAndDropPackage;

	private Xml.Component? component;

	private object? userData;

	/// <summary>
	/// Real list of children.
	/// </summary>
	private readonly List<ControlBase> children;

	/// <summary>
	/// Invoked when mouse pointer enters the control.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? HoverEnter;

	/// <summary>
	/// Invoked when mouse pointer leaves the control.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? HoverLeave;

	/// <summary>
	/// Invoked when control's bounds have been changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? BoundsChanged;

	/// <summary>
	/// Invoked when the control has been left-clicked.
	/// </summary>
	[Xml.XmlEvent]
	public virtual event GwenEventHandler<ClickedEventArgs>? Clicked;

	/// <summary>
	/// Invoked when the control has been double-left-clicked.
	/// </summary>
	[Xml.XmlEvent]
	public virtual event GwenEventHandler<ClickedEventArgs>? DoubleClicked;

	/// <summary>
	/// Invoked when the control has been right-clicked.
	/// </summary>
	[Xml.XmlEvent]
	public virtual event GwenEventHandler<ClickedEventArgs>? RightClicked;

	/// <summary>
	/// Invoked when the control has been double-right-clicked.
	/// </summary>
	[Xml.XmlEvent]
	public virtual event GwenEventHandler<ClickedEventArgs>? DoubleRightClicked;

	/// <summary>
	/// Returns true if any on click events are set.
	/// </summary>
	internal bool ClickEventAssigned => Clicked != null || RightClicked != null || DoubleClicked != null || DoubleRightClicked != null;

	/// <summary>
	/// Accelerator map.
	/// </summary>
	private readonly Dictionary<string, GwenEventHandler<EventArgs>> accelerators;

	/// <summary>
	/// Logical list of children.
	/// </summary>
	public virtual List<ControlBase> Children => children;

	/// <summary>
	/// The logical parent. It's usually what you expect, the control you've parented it to.
	/// </summary>
	public ControlBase? Parent {
		get => parent;
		set {
			if(parent == value) {
				return;
			}

			parent?.RemoveChild(this, false);

			parent = value;
			actualParent = null;

			parent?.AddChild(this);
		}
	}

	/// <summary>
	/// Dock position.
	/// </summary>
	[Xml.XmlProperty]
	public Dock Dock {
		get { return (Dock)GetInternalFlag(InternalFlags.Dock_Mask); }
		set {
			if (CheckAndChangeInternalFlag(InternalFlags.Dock_Mask, (InternalFlags)value))
				Invalidate();
		}
	}

	/// <summary>
	/// Is layout needed.
	/// </summary>
	protected bool NeedsLayout { get { return IsSetInternalFlag(InternalFlags.NeedsLayout); } set { SetInternalFlag(InternalFlags.NeedsLayout, value); } }

	/// <summary>
	/// Is layout done at least once for the control.
	/// </summary>
	protected bool LayoutDone { get { return IsSetInternalFlag(InternalFlags.LayoutDone); } set { SetInternalFlag(InternalFlags.LayoutDone, value); } }

	/// <summary>
	/// Current skin.
	/// </summary>
	public Skin.SkinBase Skin {
		get {
			if (skin != null)
				return skin;
			if (parent != null)
				return parent.Skin;

			throw new InvalidOperationException("GetSkin: null");
		}
	}

	/// <summary>
	/// Current tooltip.
	/// </summary>
	public ControlBase? ToolTip {
		get { return toolTip; }
		set {
			toolTip = value;
			if (toolTip != null) {
				toolTip.Collapse(true, false);
			}
		}
	}

	/// <summary>
	/// Label typed tool tip text.
	/// </summary>
	[Xml.XmlProperty]
	public string ToolTipText {
		get {
			if (toolTip != null && toolTip is Label)
				return ((Label)toolTip).Text;
			else
				return String.Empty;
		}
		set {
			SetToolTipText(value);
		}
	}

	/// <summary>
	/// Indicates whether this control is a menu component.
	/// </summary>
	internal virtual bool IsMenuComponent {
		get {
			if (parent == null)
				return false;
			return parent.IsMenuComponent;
		}
	}

	/// <summary>
	/// Determines whether the control should be clipped to its bounds while rendering.
	/// </summary>
	protected virtual bool ShouldClip { get { return true; } }

	/// <summary>
	/// Minimum size that the control needs to draw itself correctly. Valid after DoMeasure call. This includes margins.
	/// </summary>
	public Size MeasuredSize { get { return measuredSize; } }

	public virtual float Scale {
		get {
			if (parent != null)
				return parent.Scale;
			else
				return 1.0f;
		}
		set {
			throw new NotImplementedException();
		}
	}

	public int BaseUnit {
		get {
			return Util.Ceil(Skin.BaseUnit * Scale);
		}
	}

	/// <summary>
	/// Current padding - inner spacing. Padding is not valid for all controls.
	/// </summary>
	[Xml.XmlProperty]
	public virtual Padding Padding {
		get { return padding; }
		set {
			if (padding == value)
				return;

			padding = value;
			Invalidate();
		}
	}

	/// <summary>
	/// Current margin - outer spacing.
	/// </summary>
	[Xml.XmlProperty]
	public Margin Margin {
		get { return margin; }
		set {
			if (margin == value)
				return;

			margin = value;
			InvalidateParent();
		}
	}

	/// <summary>
	/// Vertical alignment of the control if the control is smaller than the available space.
	/// </summary>
	[Xml.XmlProperty]
	public VerticalAlignment VerticalAlignment {
		get { return (VerticalAlignment)GetInternalFlag(InternalFlags.AlignV_Mask); }
		set {
			if (CheckAndChangeInternalFlag(InternalFlags.AlignV_Mask, (InternalFlags)value))
				Invalidate();
		}
	}

	/// <summary>
	/// Horizontal alignment of the control if the control is smaller than the available space.
	/// </summary>
	[Xml.XmlProperty]
	public HorizontalAlignment HorizontalAlignment {
		get { return (HorizontalAlignment)GetInternalFlag(InternalFlags.AlignH_Mask); }
		set {
			if (CheckAndChangeInternalFlag(InternalFlags.AlignH_Mask, (InternalFlags)value))
				Invalidate();
		}
	}

	/// <summary>
	/// Indicates whether the control is on top of its parent's children.
	/// </summary>
	public virtual bool IsOnTop => Parent?.children.FirstOrDefault() is ControlBase control && control == this;

	/// <summary>
	/// Component if this control is the base of the user defined control group.
	/// </summary>
	public Xml.Component? Component { get { return component; } set { component = value; } }

	/// <summary>
	/// User data associated with the control.
	/// </summary>
	[Xml.XmlProperty]
	public object? UserData { get { return userData; } set { userData = value; } }

	/// <summary>
	/// Indicates whether the control is hovered by mouse pointer.
	/// </summary>
	public virtual bool IsHovered => InputHandler.HoveredControl == this;

	/// <summary>
	/// Indicates whether the control has focus.
	/// </summary>
	public bool HasFocus => InputHandler.KeyboardFocus == this;

	/// <summary>
	/// Indicates whether the control is disabled.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsDisabled { 
		get => IsSetInternalFlag(InternalFlags.Disabled);
		set { 
			SetInternalFlag(InternalFlags.Disabled, value); 
		} 
	}

	/// <summary>
	/// Indicates whether the control is hidden.
	/// </summary>
	[Xml.XmlProperty]
	public virtual bool IsHidden { 
		get => IsSetInternalFlag(InternalFlags.Hidden); 
		set {
			if(CheckAndChangeInternalFlag(InternalFlags.Hidden, value)) {
				Redraw();
			}
		} 
	}

	/// <summary>
	/// Indicates whether the control is hidden.
	/// </summary>
	[Xml.XmlProperty]
	public virtual bool IsCollapsed { 
		get { 
			return IsSetInternalFlag(InternalFlags.Collapsed); 
		} set {
			if(CheckAndChangeInternalFlag(InternalFlags.Collapsed, value)) {
				InvalidateParent();
			}
		} 
	}

	/// <summary>
	/// Determines whether the control's position should be restricted to parent's bounds.
	/// </summary>
	public bool RestrictToParent { get { return IsSetInternalFlag(InternalFlags.RestrictToParent); } set { SetInternalFlag(InternalFlags.RestrictToParent, value); } }

	/// <summary>
	/// Determines whether the control receives mouse input events.
	/// </summary>
	public bool MouseInputEnabled { get { return IsSetInternalFlag(InternalFlags.MouseInputEnabled); } set { SetInternalFlag(InternalFlags.MouseInputEnabled, value); } }

	/// <summary>
	/// Determines whether the control receives keyboard input events.
	/// </summary>
	public bool KeyboardInputEnabled { get { return IsSetInternalFlag(InternalFlags.KeyboardInputEnabled); } set { SetInternalFlag(InternalFlags.KeyboardInputEnabled, value); } }

	/// <summary>
	/// Determines whether the control receives keyboard character events.
	/// </summary>
	public bool KeyboardNeeded { get { return IsSetInternalFlag(InternalFlags.KeyboardNeeded); } set { SetInternalFlag(InternalFlags.KeyboardNeeded, value); } }

	/// <summary>
	/// Gets or sets the mouse cursor when the cursor is hovering the control.
	/// </summary>
	public Cursor Cursor { get { return cursor; } set { cursor = value; } }

	/// <summary>
	/// Indicates whether the control is tabable (can be focused by pressing Tab).
	/// </summary>
	public bool IsTabable { get { return IsSetInternalFlag(InternalFlags.Tabable); } set { SetInternalFlag(InternalFlags.Tabable, value); } }

	/// <summary>
	/// Indicates whether control's background should be drawn during rendering.
	/// </summary>
	public bool ShouldDrawBackground { get { return IsSetInternalFlag(InternalFlags.DrawBackground); } set { SetInternalFlag(InternalFlags.DrawBackground, value); } }

	/// <summary>
	/// Indicates whether the renderer should cache drawing to a texture to improve performance (at the cost of memory).
	/// </summary>
	public bool ShouldCacheToTexture { get { return cacheToTexture; } set { cacheToTexture = value; /*Children.ForEach(x => x.ShouldCacheToTexture=value);*/ } }

	/// <summary>
	/// Gets or sets the control's internal name.
	/// </summary>
	[Xml.XmlProperty]
	public string Name { get { return name; } set { name = value; } }

	/// <summary>
	/// Control's size and position relative to the parent.
	/// </summary>
	public Rectangle Bounds { get { return bounds; } }

	/// <summary>
	/// Bounds for the renderer.
	/// </summary>
	public Rectangle RenderBounds { get { return renderBounds; } }

	/// <summary>
	/// Bounds adjusted by padding.
	/// </summary>
	public Rectangle InnerBounds { get { return innerBounds; } }

	/// <summary>
	/// Size restriction.
	/// </summary>
	[Xml.XmlProperty]
	public Size MinimumSize { get { return minimumSize; } set { minimumSize = value; InvalidateParent(); } }

	/// <summary>
	/// Size restriction.
	/// </summary>
	[Xml.XmlProperty]
	public Size MaximumSize { get { return maximumSize; } set { maximumSize = value; InvalidateParent(); } }

	/// <summary>
	/// Determines whether hover should be drawn during rendering.
	/// </summary>
	protected bool ShouldDrawHover { get { return InputHandler.MouseFocus == this || InputHandler.MouseFocus == null; } }

	protected virtual bool AccelOnlyFocus { get { return false; } }
	protected virtual bool NeedsInputChars { get { return false; } }

	/// <summary>
	/// Indicates whether the control and its parents are visible.
	/// </summary>
	public bool IsVisible {
		get {
			if (IsHidden)
				return false;

			if (IsCollapsed)
				return false;

			if (Parent != null)
				return Parent.IsVisible;

			return true;
		}
	}

	/// <summary>
	/// Location of the control. Valid after DoArrange call.
	/// </summary>
	public int ActualLeft { get { return bounds.X; } }
	/// <summary>
	/// Location of the control. Valid after DoArrange call.
	/// </summary>
	public int ActualTop { get { return bounds.Y; } }
	/// <summary>
	/// Width of the control. Valid after DoArrange call.
	/// </summary>
	public int ActualWidth { get { return bounds.Width; } }
	/// <summary>
	/// Height of the control. Valid after DoArrange call.
	/// </summary>
	public int ActualHeight { get { return bounds.Height; } }

	/// <summary>
	/// Location of the control. Valid after DoArrange call.
	/// </summary>
	public Point ActualPosition { get { return bounds.Location; } }
	/// <summary>
	/// Size of the control. Valid after DoArrange call.
	/// </summary>
	public Size ActualSize { get { return bounds.Size; } }

	/// <summary>
	/// Location of the control. Valid after DoArrange call.
	/// </summary>
	public int ActualRight { get { return bounds.Right; } }
	/// <summary>
	/// Location of the control. Valid after DoArrange call.
	/// </summary>
	public int ActualBottom { get { return bounds.Bottom; } }

	/// <summary>
	/// Desired location of the control. Used only on default layout (DockLayout) if Dock property is None.
	/// </summary>
	[Xml.XmlProperty]
	public virtual int Left { get { return desiredBounds.X; } set { if (desiredBounds.X == value) return; desiredBounds.X = value; InvalidateParent(); } }
	/// <summary>
	/// Desired location of the control. Used only on default layout (DockLayout) if Dock property is None.
	/// </summary>
	[Xml.XmlProperty]
	public virtual int Top { get { return desiredBounds.Y; } set { if (desiredBounds.Y == value) return; desiredBounds.Y = value; InvalidateParent(); } }
	/// <summary>
	/// Desired size of the control. Set this value only if HorizontalAlignment is not Stretch. By default this value is ignored.
	/// </summary>
	[Xml.XmlProperty]
	public virtual int Width { get { return desiredBounds.Width; } set { if (desiredBounds.Width == value) return; desiredBounds.Width = value; /*if (m_HorizontalAlignment == HorizontalAlignment.Stretch) m_HorizontalAlignment = HorizontalAlignment.Left;*/ InvalidateParent(); } }
	/// <summary>
	/// Desired size of the control. Set this value only if VerticalAlignment is not Stretch. By default this value is ignored.
	/// </summary>
	[Xml.XmlProperty]
	public virtual int Height { get { return desiredBounds.Height; } set { if (desiredBounds.Height == value) return; desiredBounds.Height = value; /*if (m_VerticalAlignment == VerticalAlignment.Stretch) m_VerticalAlignment = VerticalAlignment.Top;*/ InvalidateParent(); } }

	/// <summary>
	/// Desired location of the control. Used only on default layout (DockLayout) if Dock property is None.
	/// </summary>
	[Xml.XmlProperty]
	public virtual Point Position { get { return desiredBounds.Location; } set { if (desiredBounds.Location == value) return; desiredBounds.Location = value; InvalidateParent(); } }
	/// <summary>
	/// Desired size of the control. Set this only if both of alignments are not Stretch. By default this value is ignored.
	/// </summary>
	[Xml.XmlProperty]
	public virtual Size Size { get { return desiredBounds.Size; } set { if (desiredBounds.Size == value) return; desiredBounds.Size = value; InvalidateParent(); } }

	/// <summary>
	/// Desired location and size of the control. Set this only if both of alignments are not Stretch. Used only on default layout (DockLayout) if Dock property is None. By default size is ignored.
	/// </summary>
	[Xml.XmlProperty]
	public virtual Rectangle DesiredBounds { get { return desiredBounds; } set { if (desiredBounds == value) return; desiredBounds = value; InvalidateParent(); } }

	/// <summary>
	/// Default location and size of the control insize the container. Used only on AnchorLayout.
	/// </summary>
	[Xml.XmlProperty]
	public Rectangle AnchorBounds { get { return anchorBounds; } set { if (anchorBounds == value) return; anchorBounds = value; Invalidate(); } }
	/// <summary>
	/// How the control is moved and/or stretched if the container size changes. Used only on AnchorLayout.
	/// </summary>
	[Xml.XmlProperty]
	public Anchor Anchor { get { return anchor; } set { if (anchor == value) return; anchor = value; Invalidate(); } }

	/// <summary>
	/// Enable this if the parent of the control doesn't need to know if a new layout is needed.
	/// </summary>
	protected bool IsVirtualControl { get { return IsSetInternalFlag(InternalFlags.VirtualControl); } set { SetInternalFlag(InternalFlags.VirtualControl, value); } }

	/// <summary>
	/// Determines whether margin, padding and bounds outlines for the control will be drawn. Applied recursively to all children.
	/// </summary>
	public bool DrawDebugOutlines {
		get { return IsSetInternalFlag(InternalFlags.DrawDebugOutlines); }
		set {
			if (!CheckAndChangeInternalFlag(InternalFlags.DrawDebugOutlines, value))
				return;
			foreach (ControlBase child in Children) {
				child.DrawDebugOutlines = value;
			}
		}
	}

	public Color PaddingOutlineColor { get; set; }
	public Color MarginOutlineColor { get; set; }
	public Color BoundsOutlineColor { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ControlBase"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ControlBase(ControlBase? parent = null) {
		children = new List<ControlBase>();
		accelerators = new Dictionary<string, GwenEventHandler<EventArgs>>();

		bounds = new Rectangle(Point.Zero, Size.Infinity);
		padding = Padding.Zero;
		margin = Margin.Zero;

		anchor = Anchor.LeftTop;

		desiredBounds = new Rectangle(0, 0, Util.Ignore, Util.Ignore);

		anchorBounds = new Rectangle(0, 0, 0, 0);

		SetInternalFlag(InternalFlags.AlignHStretch | InternalFlags.AlignVStretch | InternalFlags.DockNone, true);

		Parent = parent;

		RestrictToParent = false;

		MouseInputEnabled = false; // Edit: Changed to false. Todo: Check if this is ok.
		KeyboardInputEnabled = false;

		Invalidate();
		Cursor = Cursor.Normal;
		toolTip = null;
		IsTabable = false;
		ShouldDrawBackground = true;
		cacheTextureDirty = true;
		cacheToTexture = false;

		BoundsOutlineColor = Color.Red;
		MarginOutlineColor = Color.Green;
		PaddingOutlineColor = Color.Blue;

		name = "Unnamed " + GetType().Name;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public virtual void Dispose() {
		if (disposed) {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(String.Format("Control {{{0}}} disposed twice.", this));
#endif
			return;
		}

		if (InputHandler.HoveredControl == this)
			InputHandler.HoveredControl = null;
		if (InputHandler.KeyboardFocus == this)
			InputHandler.KeyboardFocus = null;
		if (InputHandler.MouseFocus == this)
			InputHandler.MouseFocus = null;

		DragAndDrop.ControlDeleted(this);
		Gwen.Net.ToolTip.ControlDeleted(this);
		Animation.Cancel(this);

		foreach (ControlBase child in children)
			child.Dispose();

		if (toolTip != null)
			toolTip.Dispose();

		children.Clear();

		disposed = true;
		GC.SuppressFinalize(this);
	}

#if DEBUG
        ~ControlBase()
        {
            throw new InvalidOperationException(String.Format("IDisposable object {{{0}}} finalized.", this));
        }
#endif

	/// <summary>
	/// Detaches the control from canvas and adds to the deletion queue (processed in Canvas.DoThink).
	/// </summary>
	public void DelayedDelete() {
		GetCanvas().AddDelayedDelete(this);
	}

	public override string ToString() {
		string type = GetType().ToString();
		string name = string.IsNullOrWhiteSpace(this.name) ? "" : " Name: " + this.name;
		if(this is MenuItem menuItem) {
			return type + name + " [MenuItem: " + menuItem.Text + "]";
		}
		if(this is Label label) {
			return type + name + " [Label: " + label.Text + "]";
		}
		if (this is Text text) {
			return type + name + " [Text: " + text.Content + "]";
		}

		return type + name;
	}

	/// <summary>
	/// Gets the canvas (root parent) of the control.
	/// </summary>
	/// <returns></returns>
	public virtual Canvas GetCanvas() {
		if(parent == null) {
			if(this is Canvas retVal) {
				return retVal;
			}
			throw new Exception("Root parent is not a canvas!");
		}

		return parent.GetCanvas();
	}

	/// <summary>
	/// Enables the control.
	/// </summary>
	public void Enable() {
		IsDisabled = false;
	}

	/// <summary>
	/// Disables the control.
	/// </summary>
	public virtual void Disable() {
		IsDisabled = true;
	}

	/// <summary>
	/// Default accelerator handler.
	/// </summary>
	/// <param name="control">Event source.</param>
	/// <param name="args">Event args.</param>
	private void DefaultAcceleratorHandler(ControlBase control, EventArgs args) {
		OnAccelerator();
	}

	/// <summary>
	/// Default accelerator handler.
	/// </summary>
	protected virtual void OnAccelerator() {

	}

	/// <summary>
	/// Hides the control. Hidden controls participate in the layout process. If you don't want to layout, use Collapse.
	/// </summary>
	public virtual void Hide() {
		IsHidden = true;
	}

	/// <summary>
	/// Collapse or show the control. Collapsed controls don't participate in the layout process and are hidden.
	/// </summary>
	/// <param name="collapsed">Collapse or show.</param>
	/// <param name="measure">Is layout triggered.</param>
	public virtual void Collapse(bool collapsed = true, bool measure = true) {
		if (!measure)
			SetInternalFlag(InternalFlags.Collapsed, collapsed);
		else
			IsCollapsed = collapsed;
	}

	/// <summary>
	/// Shows the control.
	/// </summary>
	public virtual void Show() {
		IsCollapsed = false;
		IsHidden = false;
	}

	/// <summary>
	/// Creates a tooltip for the control.
	/// </summary>
	/// <param name="text">Tooltip text.</param>
	public virtual void SetToolTipText(string text) {
		Label tooltip = new Label(this) {
			Parent = null,
			skin = Skin,
			Text = text,
			TextColorOverride = Skin.Colors.TooltipText,
			Padding = new Padding(5, 3, 5, 3)
		};
		// ToolTip doesn't have a parent
		// and that's why we need to set skin here.

		ToolTip = tooltip;
	}

	/// <summary>
	/// Trigger the layout process.
	/// </summary>
	public virtual void Invalidate() {
		if (!this.IsVirtualControl || !this.LayoutDone) {
			NeedsLayout = true;

			if (parent != null)
				if (!parent.NeedsLayout)
					parent.Invalidate();
		} else {
			Canvas canvas = GetCanvas();
			if (canvas != null)
				canvas.AddToMeasure(this);
		}
	}

	/// <summary>
	/// Trigger parent layout process. Use this instead of Invalidate() if you know that
	/// the parent is affected some way by the change.
	/// </summary>
	public virtual void InvalidateParent() {
		if (parent != null)
			parent.Invalidate();
	}

	/// <summary>
	/// Sends the control to the bottom of paren't visibility stack.
	/// </summary>
	public virtual void SendToBack() {
		if (actualParent == null)
			return;
		if (actualParent.children.Count == 0)
			return;
		if (actualParent.children.First() == this)
			return;

		actualParent.children.Remove(this);
		actualParent.children.Insert(0, this);
	}

	/// <summary>
	/// Brings the control to the top of paren't visibility stack.
	/// </summary>
	public virtual void BringToFront() {
		if (actualParent == null)
			return;
		if (actualParent.children.Last() == this)
			return;

		actualParent.children.Remove(this);
		actualParent.children.Add(this);
		Redraw();
	}

	public virtual void BringNextToControl(ControlBase child, bool behind) {
		if (null == actualParent)
			return;

		int index = actualParent.children.IndexOf(this);
		int newIndex = actualParent.children.IndexOf(child);

		if (index == -1 || newIndex == -1)
			return;

		if (newIndex == 0 && !behind) {
			SendToBack();
			return;
		} else if (newIndex == actualParent.children.Count - 1 && behind) {
			BringToFront();
			return;
		}

		actualParent.children.Remove(this);
		if (newIndex > index)
			newIndex--;

		if (behind)
			newIndex++;

		actualParent.children.Insert(newIndex, this);
	}

	/// <summary>
	/// Finds a child by name.
	/// </summary>
	/// <param name="name">Child name.</param>
	/// <param name="recursive">Determines whether the search should be recursive.</param>
	/// <returns>Found control or null.</returns>
	public virtual ControlBase? FindChildByName(string name, bool recursive = false) {
		ControlBase? b = Children.Where(x => x.name == name).FirstOrDefault();
		if(b != null) {
			return b;
		}

		if (recursive) {
			foreach (ControlBase child in Children) {
				b = child.FindChildByName(name, true);
				if(b != null) {
					return b;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Attaches specified control as a child of this one.
	/// </summary>
	/// <param name="child">Control to be added as a child.</param>
	public virtual void AddChild(ControlBase child) {
		children.Add(child);
		child.actualParent = this;

		OnChildAdded(child);
	}

	/// <summary>
	/// Detaches specified control from this one.
	/// </summary>
	/// <param name="child">Child to be removed.</param>
	/// <param name="dispose">Determines whether the child should be disposed (added to delayed delete queue).</param>
	public virtual void RemoveChild(ControlBase child, bool dispose) {
		children.Remove(child);
		OnChildRemoved(child);

		if (dispose)
			child.DelayedDelete();
	}

	/// <summary>
	/// Removes all children (and disposes them).
	/// </summary>
	public virtual void DeleteAllChildren() {
		// todo: probably shouldn't invalidate after each removal
		while (children.Count > 0)
			RemoveChild(children[0], true);
	}

	/// <summary>
	/// Handler invoked when a child is added.
	/// </summary>
	/// <param name="child">Child added.</param>
	protected virtual void OnChildAdded(ControlBase child) {
	}

	/// <summary>
	/// Handler invoked when a child is removed.
	/// </summary>
	/// <param name="child">Child removed.</param>
	protected virtual void OnChildRemoved(ControlBase child) {
	}

	/// <summary>
	/// Moves the control to a specific point, clamping on paren't bounds if RestrictToParent is set.
	/// This function will override control location set by layout or user.
	/// </summary>
	/// <param name="x">Target x coordinate.</param>
	/// <param name="y">Target y coordinate.</param>
	public virtual void MoveTo(int x, int y) {
		if (RestrictToParent && (Parent != null)) {
			ControlBase parent = Parent;
			if (x < Padding.Left)
				x = Padding.Left;
			else if (x + ActualWidth > parent.ActualWidth - Padding.Right)
				x = parent.ActualWidth - ActualWidth - Padding.Right;
			if (y < Padding.Top)
				y = Padding.Top;
			else if (y + ActualHeight > parent.ActualHeight - Padding.Bottom)
				y = parent.ActualHeight - ActualHeight - Padding.Bottom;
		}

		SetBounds(x, y, ActualWidth, ActualHeight);

		desiredBounds.X = bounds.X;
		desiredBounds.Y = bounds.Y;
	}

	/// <summary>
	/// Sets the control position.
	/// </summary>
	/// <param name="x">Target x coordinate.</param>
	/// <param name="y">Target y coordinate.</param>
	/// <remarks>Bounds are reset after the next layout pass.</remarks>
	public virtual void SetPosition(float x, float y) {
		SetPosition((int)x, (int)y);
	}

	/// <summary>
	/// Sets the control position.
	/// </summary>
	/// <param name="x">Target x coordinate.</param>
	/// <param name="y">Target y coordinate.</param>
	/// <remarks>Bounds are reset after the next layout pass.</remarks>
	public virtual void SetPosition(int x, int y) {
		SetBounds(x, y, ActualWidth, ActualHeight);
	}

	/// <summary>
	/// Sets the control size.
	/// </summary>
	/// <param name="width">New width.</param>
	/// <param name="height">New height.</param>
	/// <returns>True if bounds changed.</returns>
	/// <remarks>Bounds are reset after the next layout pass.</remarks>
	public virtual bool SetSize(int width, int height) {
		return SetBounds(ActualLeft, ActualTop, width, height);
	}

	/// <summary>
	/// Sets the control bounds.
	/// </summary>
	/// <param name="bounds">New bounds.</param>
	/// <returns>True if bounds changed.</returns>
	/// <remarks>Bounds are reset after the next layout pass.</remarks>
	public virtual bool SetBounds(Rectangle bounds) {
		return SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
	}

	/// <summary>
	/// Sets the control bounds.
	/// </summary>
	/// <param name="x">X position.</param>
	/// <param name="y">Y position.</param>
	/// <param name="width">Width.</param>
	/// <param name="height">Height.</param>
	/// <returns>
	/// True if bounds changed.
	/// </returns>
	/// <remarks>Bounds are reset after the next layout pass.</remarks>
	public virtual bool SetBounds(int x, int y, int width, int height) {
		if (bounds.X == x &&
			bounds.Y == y &&
			bounds.Width == width &&
			bounds.Height == height)
			return false;

		Rectangle oldBounds = Bounds;

		bounds.X = x;
		bounds.Y = y;

		bounds.Width = width;
		bounds.Height = height;

		OnBoundsChanged(oldBounds);

		Redraw();
		UpdateRenderBounds();

		if (BoundsChanged != null)
			BoundsChanged.Invoke(this, EventArgs.Empty);

		return true;
	}

	/// <summary>
	/// Handler invoked when control's bounds change.
	/// </summary>
	/// <param name="oldBounds">Old bounds.</param>
	protected virtual void OnBoundsChanged(Rectangle oldBounds) {
	}

	/// <summary>
	/// Handler invoked when control's scale changes.
	/// </summary>
	protected virtual void OnScaleChanged() {
		foreach (ControlBase child in children) {
			child.OnScaleChanged();
		}
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected virtual void Render(Skin.SkinBase skin) {
	}

	/// <summary>
	/// Renders the control to a cache using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	/// <param name="master">Root parent.</param>
	protected virtual void DoCacheRender(Skin.SkinBase skin, ControlBase master) {
		Renderer.RendererBase render = skin.Renderer;
		Renderer.ICacheToTexture? cache = render.CTT;

		if (cache == null)
			return;

		Point oldRenderOffset = render.RenderOffset;
		Rectangle oldRegion = render.ClipRegion;

		if (this != master) {
			render.AddRenderOffset(Bounds);
			render.AddClipRegion(Bounds);
		} else {
			render.RenderOffset = Point.Zero;
			render.ClipRegion = new Rectangle(0, 0, ActualWidth, ActualHeight);
		}

		if (cacheTextureDirty && render.ClipRegionVisible) {
			render.StartClip();

			if (ShouldCacheToTexture)
				cache.SetupCacheTexture(this);

			//Render myself first
			//var old = render.ClipRegion;
			//render.ClipRegion = Bounds;
			//var old = render.RenderOffset;
			//render.RenderOffset = new Point(Bounds.X, Bounds.Y);
			Render(skin);
			//render.RenderOffset = old;
			//render.ClipRegion = old;

			if (children.Count > 0) {
				//Now render my kids
				foreach (ControlBase child in children) {
					if (child.IsHidden || child.IsCollapsed)
						continue;
					child.DoCacheRender(skin, master);
				}
			}

			if (ShouldCacheToTexture) {
				cache.FinishCacheTexture(this);
				cacheTextureDirty = false;
			}
		}

		render.ClipRegion = oldRegion;
		render.StartClip();
		render.RenderOffset = oldRenderOffset;

		if (ShouldCacheToTexture)
			cache.DrawCachedControlTexture(this);
	}

	/// <summary>
	/// Rendering logic implementation.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	internal virtual void DoRender(Skin.SkinBase skin) {
		// If this control has a different skin, 
		// then so does its children.
		if (this.skin != null)
			skin = this.skin;

		// Do think
		Think();

		Renderer.RendererBase render = skin.Renderer;

		if (render.CTT != null && ShouldCacheToTexture) {
			DoCacheRender(skin, this);
			return;
		}

		RenderRecursive(skin, Bounds);

		if (DrawDebugOutlines)
			skin.DrawDebugOutlines(this);
	}

	/// <summary>
	/// Recursive rendering logic.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	/// <param name="clipRect">Clipping rectangle.</param>
	protected virtual void RenderRecursive(Skin.SkinBase skin, Rectangle clipRect) {
		Renderer.RendererBase render = skin.Renderer;
		Point oldRenderOffset = render.RenderOffset;

		render.AddRenderOffset(clipRect);

		RenderUnder(skin);

		Rectangle oldRegion = render.ClipRegion;

		if (ShouldClip) {
			render.AddClipRegion(clipRect);

			if (!render.ClipRegionVisible) {
				render.RenderOffset = oldRenderOffset;
				render.ClipRegion = oldRegion;
				return;
			}

			render.StartClip();
		}

		//Render myself first
		Render(skin);

		if (children.Count > 0) {
			//Now render my kids
			foreach (ControlBase child in children) {
				if (child.IsHidden || child.IsCollapsed)
					continue;
				child.DoRender(skin);
			}
		}

		render.ClipRegion = oldRegion;
		render.StartClip();
		RenderOver(skin);

		RenderFocus(skin);

		render.RenderOffset = oldRenderOffset;
	}

	/// <summary>
	/// Sets the control's skin.
	/// </summary>
	/// <param name="skin">New skin.</param>
	/// <param name="doChildren">Deterines whether to change children skin.</param>
	public virtual void SetSkin(Skin.SkinBase skin, bool doChildren = false) {
		if (this.skin == skin)
			return;
		this.skin = skin;
		//Invalidate();
		Redraw();
		OnSkinChanged(skin);

		if (doChildren) {
			foreach (ControlBase child in children) {
				child.SetSkin(skin, true);
			}
		}
	}

	/// <summary>
	/// Handler invoked when control's skin changes.
	/// </summary>
	/// <param name="newSkin">New skin.</param>
	protected virtual void OnSkinChanged(Skin.SkinBase newSkin) {

	}

	/// <summary>
	/// Handler invoked on mouse wheel event.
	/// </summary>
	/// <param name="delta">Scroll delta.</param>
	protected virtual bool OnMouseWheeled(int delta) {
		if (actualParent != null)
			return actualParent.OnMouseWheeled(delta);

		return false;
	}

	/// <summary>
	/// Invokes mouse wheeled event (used by input system).
	/// </summary>
	internal bool InputMouseWheeled(int delta) {
		return OnMouseWheeled(delta);
	}

	/// <summary>
	/// Handler invoked on mouse moved event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="dx">X change.</param>
	/// <param name="dy">Y change.</param>
	protected virtual void OnMouseMoved(int x, int y, int dx, int dy) {

	}

	/// <summary>
	/// Invokes mouse moved event (used by input system).
	/// </summary>
	internal void InputMouseMoved(int x, int y, int dx, int dy) {
		OnMouseMoved(x, y, dx, dy);
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected virtual void OnMouseClickedLeft(int x, int y, bool down) {
		if (down && Clicked != null)
			Clicked(this, new ClickedEventArgs(x, y, down));
	}

	/// <summary>
	/// Invokes left mouse click event (used by input system).
	/// </summary>
	internal void InputMouseClickedLeft(int x, int y, bool down) {
		OnMouseClickedLeft(x, y, down);
	}

	/// <summary>
	/// Handler invoked on mouse click (right) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected virtual void OnMouseClickedRight(int x, int y, bool down) {
		if (down && RightClicked != null)
			RightClicked(this, new ClickedEventArgs(x, y, down));
	}

	/// <summary>
	/// Invokes right mouse click event (used by input system).
	/// </summary>
	internal void InputMouseClickedRight(int x, int y, bool down) {
		OnMouseClickedRight(x, y, down);
	}

	/// <summary>
	/// Handler invoked on mouse double click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	protected virtual void OnMouseDoubleClickedLeft(int x, int y) {
		// [omeg] should this be called?
		// [halfofastaple] Maybe. Technically, a double click is still technically a single click. However, this shouldn't be called here, and
		//					Should be called by the event handler.
		OnMouseClickedLeft(x, y, true);

		if (DoubleClicked != null)
			DoubleClicked(this, new ClickedEventArgs(x, y, true));
	}

	/// <summary>
	/// Invokes left double mouse click event (used by input system).
	/// </summary>
	internal void InputMouseDoubleClickedLeft(int x, int y) {
		OnMouseDoubleClickedLeft(x, y);
	}

	/// <summary>
	/// Handler invoked on mouse double click (right) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	protected virtual void OnMouseDoubleClickedRight(int x, int y) {
		// [halfofastaple] See: OnMouseDoubleClicked for discussion on triggering single clicks in a double click event
		OnMouseClickedRight(x, y, true);

		if (DoubleRightClicked != null)
			DoubleRightClicked(this, new ClickedEventArgs(x, y, true));
	}

	/// <summary>
	/// Invokes right double mouse click event (used by input system).
	/// </summary>
	internal void InputMouseDoubleClickedRight(int x, int y) {
		OnMouseDoubleClickedRight(x, y);
	}

	/// <summary>
	/// Handler invoked on mouse cursor entering control's bounds.
	/// </summary>
	protected virtual void OnMouseEntered() {
		if (HoverEnter != null)
			HoverEnter.Invoke(this, EventArgs.Empty);

		if (ToolTip != null)
			Gwen.Net.ToolTip.Enable(this);
		else if (Parent != null && Parent.ToolTip != null)
			Gwen.Net.ToolTip.Enable(Parent);

		Redraw();
	}

	/// <summary>
	/// Invokes mouse enter event (used by input system).
	/// </summary>
	internal void InputMouseEntered() {
		OnMouseEntered();
	}

	/// <summary>
	/// Handler invoked on mouse cursor leaving control's bounds.
	/// </summary>
	protected virtual void OnMouseLeft() {
		if (HoverLeave != null)
			HoverLeave.Invoke(this, EventArgs.Empty);

		if (ToolTip != null)
			Gwen.Net.ToolTip.Disable(this);

		Redraw();
	}

	/// <summary>
	/// Invokes mouse leave event (used by input system).
	/// </summary>
	internal void InputMouseLeft() {
		OnMouseLeft();
	}

	/// <summary>
	/// Focuses the control.
	/// </summary>
	public virtual void Focus() {
		if (InputHandler.KeyboardFocus == this)
			return;

		if (InputHandler.KeyboardFocus != null)
			InputHandler.KeyboardFocus.OnLostKeyboardFocus();

		InputHandler.KeyboardFocus = this;
		OnKeyboardFocus();
		Redraw();
	}

	/// <summary>
	/// Unfocuses the control.
	/// </summary>
	public virtual void Blur() {
		if (InputHandler.KeyboardFocus != this)
			return;

		InputHandler.KeyboardFocus = null;
		OnLostKeyboardFocus();
		Redraw();
	}

	/// <summary>
	/// Control has been clicked - invoked by input system. Windows use it to propagate activation.
	/// </summary>
	public virtual void Touch() {
		if (Parent != null)
			Parent.OnChildTouched(this);
	}

	protected virtual void OnChildTouched(ControlBase control) {
		Touch();
	}

	/// <summary>
	/// Gets a child by its coordinates.
	/// </summary>
	/// <param name="x">Child X.</param>
	/// <param name="y">Child Y.</param>
	/// <returns>Control or null if not found.</returns>
	public virtual ControlBase? GetControlAt(int x, int y) {
		if (IsHidden || IsCollapsed)
			return null;

		if (x < 0 || y < 0 || x >= ActualWidth || y >= ActualHeight)
			return null;

		// todo: convert to linq FindLast
		var rev = ((IList<ControlBase>)children).Reverse(); // IList.Reverse creates new list, List.Reverse works in place.. go figure
		foreach (ControlBase child in rev) {
			ControlBase? found = child.GetControlAt(x - child.ActualLeft, y - child.ActualTop);
			if(found != null) {
				return found;
			}
		}

		if (!MouseInputEnabled)
			return null;

		return this;
	}

	/// <summary>
	/// Override this method if you need to customize the layout process.
	/// </summary>
	/// <param name="availableSize">Available size for the control. The control doesn't need to use all the space that is available.</param>
	/// <returns>Minimum size that the control needs to draw itself correctly.</returns>
	protected virtual Size Measure(Size availableSize) {
		int parentWidth = padding.Left + padding.Right;
		int parentHeight = padding.Top + padding.Bottom;
		int childrenWidth = padding.Left + padding.Right;
		int childrenHeight = padding.Top + padding.Bottom;

		foreach (ControlBase child in children) {
			if (child.IsCollapsed)
				continue;

			Dock dock = child.Dock;

			if (dock == Dock.None || dock == Dock.Fill)
				continue;

			Size childSize = new Size(Math.Max(0, availableSize.Width - childrenWidth), Math.Max(0, availableSize.Height - childrenHeight));

			childSize = child.DoMeasure(childSize);

			switch (child.Dock) {
				case Dock.Left:
				case Dock.Right:
					parentHeight = Math.Max(parentHeight, childrenHeight + childSize.Height);
					childrenWidth += childSize.Width;
					break;
				case Dock.Top:
				case Dock.Bottom:
					parentWidth = Math.Max(parentWidth, childrenWidth + childSize.Width);
					childrenHeight += childSize.Height;
					break;
			}
		}

		foreach (ControlBase child in children) {
			if (child.IsCollapsed)
				continue;

			Dock dock = child.Dock;

			if (dock != Dock.Fill)
				continue;

			Size childSize = new Size(Math.Max(0, availableSize.Width - childrenWidth), Math.Max(0, availableSize.Height - childrenHeight));

			childSize = child.DoMeasure(childSize);

			parentWidth = Math.Max(parentWidth, childrenWidth + childSize.Width);
			parentHeight = Math.Max(parentHeight, childrenHeight + childSize.Height);
		}

		foreach (ControlBase child in children) {
			if (child.IsCollapsed)
				continue;

			Dock dock = child.Dock;

			if (dock != Dock.None)
				continue;

			Size childSize = child.DoMeasure(availableSize);

			parentWidth = Math.Max(parentWidth, child.Left + childSize.Width);
			parentHeight = Math.Max(parentHeight, child.Top + childSize.Height);
		}

		parentWidth = Math.Max(parentWidth, childrenWidth);
		parentHeight = Math.Max(parentHeight, childrenHeight);

		return new Size(parentWidth, parentHeight);
	}

	/// <summary>
	/// Call this method for all child controls.
	/// </summary>
	/// <param name="availableWidth">Width that is available for the control.</param>
	/// <param name="availableHeight">Height that is available for the control.</param>
	/// <returns>Minimum size that the control needs to draw itself correctly.</returns>
	public Size DoMeasure(int availableWidth, int availableHeight) {
		return DoMeasure(new Size(availableWidth, availableHeight));
	}

	/// <summary>
	/// Call this method for all child controls.
	/// </summary>
	/// <param name="availableSize">Size that is available for the control.</param>
	/// <returns>Minimum size that the control needs to draw itself correctly.</returns>
	public Size DoMeasure(Size availableSize) {
		availableSize -= margin;

		if (!Util.IsIgnore(desiredBounds.Width))
			availableSize.Width = Math.Min(availableSize.Width, desiredBounds.Width);
		if (!Util.IsIgnore(desiredBounds.Height))
			availableSize.Height = Math.Min(availableSize.Height, desiredBounds.Height);

		availableSize.Width = Util.Clamp(availableSize.Width, minimumSize.Width, maximumSize.Width);
		availableSize.Height = Util.Clamp(availableSize.Height, minimumSize.Height, maximumSize.Height);

		Size size = Measure(availableSize);
		if (Util.IsInfinity(size.Width) || Util.IsInfinity(size.Height))
			throw new InvalidOperationException("Measured size cannot be infinity.");

		if (!Util.IsIgnore(desiredBounds.Width))
			size.Width = desiredBounds.Width;
		if (!Util.IsIgnore(desiredBounds.Height))
			size.Height = desiredBounds.Height;

		size.Width = Util.Clamp(size.Width, minimumSize.Width, maximumSize.Width);
		size.Height = Util.Clamp(size.Height, minimumSize.Height, maximumSize.Height);

		if (size.Width > availableSize.Width)
			size.Width = availableSize.Width;
		if (size.Height > availableSize.Height)
			size.Height = availableSize.Height;

		size += margin;

		measuredSize = size;

		return measuredSize;
	}

	/// <summary>
	/// Override this method if you need to customize the layout process. Usually, if you override Measure, you also need to override Arrange.
	/// </summary>
	/// <param name="finalSize">Space that the control should fill.</param>
	/// <returns>Space that the control filled.</returns>
	protected virtual Size Arrange(Size finalSize) {
		int childrenLeft = padding.Left;
		int childrenTop = padding.Top;
		int childrenRight = padding.Right;
		int childrenBottom = padding.Bottom;

		foreach (ControlBase child in children) {
			if (child.IsCollapsed)
				continue;

			Dock dock = child.Dock;

			if (dock == Dock.None || dock == Dock.Fill)
				continue;

			Size childSize = child.MeasuredSize;
			Rectangle bounds = new Rectangle(childrenLeft, childrenTop, Math.Max(0, finalSize.Width - (childrenLeft + childrenRight)), Math.Max(0, finalSize.Height - (childrenTop + childrenBottom)));

			switch (dock) {
				case Dock.Left:
					childrenLeft += childSize.Width;
					bounds.Width = childSize.Width;
					break;
				case Dock.Right:
					childrenRight += childSize.Width;
					bounds.X = Math.Max(0, finalSize.Width - childrenRight);
					bounds.Width = childSize.Width;
					break;
				case Dock.Top:
					childrenTop += childSize.Height;
					bounds.Height = childSize.Height;
					break;
				case Dock.Bottom:
					childrenBottom += childSize.Height;
					bounds.Y = Math.Max(0, finalSize.Height - childrenBottom);
					bounds.Height = childSize.Height;
					break;
			}

			child.DoArrange(bounds);
		}

		foreach (ControlBase child in children) {
			if (child.IsCollapsed)
				continue;

			Dock dock = child.Dock;

			if (dock != Dock.Fill)
				continue;

			Rectangle bounds = new Rectangle(childrenLeft, childrenTop, Math.Max(0, finalSize.Width - (childrenLeft + childrenRight)), Math.Max(0, finalSize.Height - (childrenTop + childrenBottom)));

			innerBounds = bounds;

			child.DoArrange(bounds);
		}

		foreach (ControlBase child in children) {
			if (child.IsCollapsed)
				continue;

			Dock dock = child.Dock;

			if (dock != Dock.None)
				continue;

			Size childSize = child.MeasuredSize;
			Rectangle bounds = new Rectangle(child.Left, child.Top, finalSize.Width - child.Left, finalSize.Height - child.Top);

			child.DoArrange(bounds);
		}

		return finalSize;
	}

	/// <summary>
	/// Call this method for all child controls.
	/// </summary>
	/// <param name="x">Final horizontal location. This includes margins.</param>
	/// <param name="y">Final vertical location. This includes margins.</param>
	/// <param name="width">Final width of the control. This includes margins.</param>
	/// <param name="height">Final height of the control. This includes margins.</param>
	public void DoArrange(int x, int y, int width, int height) {
		DoArrange(new Rectangle(x, y, width, height));
	}

	/// <summary>
	/// Call this method for all child controls.
	/// </summary>
	/// <param name="finalRect">Final location and size of the control. This includes margins.</param>
	public void DoArrange(Rectangle finalRect) {
		Size finalSize = finalRect.Size;

		HorizontalAlignment halign = HorizontalAlignment;
		VerticalAlignment valign = VerticalAlignment;

		if (halign != HorizontalAlignment.Stretch)
			finalSize.Width = measuredSize.Width;
		if (valign != VerticalAlignment.Stretch)
			finalSize.Height = measuredSize.Height;

		finalSize -= margin;

		if (!Util.IsIgnore(desiredBounds.Width))
			finalSize.Width = Math.Min(finalRect.Width, desiredBounds.Width);
		if (!Util.IsIgnore(desiredBounds.Height))
			finalSize.Height = Math.Min(finalRect.Height, desiredBounds.Height);

		Size arrangedSize = Arrange(finalSize);

		if (!Util.IsIgnore(desiredBounds.Width))
			arrangedSize.Width = desiredBounds.Width;
		else if (halign == HorizontalAlignment.Stretch)
			arrangedSize.Width = finalSize.Width;

		if (!Util.IsIgnore(desiredBounds.Height))
			arrangedSize.Height = desiredBounds.Height;
		else if (valign == VerticalAlignment.Stretch)
			arrangedSize.Height = finalSize.Height;

		arrangedSize.Width = Util.Clamp(arrangedSize.Width, minimumSize.Width, maximumSize.Width);
		arrangedSize.Height = Util.Clamp(arrangedSize.Height, minimumSize.Height, maximumSize.Height);

		if (arrangedSize.Width > finalSize.Width)
			arrangedSize.Width = finalSize.Width;
		if (arrangedSize.Height > finalSize.Height)
			arrangedSize.Height = finalSize.Height;

		Size areaSize = finalRect.Size;
		areaSize -= margin;

		Point offset = Point.Zero;
		if (halign == HorizontalAlignment.Center)
			offset.X = (areaSize.Width - arrangedSize.Width) / 2;
		else if (halign == HorizontalAlignment.Right)
			offset.X = areaSize.Width - arrangedSize.Width;
		if (valign == VerticalAlignment.Center)
			offset.Y = (areaSize.Height - arrangedSize.Height) / 2;
		else if (valign == VerticalAlignment.Bottom)
			offset.Y = areaSize.Height - arrangedSize.Height;

		SetBounds(finalRect.Left + margin.Left + offset.X, finalRect.Top + margin.Top + offset.Y, arrangedSize.Width, arrangedSize.Height);

		NeedsLayout = false;
		LayoutDone = true;
	}

	/// <summary>
	/// Invoke the layout process for the control and it's children.
	/// </summary>
	public virtual void DoLayout() {
		Measure(bounds.Size);
		Arrange(bounds.Size);

		NeedsLayout = false;
		LayoutDone = true;
	}

	/// <summary>
	/// Recursively check tabs, focus etc.
	/// </summary>
	protected virtual void RecurseControls() {
		if (IsHidden || IsCollapsed)
			return;

		foreach (ControlBase child in Children) {
			child.RecurseControls();
		}

		if (IsTabable) {
			if (GetCanvas().firstTab == null)
				GetCanvas().firstTab = this;
			if (GetCanvas().nextTab == null)
				GetCanvas().nextTab = this;
		}

		if (InputHandler.KeyboardFocus == this) {
			GetCanvas().nextTab = null;
		}
	}

	/// <summary>
	/// Checks if the given control is a child of this instance.
	/// </summary>
	/// <param name="child">Control to examine.</param>
	/// <returns>True if the control is our child.</returns>
	public bool IsChild(ControlBase child) {
		return children.Contains(child);
	}

	/// <summary>
	/// Converts local coordinates to canvas coordinates.
	/// </summary>
	/// <param name="pnt">Local coordinates.</param>
	/// <returns>Canvas coordinates.</returns>
	public virtual Point LocalPosToCanvas(Point pnt) {
		if (actualParent != null) {
			int x = pnt.X + ActualLeft;
			int y = pnt.Y + ActualTop;

			return actualParent.LocalPosToCanvas(new Point(x, y));
		}

		return pnt;
	}

	/// <summary>
	/// Converts canvas coordinates to local coordinates.
	/// </summary>
	/// <param name="pnt">Canvas coordinates.</param>
	/// <returns>Local coordinates.</returns>
	public virtual Point CanvasPosToLocal(Point pnt) {
		if (actualParent != null) {
			int x = pnt.X - ActualLeft;
			int y = pnt.Y - ActualTop;

			return actualParent.CanvasPosToLocal(new Point(x, y));
		}

		return pnt;
	}

	/// <summary>
	/// Closes all menus recursively.
	/// </summary>
	public virtual void CloseMenus() {
		// todo: not very efficient with the copying and recursive closing, maybe store currently open menus somewhere (canvas)?
		var childrenCopy = children.ToArray();
		foreach (ControlBase child in childrenCopy) {
			child.CloseMenus();
		}
	}

	/// <summary>
	/// Copies Bounds to RenderBounds.
	/// </summary>
	protected virtual void UpdateRenderBounds() {
		renderBounds.X = 0;
		renderBounds.Y = 0;

		renderBounds.Width = bounds.Width;
		renderBounds.Height = bounds.Height;
	}

	/// <summary>
	/// Sets mouse cursor to current cursor.
	/// </summary>
	public virtual void UpdateCursor() {
		Platform.GwenPlatform.SetCursor(cursor);
	}

	// giver
	public virtual Package? DragAndDrop_GetPackage(int x, int y) {
		return dragAndDropPackage;
	}

	// giver
	public virtual bool DragAndDrop_Draggable() {
		if (dragAndDropPackage == null)
			return false;

		return dragAndDropPackage.IsDraggable;
	}

	// giver
	public virtual void DragAndDrop_SetPackage(bool draggable, string name = "", object? userData = null) {
		if (dragAndDropPackage == null) {
			dragAndDropPackage = new Package {
				IsDraggable = draggable,
				Name = name,
				UserData = userData
			};
		}
	}

	// giver
	public virtual bool DragAndDrop_ShouldStartDrag() {
		return true;
	}

	// giver
	public virtual void DragAndDrop_StartDragging(Package package, int x, int y) {
		package.HoldOffset = CanvasPosToLocal(new Point(x, y));
		package.DrawControl = this;
	}

	// giver
	public virtual void DragAndDrop_EndDragging(bool success, int x, int y) {
	}

	// receiver
	public virtual bool DragAndDrop_HandleDrop(Package p, int x, int y) {
		if(DragAndDrop.SourceControl != null) {
			DragAndDrop.SourceControl.Parent = this;
		}
		return true;
	}

	// receiver
	public virtual void DragAndDrop_HoverEnter(Package p, int x, int y) {

	}

	// receiver
	public virtual void DragAndDrop_HoverLeave(Package p) {

	}

	// receiver
	public virtual void DragAndDrop_Hover(Package p, int x, int y) {

	}

	// receiver
	public virtual bool DragAndDrop_CanAcceptPackage(Package p) {
		return false;
	}

	/// <summary>
	/// Handles keyboard accelerator.
	/// </summary>
	/// <param name="accelerator">Accelerator text.</param>
	/// <returns>True if handled.</returns>
	internal virtual bool HandleAccelerator(string accelerator) {
		if (InputHandler.KeyboardFocus == this || !AccelOnlyFocus) {
			if (accelerators.ContainsKey(accelerator)) {
				accelerators[accelerator].Invoke(this, EventArgs.Empty);
				return true;
			}
		}

		return children.Any(child => child.HandleAccelerator(accelerator));
	}

	/// <summary>
	/// Adds keyboard accelerator.
	/// </summary>
	/// <param name="accelerator">Accelerator text.</param>
	/// <param name="handler">Handler.</param>
	public void AddAccelerator(string accelerator, GwenEventHandler<EventArgs> handler) {
		accelerator = accelerator.Trim().ToUpperInvariant();
		accelerators[accelerator] = handler;
	}

	/// <summary>
	/// Adds keyboard accelerator with a default handler.
	/// </summary>
	/// <param name="accelerator">Accelerator text.</param>
	public void AddAccelerator(string accelerator) {
		accelerators[accelerator] = DefaultAcceleratorHandler;
	}

	/// <summary>
	/// Re-renders the control, invalidates cached texture.
	/// </summary>
	public virtual void Redraw() {
		UpdateColors();
		cacheTextureDirty = true;
		if (parent != null)
			parent.Redraw();
	}

	/// <summary>
	/// Updates control colors.
	/// </summary>
	/// <remarks>
	/// Used in composite controls like lists to differentiate row colors etc.
	/// </remarks>
	public virtual void UpdateColors() {

	}

	/// <summary>
	/// Handler for keyboard events.
	/// </summary>
	/// <param name="key">Key pressed.</param>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyPressed(GwenMappedKey key, bool down = true) {
		bool handled = false;
		switch (key) {
			case GwenMappedKey.Tab: handled = OnKeyTab(down); break;
			case GwenMappedKey.Space: handled = OnKeySpace(down); break;
			case GwenMappedKey.Home: handled = OnKeyHome(down); break;
			case GwenMappedKey.End: handled = OnKeyEnd(down); break;
			case GwenMappedKey.Return: handled = OnKeyReturn(down); break;
			case GwenMappedKey.Backspace: handled = OnKeyBackspace(down); break;
			case GwenMappedKey.Delete: handled = OnKeyDelete(down); break;
			case GwenMappedKey.Right: handled = OnKeyRight(down); break;
			case GwenMappedKey.Left: handled = OnKeyLeft(down); break;
			case GwenMappedKey.Up: handled = OnKeyUp(down); break;
			case GwenMappedKey.Down: handled = OnKeyDown(down); break;
			case GwenMappedKey.Escape: handled = OnKeyEscape(down); break;
			default: break;
		}

		if (!handled && Parent != null)
			Parent.OnKeyPressed(key, down);

		return handled;
	}

	/// <summary>
	/// Invokes key press event (used by input system).
	/// </summary>
	internal bool InputKeyPressed(GwenMappedKey key, bool down = true) {
		return OnKeyPressed(key, down);
	}

	/// <summary>
	/// Handler for keyboard events.
	/// </summary>
	/// <param name="key">Key pressed.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyReleaseed(GwenMappedKey key) {
		return OnKeyPressed(key, false);
	}

	/// <summary>
	/// Handler for Tab keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyTab(bool down) {
		if(!down) {
			return true;
		}

		ControlBase? nextTab = GetCanvas().nextTab;
		if(nextTab != null) {
			nextTab.Focus();
			Redraw();
		}

		return true;
	}

	/// <summary>
	/// Handler for Space keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeySpace(bool down) { return false; }

	/// <summary>
	/// Handler for Return keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyReturn(bool down) { return false; }

	/// <summary>
	/// Handler for Backspace keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyBackspace(bool down) { return false; }

	/// <summary>
	/// Handler for Delete keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyDelete(bool down) { return false; }

	/// <summary>
	/// Handler for Right Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyRight(bool down) { return false; }

	/// <summary>
	/// Handler for Left Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyLeft(bool down) { return false; }

	/// <summary>
	/// Handler for Home keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyHome(bool down) { return false; }

	/// <summary>
	/// Handler for End keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyEnd(bool down) { return false; }

	/// <summary>
	/// Handler for Up Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyUp(bool down) { return false; }

	/// <summary>
	/// Handler for Down Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyDown(bool down) { return false; }

	/// <summary>
	/// Handler for Escape keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnKeyEscape(bool down) { return false; }

	/// <summary>
	/// Handler for Paste event.
	/// </summary>
	/// <param name="from">Source control.</param>
	/// <param name="args">Event args.</param>
	protected virtual void OnPaste(ControlBase from, EventArgs args) {
	}

	/// <summary>
	/// Handler for Copy event.
	/// </summary>
	/// <param name="from">Source control.</param>
	/// <param name="args">Event args.</param>
	protected virtual void OnCopy(ControlBase from, EventArgs args) {
	}

	/// <summary>
	/// Handler for Cut event.
	/// </summary>
	/// <param name="from">Source control.</param>
	/// <param name="args">Event args.</param>
	protected virtual void OnCut(ControlBase from, EventArgs args) {
	}

	/// <summary>
	/// Handler for Select All event.
	/// </summary>
	/// <param name="from">Source control.</param>
	/// <param name="args">Event args.</param>
	protected virtual void OnSelectAll(ControlBase from, EventArgs args) {
	}

	internal void InputCopy(ControlBase from) {
		OnCopy(from, EventArgs.Empty);
	}

	internal void InputPaste(ControlBase from) {
		OnPaste(from, EventArgs.Empty);
	}

	internal void InputCut(ControlBase from) {
		OnCut(from, EventArgs.Empty);
	}

	internal void InputSelectAll(ControlBase from) {
		OnSelectAll(from, EventArgs.Empty);
	}

	/// <summary>
	/// Renders the focus overlay.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected virtual void RenderFocus(Skin.SkinBase skin) {
		if (InputHandler.KeyboardFocus != this)
			return;
		if (!IsTabable)
			return;

		skin.DrawKeyboardHighlight(this, RenderBounds, 3);
	}

	/// <summary>
	/// Renders under the actual control (shadows etc).
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected virtual void RenderUnder(Skin.SkinBase skin) {

	}

	/// <summary>
	/// Renders over the actual control (overlays).
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected virtual void RenderOver(Skin.SkinBase skin) {

	}

	/// <summary>
	/// Called during rendering.
	/// </summary>
	public virtual void Think() {

	}

	/// <summary>
	/// Handler for gaining keyboard focus.
	/// </summary>
	protected virtual void OnKeyboardFocus() {

	}

	/// <summary>
	/// Handler for losing keyboard focus.
	/// </summary>
	protected virtual void OnLostKeyboardFocus() {

	}

	/// <summary>
	/// Handler for character input event.
	/// </summary>
	/// <param name="chr">Character typed.</param>
	/// <returns>True if handled.</returns>
	protected virtual bool OnChar(Char chr) {
		return false;
	}

	internal bool InputChar(Char chr) {
		return OnChar(chr);
	}

#if false
		public virtual void Anim_WidthIn(float length, float delay = 0.0f, float ease = 1.0f)
		{
			Animation.Add(this, new Anim.Size.Width(0, ActualWidth, length, false, delay, ease));
			ActualWidth = 0;
		}

		public virtual void Anim_HeightIn(float length, float delay, float ease)
		{
			Animation.Add(this, new Anim.Size.Height(0, ActualHeight, length, false, delay, ease));
			ActualHeight = 0;
		}

		public virtual void Anim_WidthOut(float length, bool hide, float delay, float ease)
		{
			Animation.Add(this, new Anim.Size.Width(ActualWidth, 0, length, hide, delay, ease));
		}

		public virtual void Anim_HeightOut(float length, bool hide, float delay, float ease)
		{
			Animation.Add(this, new Anim.Size.Height(ActualHeight, 0, length, hide, delay, ease));
		}
#endif

	private InternalFlags internalFlags;

	private bool IsSetInternalFlag(InternalFlags flag) {
		return (internalFlags & flag) != 0;
	}

	private InternalFlags GetInternalFlag(InternalFlags mask) {
		return internalFlags & mask;
	}

	private void SetInternalFlag(InternalFlags flag, bool value) {
		if (value)
			internalFlags |= flag;
		else
			internalFlags &= ~flag;
	}

	private bool CheckAndChangeInternalFlag(InternalFlags flag, bool value) {
		bool oldValue = (internalFlags & flag) != 0;
		if (oldValue == value)
			return false;

		if (value)
			internalFlags |= flag;
		else
			internalFlags &= ~flag;

		return true;
	}

	private void SetInternalFlag(InternalFlags mask, InternalFlags flag) {
		internalFlags = (internalFlags & ~mask) | flag;
	}

	private bool CheckAndChangeInternalFlag(InternalFlags mask, InternalFlags flag) {
		if ((internalFlags & mask) == flag)
			return false;

		internalFlags = (internalFlags & ~mask) | flag;

		return true;
	}

	[Flags]
	internal enum InternalFlags {
		// AlignH
		AlignHLeft = 1 << 0,
		AlignHCenter = 1 << 1,
		AlignHRight = 1 << 2,
		AlignHStretch = 1 << 3,
		AlignH_Mask = AlignHLeft | AlignHCenter | AlignHRight | AlignHStretch,

		// AlignV
		AlignVTop = 1 << 4,
		AlignVCenter = 1 << 5,
		AlignVBottom = 1 << 6,
		AlignVStretch = 1 << 7,
		AlignV_Mask = AlignVTop | AlignVCenter | AlignVBottom | AlignVStretch,

		// Dock
		DockNone = 1 << 8,
		DockLeft = 1 << 9,
		DockTop = 1 << 10,
		DockRight = 1 << 11,
		DockBottom = 1 << 12,
		DockFill = 1 << 13,
		Dock_Mask = DockNone | DockLeft | DockTop | DockRight | DockBottom | DockFill,

		// Flags
		VirtualControl = 1 << 14,
		NeedsLayout = 1 << 15,
		LayoutDone = 1 << 16,
		Disabled = 1 << 17,
		Hidden = 1 << 18,
		Collapsed = 1 << 19,
		DrawDebugOutlines = 1 << 20,
		RestrictToParent = 1 << 21,
		MouseInputEnabled = 1 << 22,
		KeyboardInputEnabled = 1 << 23,
		DrawBackground = 1 << 24,
		Tabable = 1 << 25,
		KeyboardNeeded = 1 << 26,
	}
}