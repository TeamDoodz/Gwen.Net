using System;
using System.Collections.Generic;
using Gwen.Net.Anim;
using Gwen.Net.DragDrop;
using Gwen.Net.Input;

namespace Gwen.Net.Control;

/// <summary>
/// Canvas control. It should be the root parent for all other controls.
/// </summary>
public class Canvas : ControlBase {
	private bool needsRedraw;
	private float scale;

	private Color backgroundColor;

	// [omeg] these are not created by us, so no disposing
	internal ControlBase? firstTab;
	internal ControlBase? nextTab;

	private readonly List<IDisposable> disposeQueue; // dictionary for faster access?

	private readonly HashSet<ControlBase> measureQueue = new HashSet<ControlBase>();

	/// <summary>
	/// Scale for rendering.
	/// </summary>
	public override float Scale {
		get { return scale; }
		set {
			if(scale == value)
				return;

			scale = value;

			if(Skin != null && Skin.Renderer != null)
				Skin.Renderer.Scale = scale;

			OnScaleChanged();
			Redraw();
			Invalidate();
		}
	}

	/// <summary>
	/// Background color.
	/// </summary>
	public Color BackgroundColor { get { return backgroundColor; } set { backgroundColor = value; } }

	/// <summary>
	/// In most situations you will be rendering the canvas every frame. 
	/// But in some situations you will only want to render when there have been changes. 
	/// You can do this by checking NeedsRedraw.
	/// </summary>
	public bool NeedsRedraw { get { return needsRedraw; } set { needsRedraw = value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="Canvas"/> class.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	public Canvas(Skin.SkinBase skin) {
		Dock = Dock.Fill;
		SetBounds(0, 0, 10000, 10000);
		SetSkin(skin);
		Scale = 1.0f;
		BackgroundColor = Color.White;
		ShouldDrawBackground = false;

		disposeQueue = new List<IDisposable>();
	}

	public override void Dispose() {
		ProcessDelayedDeletes();

		// Dispose all cached fonts.
		FontCache.FreeCache();

		base.Dispose();
	}

	/// <summary>
	/// Re-renders the control, invalidates cached texture.
	/// </summary>
	public override void Redraw() {
		NeedsRedraw = true;
		base.Redraw();
	}

	// Children call parent.GetCanvas() until they get to 
	// this top level function.
	public override Canvas GetCanvas() {
		return this;
	}

	/// <summary>
	/// Renders the canvas. Call in your rendering loop.
	/// </summary>
	public void RenderCanvas() {
		DoThink();

		Skin.SkinBase skin = Skin;
		Renderer.RendererBase render = skin.Renderer;

		render.Begin();

		render.ClipRegion = Bounds;
		render.RenderOffset = Point.Zero;

		if(ShouldDrawBackground) {
			render.DrawColor = backgroundColor;
			render.DrawFilledRect(RenderBounds);
		}

		DoRender(skin);

		DragAndDrop.RenderOverlay(this, skin);

		Gwen.Net.ToolTip.RenderToolTip(skin);

		render.EndClip();

		render.End();
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		base.Render(skin);
		needsRedraw = false;
	}

	/// <summary>
	/// Handler invoked when control's bounds change.
	/// </summary>
	/// <param name="oldBounds">Old bounds.</param>
	protected override void OnBoundsChanged(Rectangle oldBounds) {
		base.OnBoundsChanged(oldBounds);
		Invalidate();
	}

	/// <summary>
	/// Processes input and layout. Also purges delayed delete queue.
	/// </summary>
	private void DoThink() {
		if(IsHidden || IsCollapsed)
			return;

		Animation.GlobalThink();

		// Reset tabbing
		nextTab = null;
		firstTab = null;

		ProcessDelayedDeletes();

		// Check has focus etc..
		RecurseControls();

		// If we didn't have a next tab, cycle to the start.
		if(nextTab == null)
			nextTab = firstTab;

		InputHandler.OnCanvasThink(this);

		// Is total layout needed
		if(this.NeedsLayout) {
			DoLayout();
		}

		// Check if individual controls need layout
		if(measureQueue.Count > 0) {
			foreach(ControlBase element in measureQueue) {
				element.DoLayout();
			}

			measureQueue.Clear();
		}
	}

	/// <summary>
	/// Adds given control to the delete queue and detaches it from canvas. Don't call from Dispose, it modifies child list.
	/// </summary>
	/// <param name="control">Control to delete.</param>
	public void AddDelayedDelete(ControlBase control) {
		if(!disposeQueue.Contains(control)) {
			disposeQueue.Add(control);
			RemoveChild(control, false);
		}
#if DEBUG
		else
			throw new InvalidOperationException("Control deleted twice");
#endif
	}

	private void ProcessDelayedDeletes() {
		//if (m_DisposeQueue.Count > 0)
		//    System.Diagnostics.Debug.Print("Canvas.ProcessDelayedDeletes: {0} items", m_DisposeQueue.Count);
		foreach(IDisposable control in disposeQueue) {
			control.Dispose();
		}
		disposeQueue.Clear();
	}

	public void AddToMeasure(ControlBase element) {
		measureQueue.Add(element);
	}

	/// <summary>
	/// Handles mouse movement events. Called from Input subsystems.
	/// </summary>
	/// <returns>True if handled.</returns>
	public bool Input_MouseMoved(int x, int y, int dx, int dy) {
		if(IsHidden || IsCollapsed)
			return false;

		// Todo: Handle scaling here..
		//float fScale = 1.0f / Scale();

		return InputHandler.OnMouseMoved(this, x, y, dx, dy);
	}

	/// <summary>
	/// Handles mouse button events. Called from Input subsystems.
	/// </summary>
	/// <returns>True if handled.</returns>
	public bool Input_MouseButton(int button, bool down) {
		if(IsHidden || IsCollapsed) return false;

		return InputHandler.OnMouseClicked(this, button, down);
	}

	/// <summary>
	/// Handles keyboard events. Called from Input subsystems.
	/// </summary>
	/// <returns>True if handled.</returns>
	public bool Input_Key(GwenMappedKey key, bool down) {
		if(IsHidden || IsCollapsed) return false;
		if(key <= GwenMappedKey.Invalid) return false;
		if(key >= GwenMappedKey.Count) return false;

		return InputHandler.OnKeyEvent(this, key, down);
	}

	/// <summary>
	/// Handles keyboard events. Called from Input subsystems.
	/// </summary>
	/// <returns>True if handled.</returns>
	public bool Input_Character(char chr) {
		if(IsHidden || IsCollapsed) return false;
		if(char.IsControl(chr)) return false;

		//Handle Accelerators
		if(InputHandler.HandleAccelerator(this, chr))
			return true;

		//Handle characters
		if(InputHandler.KeyboardFocus == null) return false;
		if(InputHandler.KeyboardFocus.GetCanvas() != this) return false;
		if(!InputHandler.KeyboardFocus.IsVisible) return false;
		if(InputHandler.IsControlDown) return false;

		return InputHandler.KeyboardFocus.InputChar(chr);
	}

	/// <summary>
	/// Handles the mouse wheel events. Called from Input subsystems.
	/// </summary>
	/// <returns>True if handled.</returns>
	public bool Input_MouseWheel(int val) {
		if(IsHidden || IsCollapsed) return false;
		if(InputHandler.HoveredControl == null) return false;
		if(InputHandler.HoveredControl == this) return false;
		if(InputHandler.HoveredControl.GetCanvas() != this) return false;

		return InputHandler.HoveredControl.InputMouseWheeled(val);
	}
}