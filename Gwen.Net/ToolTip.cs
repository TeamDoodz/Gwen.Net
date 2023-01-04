using Gwen.Net.Control;

namespace Gwen.Net;

/// <summary>
/// Tooltip handling.
/// </summary>
public static class ToolTip {
	private static ControlBase? instance;

	/// <summary>
	/// Enables tooltip display for the specified control.
	/// </summary>
	/// <param name="control">Target control.</param>
	public static void Enable(ControlBase control) {
		if(null == control.ToolTip)
			return;

		ControlBase toolTip = control.ToolTip;
		instance = control;
		toolTip.DoMeasure(Size.Infinity);
		toolTip.DoArrange(new Rectangle(Point.Zero, toolTip.MeasuredSize));
	}

	/// <summary>
	/// Disables tooltip display for the specified control.
	/// </summary>
	/// <param name="control">Target control.</param>
	public static void Disable(ControlBase control) {
		if(instance == control) {
			instance = null;
		}
	}

	/// <summary>
	/// Disables tooltip display for the specified control.
	/// </summary>
	/// <param name="control">Target control.</param>
	public static void ControlDeleted(ControlBase control) {
		Disable(control);
	}

	/// <summary>
	/// Renders the currently visible tooltip.
	/// </summary>
	/// <param name="skin"></param>
	public static void RenderToolTip(Skin.SkinBase skin) {
		if(instance == null || instance.ToolTip == null) return;

		Renderer.RendererBase render = skin.Renderer;

		Point oldRenderOffset = render.RenderOffset;
		Point mousePos = Input.InputHandler.MousePosition;
		Rectangle bounds = instance.ToolTip.Bounds;

		Rectangle offset = Util.FloatRect(mousePos.X - bounds.Width / 2, mousePos.Y - bounds.Height - 10, bounds.Width, bounds.Height);
		offset = Rectangle.ClampRectToRect(offset, instance.GetCanvas().Bounds);

		//Calculate offset on screen bounds
		render.AddRenderOffset(offset);
		render.EndClip();

		skin.DrawToolTip(instance.ToolTip);
		instance.ToolTip.DoRender(skin);

		render.RenderOffset = oldRenderOffset;
	}
}