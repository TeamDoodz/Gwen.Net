using System;
using Gwen.Net.Input;

namespace Gwen.Net.Control;

/// <summary>
/// Linear-interpolated HSV color box.
/// </summary>
public class ColorLerpBox : ControlBase {
	private Point cursorPos;
	private bool depressed;
	private float hue;
	private Texture? texture;

	/// <summary>
	/// Invoked when the selected color has been changed.
	/// </summary>
	public event GwenEventHandler<EventArgs>? ColorChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorLerpBox"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ColorLerpBox(ControlBase? parent) : base(parent) {
		SetColor(new Color(255, 255, 128, 0));
		MouseInputEnabled = true;
		depressed = false;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public override void Dispose() {
		texture?.Dispose();
		base.Dispose();
	}

	/// <summary>
	/// The color that is selected.
	/// </summary>
	public HSV SelectedColor => GetColorAt(cursorPos.X, cursorPos.Y);

	/// <summary>
	/// Sets the selected color.
	/// </summary>
	/// <param name="value">Value to set.</param>
	/// <param name="onlyHue">Determines whether to only set H value (not SV).</param>
	/// <param name="doEvents">Call event callbacks after calling this method.</param>
	public void SetColor(HSV value, bool onlyHue = true, bool doEvents = true) {
		hue = value.H;

		if(!onlyHue) {
			cursorPos.X = (int)(value.S * ActualWidth);
			cursorPos.Y = (int)((1 - value.V) * ActualHeight);
		}
		InvalidateTexture();

		if(doEvents && ColorChanged != null) {
			ColorChanged.Invoke(this, EventArgs.Empty);
		}
	}

	public void SetColor(Color value, bool onlyHue = true, bool doEvents = true) {
		SetColor(HSV.FromColor(value), onlyHue, doEvents);
	}

	/// <summary>
	/// Handler invoked on mouse moved event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="dx">X change.</param>
	/// <param name="dy">Y change.</param>
	protected override void OnMouseMoved(int x, int y, int dx, int dy) {
		if(!depressed) {
			return;
		}
		cursorPos = CanvasPosToLocal(new Point(x, y));

		cursorPos.X = Util.Clamp(cursorPos.X, 0, ActualWidth);
		cursorPos.Y = Util.Clamp(cursorPos.X, 0, ActualHeight);

		ColorChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {
		base.OnMouseClickedLeft(x, y, down);
		depressed = down;
		if(down) {
			InputHandler.MouseFocus = this;
		} else {
			InputHandler.MouseFocus = null;
		}

		OnMouseMoved(x, y, 0, 0);
	}

	/// <summary>
	/// Gets the color from specified coordinates.
	/// </summary>
	/// <param name="x">X</param>
	/// <param name="y">Y</param>
	/// <returns>Color value.</returns>
	private HSV GetColorAt(int x, int y) {
		float xPercent = (x / (float)ActualWidth);
		float yPercent = 1 - (y / (float)ActualHeight);

		return new HSV(hue, xPercent, yPercent);
	}

	/// <summary>
	/// Invalidates the control.
	/// </summary>
	private void InvalidateTexture() {
		if(texture != null) {
			texture.Dispose();
			texture = null;
		}
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		if(texture == null) {
			byte[] pixelData = new byte[ActualWidth * ActualHeight * 4];

			for(int x = 0; x < ActualWidth; x++) {
				for(int y = 0; y < ActualHeight; y++) {
					Color c = GetColorAt(x, y).ToColor();
					pixelData[4 * (x + y * ActualWidth)] = c.R;
					pixelData[4 * (x + y * ActualWidth) + 1] = c.G;
					pixelData[4 * (x + y * ActualWidth) + 2] = c.B;
					pixelData[4 * (x + y * ActualWidth) + 3] = c.A;
				}
			}

			texture = new(skin.Renderer) {
				Width = ActualWidth,
				Height = ActualHeight
			};
			texture.LoadRaw(ActualWidth, ActualHeight, pixelData);
		}

		skin.Renderer.DrawColor = Color.White;
		skin.Renderer.DrawTexturedRect(texture, RenderBounds);

		skin.Renderer.DrawColor = Color.Black;
		skin.Renderer.DrawLinedRect(RenderBounds);

		Color selected = SelectedColor.ToColor();
		if((selected.R + selected.G + selected.B) / 3 < 170) {
			skin.Renderer.DrawColor = Color.White;
		} else {
			skin.Renderer.DrawColor = Color.Black;
		}

		Rectangle testRect = new(cursorPos.X - 3, cursorPos.Y - 3, 6, 6);

		skin.Renderer.DrawShavedCornerRect(testRect);
	}

	protected override void OnBoundsChanged(Rectangle oldBounds) {
		if(texture != null) {
			texture.Dispose();
			texture = null;
		}

		base.OnBoundsChanged(oldBounds);
	}

	protected override Size Measure(Size availableSize) {
		cursorPos = new Point(0, 0);

		return new Size(128, 128);
	}

	protected override Size Arrange(Size finalSize) {
		return finalSize;
	}
}