﻿using System;
using Gwen.Net.Input;

namespace Gwen.Net.Control.Internal;
public abstract class ButtonBase : ControlBase {
	private bool depressed;
	private bool toggle;
	private bool toggleStatus;

	/// <summary>
	/// Invoked when the button is pressed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Pressed;

	/// <summary>
	/// Invoked when the button is released.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Released;

	/// <summary>
	/// Invoked when the button's toggle state has changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Toggled;

	/// <summary>
	/// Invoked when the button's toggle state has changed to On.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? ToggledOn;

	/// <summary>
	/// Invoked when the button's toggle state has changed to Off.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? ToggledOff;

	/// <summary>
	/// Indicates whether the button is depressed.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsDepressed {
		get { return depressed; }
		set {
			if(depressed == value)
				return;
			depressed = value;
			Redraw();
		}
	}

	/// <summary>
	/// Indicates whether the button is toggleable.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsToggle { get { return toggle; } set { toggle = value; } }

	/// <summary>
	/// Determines the button's toggle state.
	/// </summary>
	[Xml.XmlProperty]
	public bool ToggleState {
		get { return toggleStatus; }
		set {
			if(!toggle) return;
			if(toggleStatus == value) return;

			toggleStatus = value;

			if(Toggled != null)
				Toggled.Invoke(this, EventArgs.Empty);

			if(toggleStatus) {
				if(ToggledOn != null)
					ToggledOn.Invoke(this, EventArgs.Empty);
			} else {
				if(ToggledOff != null)
					ToggledOff.Invoke(this, EventArgs.Empty);
			}

			Redraw();
		}
	}

	public ButtonBase(ControlBase? parent)
		: base(parent) {
		MouseInputEnabled = true;
	}

	/// <summary>
	/// Toggles the button.
	/// </summary>
	public virtual void Toggle() {
		ToggleState = !ToggleState;
	}

	/// <summary>
	/// "Clicks" the button.
	/// </summary>
	public virtual void Press(ControlBase? control = null) {
		OnClicked(0, 0);
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {
		//base.OnMouseClickedLeft(x, y, down);
		if(down) {
			IsDepressed = true;
			InputHandler.MouseFocus = this;
			if(Pressed != null)
				Pressed.Invoke(this, EventArgs.Empty);
		} else {
			if(IsHovered && depressed) {
				OnClicked(x, y);
			}

			IsDepressed = false;
			InputHandler.MouseFocus = null;
			if(Released != null)
				Released.Invoke(this, EventArgs.Empty);
		}

		Redraw();
	}

	/// <summary>
	/// Internal OnPressed implementation.
	/// </summary>
	protected virtual void OnClicked(int x, int y) {
		if(IsToggle) {
			Toggle();
		}

		base.OnMouseClickedLeft(x, y, true);
	}

	/// <summary>
	/// Default accelerator handler.
	/// </summary>
	protected override void OnAccelerator() {
		OnClicked(0, 0);
	}

	/// <summary>
	/// Handler invoked on mouse double click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	protected override void OnMouseDoubleClickedLeft(int x, int y) {
		base.OnMouseDoubleClickedLeft(x, y);
		OnMouseClickedLeft(x, y, true);
	}

	protected override Size Measure(Size availableSize) {
		return Size.Zero;
	}

	protected override Size Arrange(Size finalSize) {
		return finalSize;
	}
}