﻿using System;

namespace Gwen.Net.Control.Internal;

/// <summary>
/// Base class for scrollbars.
/// </summary>
public class ScrollBar : ControlBase {
	protected readonly ScrollBarButton[] scrollButton;
	protected readonly ScrollBarBar bar;

	protected bool depressed;
	protected float scrollAmount;
	protected float contentSize;
	protected float viewableContentSize;
	protected float nudgeAmount;

	/// <summary>
	/// Invoked when the bar is moved.
	/// </summary>
	public event GwenEventHandler<EventArgs>? BarMoved;

	/// <summary>
	/// Bar size (in pixels).
	/// </summary>
	public virtual int BarSize { get; set; }

	/// <summary>
	/// Bar position (in pixels).
	/// </summary>
	public virtual int BarPos { get { return 0; } }

	/// <summary>
	/// Button size (in pixels).
	/// </summary>
	public virtual int ButtonSize { get { return 0; } }

	public virtual float NudgeAmount { get { return nudgeAmount / contentSize; } set { nudgeAmount = value; } }
	public float ScrollAmount { get { return scrollAmount; } }
	public float ContentSize { get { return contentSize; } }
	public float ViewableContentSize { get { return viewableContentSize; } }

	/// <summary>
	/// Indicates whether the bar is horizontal.
	/// </summary>
	public virtual bool IsHorizontal { get { return false; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="ScrollBar"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	protected ScrollBar(ControlBase? parent) : base(parent) {
		scrollButton = new ScrollBarButton[2];
		scrollButton[0] = new ScrollBarButton(this);
		scrollButton[1] = new ScrollBarButton(this);

		bar = new ScrollBarBar(this);

		depressed = false;

		scrollAmount = 0;
		contentSize = 0;
		viewableContentSize = 0;

		NudgeAmount = 20;
	}

	/// <summary>
	/// Sets the scroll amount (0-1).
	/// </summary>
	/// <param name="value">Scroll amount.</param>
	/// <param name="forceUpdate">Determines whether the control should be updated.</param>
	/// <returns>True if control state changed.</returns>
	public virtual bool SetScrollAmount(float value, bool forceUpdate = false) {
		if(scrollAmount == value && !forceUpdate)
			return false;
		scrollAmount = value;
		OnBarMoved(this, EventArgs.Empty);
		return true;
	}

	public void SetContentSize(float contentSize, float viewableContentSize) {
		this.contentSize = contentSize;
		this.viewableContentSize = viewableContentSize;

		UpdateBarSize();
	}

	protected virtual void UpdateBarSize() {

	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {

	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawScrollBar(this, IsHorizontal, depressed);
	}

	/// <summary>
	/// Handler for the BarMoved event.
	/// </summary>
	/// <param name="control">The control.</param>
	protected virtual void OnBarMoved(ControlBase control, EventArgs args) {
		if(BarMoved != null)
			BarMoved.Invoke(this, EventArgs.Empty);
	}

	protected virtual float CalculateScrolledAmount() {
		return 0;
	}

	protected virtual int CalculateBarSize() {
		return 0;
	}

	public virtual void ScrollToLeft() { }
	public virtual void ScrollToRight() { }
	public virtual void ScrollToTop() { }
	public virtual void ScrollToBottom() { }
}