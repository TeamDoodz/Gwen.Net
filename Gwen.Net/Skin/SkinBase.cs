﻿using System;

namespace Gwen.Net.Skin;

/// <summary>
/// Base skin.
/// </summary>
public class SkinBase : IDisposable {
	protected Font defaultFont;
	protected readonly Renderer.RendererBase renderer;
	protected int baseUnit;

	/// <summary>
	/// Colors of various UI elements.
	/// </summary>
	public SkinColors Colors;

	/// <summary>
	/// Default font to use when rendering text if none specified.
	/// </summary>
	public Font DefaultFont {
		get { return defaultFont; }
		set {
			if (defaultFont != null)
				defaultFont.Dispose();

			defaultFont = value;

			baseUnit = Util.Ceil(defaultFont.FontMetrics.EmHeightPixels) + 1;
		}
	}

	/// <summary>
	/// Base measurement unit based on default font size used in various controls where absolute scale is necessary.
	/// </summary>
	public int BaseUnit { get { return baseUnit; } }

	/// <summary>
	/// Renderer used.
	/// </summary>
	public Renderer.RendererBase Renderer { get { return renderer; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="SkinBase"/> class.
	/// </summary>
	/// <param name="renderer">Renderer to use.</param>
	protected SkinBase(Renderer.RendererBase renderer) {
		this.renderer = renderer;

		DefaultFont = new Font(renderer);
		if(defaultFont == null) throw new Exception("Impossible");
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public virtual void Dispose() {
		defaultFont.Dispose();
		GC.SuppressFinalize(this);
	}

#if DEBUG
        ~SkinBase()
        {
            throw new InvalidOperationException(String.Format("IDisposable object finalized: {0}", GetType()));
            //Debug.Print(String.Format("IDisposable object finalized: {0}", GetType()));
        }
#endif

	/// <summary>
	/// Releases the specified font.
	/// </summary>
	/// <param name="font">Font to release.</param>
	protected virtual void ReleaseFont(Font font) {
		if (font == null)
			return;
		if (renderer == null)
			return;
		renderer.FreeFont(font);
	}

	/// <summary>
	/// Sets the default text font.
	/// </summary>
	/// <param name="faceName">Font name. Meaning can vary depending on the renderer.</param>
	/// <param name="size">Font size.</param>
	public virtual void SetDefaultFont(string faceName, int size = 10) {
		defaultFont.FaceName = faceName;
		defaultFont.Size = size;
	}

	#region UI elements
	public virtual void DrawButton(Control.ControlBase control, bool depressed, bool hovered, bool disabled) { }
	public virtual void DrawTabButton(Control.ControlBase control, bool active, Dock dir) { }
	public virtual void DrawTabControl(Control.ControlBase control) { }
	public virtual void DrawTabTitleBar(Control.ControlBase control) { }
	public virtual void DrawMenuItem(Control.ControlBase control, bool submenuOpen, bool isChecked) { }
	public virtual void DrawMenuRightArrow(Control.ControlBase control) { }
	public virtual void DrawMenuStrip(Control.ControlBase control) { }
	public virtual void DrawMenu(Control.ControlBase control, bool paddingDisabled) { }
	public virtual void DrawRadioButton(Control.ControlBase control, bool selected, bool depressed) { }
	public virtual void DrawCheckBox(Control.ControlBase control, bool selected, bool depressed) { }
	public virtual void DrawGroupBox(Control.ControlBase control, int textStart, int textHeight, int textWidth) { }
	public virtual void DrawTextBox(Control.ControlBase control) { }
	public virtual void DrawWindow(Control.ControlBase control, int topHeight, bool inFocus) { }
	public virtual void DrawWindowCloseButton(Control.ControlBase control, bool depressed, bool hovered, bool disabled) { }
	public virtual void DrawToolWindow(Control.ControlBase control, bool vertical, int dragSize) { }
	public virtual void DrawHighlight(Control.ControlBase control) { }
	public virtual void DrawStatusBar(Control.ControlBase control) { }
	public virtual void DrawShadow(Control.ControlBase control) { }
	public virtual void DrawScrollBarBar(Control.ControlBase control, bool depressed, bool hovered, bool horizontal) { }
	public virtual void DrawScrollBar(Control.ControlBase control, bool horizontal, bool depressed) { }
	public virtual void DrawScrollButton(Control.ControlBase control, Control.Internal.ScrollBarButtonDirection direction, bool depressed, bool hovered, bool disabled) { }
	public virtual void DrawProgressBar(Control.ControlBase control, bool horizontal, float progress) { }
	public virtual void DrawListBox(Control.ControlBase control) { }
	public virtual void DrawListBoxLine(Control.ControlBase control, bool selected, bool even) { }
	public virtual void DrawSlider(Control.ControlBase control, bool horizontal, int numNotches, int barSize) { }
	public virtual void DrawSliderButton(Control.ControlBase control, bool depressed, bool horizontal) { }
	public virtual void DrawComboBox(Control.ControlBase control, bool down, bool isMenuOpen) { }
	public virtual void DrawComboBoxArrow(Control.ControlBase control, bool hovered, bool depressed, bool open, bool disabled) { }
	public virtual void DrawKeyboardHighlight(Control.ControlBase control, Rectangle rect, int offset) { }
	public virtual void DrawToolTip(Control.ControlBase control) { }
	public virtual void DrawNumericUpDownButton(Control.ControlBase control, bool depressed, bool up) { }
	public virtual void DrawTreeButton(Control.ControlBase control, bool open) { }
	public virtual void DrawTreeControl(Control.ControlBase control) { }
	public virtual void DrawBorder(Control.ControlBase control, Control.BorderType borderType) { }

	public virtual void DrawDebugOutlines(Control.ControlBase control) {
		renderer.DrawColor = control.PaddingOutlineColor;
		Rectangle inner = control.Bounds;
		inner.Deflate(control.Padding);
		renderer.DrawLinedRect(inner);

		renderer.DrawColor = control.MarginOutlineColor;
		Rectangle outer = control.Bounds;
		outer.Inflate(control.Margin);
		renderer.DrawLinedRect(outer);

		renderer.DrawColor = control.BoundsOutlineColor;
		renderer.DrawLinedRect(control.Bounds);
	}

	public virtual void DrawTreeNode(Control.ControlBase ctrl, bool open, bool selected, int labelHeight, int labelWidth, int halfWay, int lastBranch, bool isRoot, int indent) {
		Renderer.DrawColor = Colors.Tree.Lines;

		if (!isRoot)
			Renderer.DrawFilledRect(new Rectangle(indent / 2, halfWay, indent / 2, 1));

		if (!open) return;

		Renderer.DrawFilledRect(new Rectangle(indent + indent / 2, labelHeight + 1, 1, lastBranch + halfWay - labelHeight));
	}

	public virtual void DrawPropertyRow(Control.ControlBase control, int iWidth, bool bBeingEdited, bool hovered) {
		Rectangle rect = control.RenderBounds;

		if (bBeingEdited)
			renderer.DrawColor = Colors.Properties.Column_Selected;
		else if (hovered)
			renderer.DrawColor = Colors.Properties.Column_Hover;
		else
			renderer.DrawColor = Colors.Properties.Column_Normal;

		renderer.DrawFilledRect(new Rectangle(0, rect.Y, iWidth, rect.Height));

		if (bBeingEdited)
			renderer.DrawColor = Colors.Properties.Line_Selected;
		else if (hovered)
			renderer.DrawColor = Colors.Properties.Line_Hover;
		else
			renderer.DrawColor = Colors.Properties.Line_Normal;

		renderer.DrawFilledRect(new Rectangle(iWidth, rect.Y, 1, rect.Height));

		rect.Y += rect.Height - 1;
		rect.Height = 1;

		renderer.DrawFilledRect(rect);
	}

	public virtual void DrawColorDisplay(Control.ControlBase control, Color color) { }
	public virtual void DrawModalControl(Control.ControlBase control, Color? backgroundColor) { }
	public virtual void DrawMenuDivider(Control.ControlBase control) { }
	public virtual void DrawCategoryHolder(Control.ControlBase control) { }
	public virtual void DrawCategoryInner(Control.ControlBase control, int headerHeight, bool collapsed) { }

	public virtual void DrawPropertyTreeNode(Control.ControlBase control, int BorderLeft, int BorderTop) {
		Rectangle rect = control.RenderBounds;

		renderer.DrawColor = Colors.Properties.Border;

		renderer.DrawFilledRect(new Rectangle(rect.X, rect.Y, BorderLeft, rect.Height));
		renderer.DrawFilledRect(new Rectangle(rect.X + BorderLeft, rect.Y, rect.Width - BorderLeft, BorderTop));
	}
	#endregion

	#region Symbols for Simple skin
	/*
	Here we're drawing a few symbols such as the directional arrows and the checkbox check

	Texture'd skins don't generally use these - but the Simple skin does. We did originally
	use the marlett font to draw these.. but since that's a Windows font it wasn't a very
	good cross platform solution.
	*/

	public virtual void DrawArrowDown(Rectangle rect) {
		float x = (rect.Width / 5.0f);
		float y = (rect.Height / 5.0f);

		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 0.0f, rect.Y + y * 1.0f, x, y * 1.0f));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 1.0f, x, y * 2.0f));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 1.0f, x, y * 3.0f));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 1.0f, x, y * 2.0f));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 4.0f, rect.Y + y * 1.0f, x, y * 1.0f));
	}

	public virtual void DrawArrowUp(Rectangle rect) {
		float x = (rect.Width / 5.0f);
		float y = (rect.Height / 5.0f);

		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 0.0f, rect.Y + y * 3.0f, x, y * 1.0f));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 2.0f, x, y * 2.0f));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 1.0f, x, y * 3.0f));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 2.0f, x, y * 2.0f));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 4.0f, rect.Y + y * 3.0f, x, y * 1.0f));
	}

	public virtual void DrawArrowLeft(Rectangle rect) {
		float x = (rect.Width / 5.0f);
		float y = (rect.Height / 5.0f);

		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 0.0f, x * 1.0f, y));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 1.0f, x * 2.0f, y));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 2.0f, x * 3.0f, y));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 3.0f, x * 2.0f, y));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 4.0f, x * 1.0f, y));
	}

	public virtual void DrawArrowRight(Rectangle rect) {
		float x = (rect.Width / 5.0f);
		float y = (rect.Height / 5.0f);

		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 0.0f, x * 1.0f, y));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 1.0f, x * 2.0f, y));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 2.0f, x * 3.0f, y));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 3.0f, x * 2.0f, y));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 4.0f, x * 1.0f, y));
	}

	public virtual void DrawCheck(Rectangle rect) {
		float x = (rect.Width / 5.0f);
		float y = (rect.Height / 5.0f);

		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 0.0f, rect.Y + y * 3.0f, x * 2, y * 2));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 4.0f, x * 2, y * 2));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 3.0f, x * 2, y * 2));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 1.0f, x * 2, y * 2));
		renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 4.0f, rect.Y + y * 0.0f, x * 2, y * 2));
	}
	#endregion
}