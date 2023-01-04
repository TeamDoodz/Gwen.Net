using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Base for controls whose interior can be scrolled.
/// </summary>
[Xml.XmlControl]
public class ScrollControl : ContentControl {
	private readonly ScrollBar verticalScrollBar;
	private readonly ScrollBar horizontalScrollBar;
	private readonly ScrollArea scrollArea;

	private bool canScrollH;
	private bool canScrollV;
	private bool autoHideBars;

	private bool autoSizeToContent;

	public int VerticalScroll {
		get {
			return -scrollArea.VerticalScroll;
		}
		set {
			verticalScrollBar.SetScrollAmount(value / (float)(ContentSize.Height - ViewableContentSize.Height), true);
		}
	}

	public int HorizontalScroll {
		get {
			return -scrollArea.HorizontalScroll;
		}
		set {
			horizontalScrollBar.SetScrollAmount(value / (float)(ContentSize.Width - ViewableContentSize.Width), true);
		}
	}

	public override ControlBase? Content {
		get { return scrollArea.Content; }
	}

	protected ControlBase Container { get { return scrollArea; } }

	/// <summary>
	/// Indicates whether the control can be scrolled horizontally.
	/// </summary>
	[Xml.XmlProperty]
	public bool CanScrollH { get { return canScrollH; } set { EnableScroll(value, canScrollV); } }

	/// <summary>
	/// Indicates whether the control can be scrolled vertically.
	/// </summary>
	[Xml.XmlProperty]
	public bool CanScrollV { get { return canScrollV; } set { EnableScroll(canScrollH, value); } }

	/// <summary>
	/// If set, try to set the control size the same as the content size. If it doesn't fit, enable scrolling.
	/// </summary>
	[Xml.XmlProperty]
	public bool AutoSizeToContent { get { return autoSizeToContent; } set { if(value == autoSizeToContent) return; autoSizeToContent = value; Invalidate(); } }

	/// <summary>
	/// Determines whether the scroll bars should be hidden if not needed.
	/// </summary>
	[Xml.XmlProperty]
	public bool AutoHideBars { get { return autoHideBars; } set { autoHideBars = value; } }

	public Size ViewableContentSize { get { return scrollArea.ViewableContentSize; } }

	public Size ContentSize { get { return scrollArea.ContentSize; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="ScrollControl"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ScrollControl(ControlBase? parent)
		: base(parent) {
		MouseInputEnabled = true;

		canScrollV = true;
		canScrollH = true;

		autoHideBars = false;
		autoSizeToContent = false;

		verticalScrollBar = new VerticalScrollBar(this);
		verticalScrollBar.BarMoved += VBarMoved;
		verticalScrollBar.NudgeAmount = 30;

		horizontalScrollBar = new HorizontalScrollBar(this);
		horizontalScrollBar.BarMoved += HBarMoved;
		horizontalScrollBar.NudgeAmount = 30;

		scrollArea = new ScrollArea(this);
		scrollArea.SendToBack();

		innerPanel = scrollArea;

		IsVirtualControl = true;
	}

	/// <summary>
	/// Enables or disables inner scrollbars.
	/// </summary>
	/// <param name="horizontal">Determines whether the horizontal scrollbar should be enabled.</param>
	/// <param name="vertical">Determines whether the vertical scrollbar should be enabled.</param>
	public virtual void EnableScroll(bool horizontal, bool vertical) {
		canScrollV = vertical;
		canScrollH = horizontal;
		//m_VerticalScrollBar.IsHidden = !m_CanScrollV;
		//m_HorizontalScrollBar.IsHidden = !m_CanScrollH;

		scrollArea.EnableScroll(horizontal, vertical);

		Invalidate();
	}

	protected virtual void VBarMoved(ControlBase control, EventArgs args) {
		UpdateScrollArea();
	}

	protected virtual void HBarMoved(ControlBase control, EventArgs args) {
		UpdateScrollArea();
	}

	protected override Size Measure(Size availableSize) {
		Size innerSize = availableSize - Padding;

		// Check if scroll bars visible because of auto hide flag not set
		bool needScrollH = canScrollH && !autoHideBars;
		bool needScrollV = canScrollV && !autoHideBars;

		Size scrollAreaSize = innerSize;

		if(needScrollH) {
			horizontalScrollBar.DoMeasure(scrollAreaSize);
			scrollAreaSize.Height = innerSize.Height - horizontalScrollBar.MeasuredSize.Height;
		}
		if(needScrollV) {
			verticalScrollBar.DoMeasure(scrollAreaSize);
			scrollAreaSize.Width = innerSize.Width - verticalScrollBar.MeasuredSize.Width;
			// Re-measure horizontal scroll bar to take into account the width of the vertical scroll bar
			if(needScrollH) {
				horizontalScrollBar.DoMeasure(scrollAreaSize);
				scrollAreaSize.Height = innerSize.Height - horizontalScrollBar.MeasuredSize.Height;
			}
		}

		scrollArea.DoMeasure(scrollAreaSize);
		Size contentSize = scrollArea.Content?.MeasuredSize ?? Size.One;

		// If auto hide flag set and one of measure results is larger than the scroll area, we need a scroll bar and re-measure scroll area
		if((!needScrollH && contentSize.Width > scrollAreaSize.Width) || (!needScrollV && contentSize.Height > scrollAreaSize.Height)) {
			needScrollH |= canScrollH && contentSize.Width > scrollAreaSize.Width;
			needScrollV |= canScrollV && contentSize.Height > scrollAreaSize.Height;

			if(needScrollH) {
				horizontalScrollBar.DoMeasure(scrollAreaSize);
				scrollAreaSize.Height = innerSize.Height - horizontalScrollBar.MeasuredSize.Height;
			}
			if(needScrollV) {
				verticalScrollBar.DoMeasure(scrollAreaSize);
				scrollAreaSize.Width = innerSize.Width - verticalScrollBar.MeasuredSize.Width;
				// Re-measure horizontal scroll bar to take into account the width of the vertical scroll bar
				if(needScrollH) {
					horizontalScrollBar.DoMeasure(scrollAreaSize);
					scrollAreaSize.Height = innerSize.Height - horizontalScrollBar.MeasuredSize.Height;
				}
			}

			scrollArea.DoMeasure(scrollAreaSize);
			contentSize = scrollArea.Content?.MeasuredSize ?? Size.One;

			// If setting one of the scroll bars visible caused the scroll area to shrink smaller than the content, we need one more measure pass
			if((!needScrollH && contentSize.Width > scrollAreaSize.Width) || (!needScrollV && contentSize.Height > scrollAreaSize.Height)) {
				needScrollH |= canScrollH && contentSize.Width > scrollAreaSize.Width;
				needScrollV |= canScrollV && contentSize.Height > scrollAreaSize.Height;

				if(needScrollH) {
					horizontalScrollBar.DoMeasure(scrollAreaSize);
					scrollAreaSize.Height = innerSize.Height - horizontalScrollBar.MeasuredSize.Height;
				}
				if(needScrollV) {
					verticalScrollBar.DoMeasure(scrollAreaSize);
					scrollAreaSize.Width = innerSize.Width - verticalScrollBar.MeasuredSize.Width;
					// Re-measure horizontal scroll bar to take into account the width of the vertical scroll bar
					if(needScrollH) {
						horizontalScrollBar.DoMeasure(scrollAreaSize);
						scrollAreaSize.Height = innerSize.Height - horizontalScrollBar.MeasuredSize.Height;
					}
				}

				scrollArea.DoMeasure(scrollAreaSize);
			}
		}

		if(needScrollH) {
			horizontalScrollBar.IsDisabled = false;
			horizontalScrollBar.Collapse(false, false);
		} else {
			//m_HorizontalScrollBar.SetScrollAmount(0, true);
			horizontalScrollBar.IsDisabled = true;
			if(autoHideBars)
				horizontalScrollBar.Collapse(true, false);
		}
		if(needScrollV) {
			verticalScrollBar.IsDisabled = false;
			verticalScrollBar.Collapse(false, false);
		} else {
			//m_VerticalScrollBar.SetScrollAmount(0, true);
			verticalScrollBar.IsDisabled = true;
			if(autoHideBars)
				verticalScrollBar.Collapse(true, false);
		}

		if(!canScrollH || autoSizeToContent)
			availableSize.Width = Math.Min(availableSize.Width, contentSize.Width + Padding.Left + Padding.Right);
		else
			availableSize.Width = verticalScrollBar.Width;
		if(!canScrollV || autoSizeToContent)
			availableSize.Height = Math.Min(availableSize.Height, contentSize.Height + Padding.Top + Padding.Bottom);
		else
			availableSize.Height = horizontalScrollBar.Height;

		return availableSize;
	}

	protected override Size Arrange(Size finalSize) {
		int scrollAreaWidth = finalSize.Width - Padding.Left - Padding.Right;
		int scrollAreaHeight = finalSize.Height - Padding.Top - Padding.Bottom;

		if(!verticalScrollBar.IsCollapsed && !horizontalScrollBar.IsCollapsed) {
			verticalScrollBar.DoArrange(new Rectangle(finalSize.Width - Padding.Right - verticalScrollBar.MeasuredSize.Width, Padding.Top, verticalScrollBar.MeasuredSize.Width, verticalScrollBar.MeasuredSize.Height));
			scrollAreaWidth -= verticalScrollBar.MeasuredSize.Width;
			horizontalScrollBar.DoArrange(new Rectangle(Padding.Left, finalSize.Height - Padding.Bottom - horizontalScrollBar.MeasuredSize.Height, horizontalScrollBar.MeasuredSize.Width, horizontalScrollBar.MeasuredSize.Height));
			scrollAreaHeight -= horizontalScrollBar.MeasuredSize.Height;
		} else if(!verticalScrollBar.IsCollapsed) {
			verticalScrollBar.DoArrange(new Rectangle(finalSize.Width - Padding.Right - verticalScrollBar.MeasuredSize.Width, Padding.Top, verticalScrollBar.MeasuredSize.Width, verticalScrollBar.MeasuredSize.Height));
			scrollAreaWidth -= verticalScrollBar.MeasuredSize.Width;
		} else if(!horizontalScrollBar.IsCollapsed) {
			horizontalScrollBar.DoArrange(new Rectangle(Padding.Left, finalSize.Height - Padding.Bottom - horizontalScrollBar.MeasuredSize.Height, horizontalScrollBar.MeasuredSize.Width, horizontalScrollBar.MeasuredSize.Height));
			scrollAreaHeight -= horizontalScrollBar.MeasuredSize.Height;
		}

		scrollArea.DoArrange(new Rectangle(Padding.Left, Padding.Top, scrollAreaWidth, scrollAreaHeight));

		UpdateScrollBars();

		return finalSize;
	}

	/// <summary>
	/// Handler invoked on mouse wheel event.
	/// </summary>
	/// <param name="delta">Scroll delta.</param>
	/// <returns></returns>
	protected override bool OnMouseWheeled(int delta) {
		if(CanScrollV && verticalScrollBar.IsVisible) {
			if(verticalScrollBar.SetScrollAmount(verticalScrollBar.ScrollAmount - verticalScrollBar.NudgeAmount * (delta / 60.0f), true))
				return true;
		}

		if(CanScrollH && horizontalScrollBar.IsVisible) {
			if(horizontalScrollBar.SetScrollAmount(horizontalScrollBar.ScrollAmount - horizontalScrollBar.NudgeAmount * (delta / 60.0f), true))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
	}

	protected virtual void UpdateScrollBars() {
		if(scrollArea == null)
			return;

		verticalScrollBar.SetContentSize(ContentSize.Height, ViewableContentSize.Height);
		horizontalScrollBar.SetContentSize(ContentSize.Width, ViewableContentSize.Width);

		UpdateScrollArea();
	}

	protected virtual void UpdateScrollArea() {
		if(scrollArea == null)
			return;

		int newInnerPanelPosX = 0;
		int newInnerPanelPosY = 0;

		if(CanScrollV && !verticalScrollBar.IsCollapsed) {
			newInnerPanelPosY = (int)(-(ContentSize.Height - ViewableContentSize.Height) * verticalScrollBar.ScrollAmount);
		}
		if(CanScrollH && !horizontalScrollBar.IsCollapsed) {
			newInnerPanelPosX = (int)(-(ContentSize.Width - ViewableContentSize.Width) * horizontalScrollBar.ScrollAmount);
		}

		scrollArea.SetScrollPosition(newInnerPanelPosX, newInnerPanelPosY);
	}

	public virtual void ScrollToTop() {
		if(CanScrollV) {
			UpdateScrollArea();
			verticalScrollBar.ScrollToTop();
		}
	}

	public virtual void ScrollToBottom() {
		if(CanScrollV) {
			UpdateScrollArea();
			verticalScrollBar.ScrollToBottom();
		}
	}

	public virtual void ScrollToLeft() {
		if(CanScrollH) {
			UpdateScrollArea();
			verticalScrollBar.ScrollToLeft();
		}
	}

	public virtual void ScrollToRight() {
		if(CanScrollH) {
			UpdateScrollArea();
			verticalScrollBar.ScrollToRight();
		}
	}

	/// <summary>
	/// Ensure that given rectangle is visible on the scroll control.
	/// </summary>
	/// <param name="rect">Rectange to make visible.</param>
	public void EnsureVisible(Rectangle rect) {
		EnsureVisible(rect, Size.Zero);
	}

	/// <summary>
	/// Ensure that given rectangle is visible on the scroll control. If scrolling is needed, minimum scrolling is given as a parameter.
	/// </summary>
	/// <param name="rect">Rectange to make visible.</param>
	/// <param name="minChange">Minimum scrolling if scrolling needed.</param>
	public void EnsureVisible(Rectangle rect, Size minChange) {
		Rectangle bounds = new Rectangle(HorizontalScroll, VerticalScroll, ViewableContentSize);

		Point offset = Point.Zero;

		if(CanScrollH) {
			if(rect.X < bounds.X)
				offset.X = rect.X - bounds.X;
			else if(rect.Right > bounds.Right)
				offset.X = rect.Right - bounds.Right;
		}

		if(CanScrollV) {
			if(rect.Y < bounds.Y)
				offset.Y = rect.Y - bounds.Y;
			else if(rect.Bottom > bounds.Bottom)
				offset.Y = rect.Bottom - bounds.Bottom;
		}

		if(offset.X < 0)
			HorizontalScroll += Math.Min(offset.X, -minChange.Width);
		else if(offset.X > 0)
			HorizontalScroll += Math.Max(offset.X, minChange.Width);

		if(offset.Y < 0)
			VerticalScroll += Math.Min(offset.Y, -minChange.Height);
		else if(offset.Y > 0)
			VerticalScroll += Math.Max(offset.Y, minChange.Height);
	}
}