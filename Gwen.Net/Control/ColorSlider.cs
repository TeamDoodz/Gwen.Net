using System;
using Gwen.Net.Input;

namespace Gwen.Net.Control
{
	/// <summary>
	/// HSV hue selector.
	/// </summary>
	public class ColorSlider : ControlBase
	{
		private int selectedDist;
		private bool depressed;
		private Texture texture;

		/// <summary>
		/// Invoked when the selected color has been changed.
		/// </summary>
		public event GwenEventHandler<EventArgs> ColorChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorSlider"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ColorSlider(ControlBase parent)
			: base(parent)
		{
			Width = BaseUnit * 2;

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
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			//Is there any way to move this into skin? Not for now, no idea how we'll "actually" render these

			if (texture == null)
			{
				byte[] pixelData = new byte[ActualWidth * ActualHeight * 4];

				for (int y = 0; y < ActualHeight; y++)
				{
					Color c = GetColorAtHeight(y).ToColor();
					for (int x = 0; x < ActualWidth; x++)
					{
						pixelData[4 * (x + y * ActualWidth)] = c.R;
						pixelData[4 * (x + y * ActualWidth) + 1] = c.G;
						pixelData[4 * (x + y * ActualWidth) + 2] = c.B;
						pixelData[4 * (x + y * ActualWidth) + 3] = c.A;
					}
				}

				texture = new Texture(skin.Renderer);
				texture.Width = ActualWidth;
				texture.Height = ActualHeight;
				texture.LoadRaw(ActualWidth, ActualHeight, pixelData);
			}

			skin.Renderer.DrawColor = Color.White;
			skin.Renderer.DrawTexturedRect(texture, new Rectangle(5, 0, ActualWidth - 10, ActualHeight));

			int drawHeight = selectedDist - 3;

			//Draw our selectors
			skin.Renderer.DrawColor = Color.Black;
			skin.Renderer.DrawFilledRect(new Rectangle(0, drawHeight + 2, ActualWidth, 1));
			skin.Renderer.DrawFilledRect(new Rectangle(0, drawHeight, 5, 5));
			skin.Renderer.DrawFilledRect(new Rectangle(ActualWidth - 5, drawHeight, 5, 5));
			skin.Renderer.DrawColor = Color.White;
			skin.Renderer.DrawFilledRect(new Rectangle(1, drawHeight + 1, 3, 3));
			skin.Renderer.DrawFilledRect(new Rectangle(ActualWidth - 4, drawHeight + 1, 3, 3));

			base.Render(skin);
		}

		/// <summary>
		/// Handler invoked on mouse click (left) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="down">If set to <c>true</c> mouse button is down.</param>
		protected override void OnMouseClickedLeft(int x, int y, bool down)
		{
			base.OnMouseClickedLeft(x, y, down);
			depressed = down;
			if (down)
				InputHandler.MouseFocus = this;
			else
				InputHandler.MouseFocus = null;

			OnMouseMoved(x, y, 0, 0);
		}

		/// <summary>
		/// Handler invoked on mouse moved event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="dx">X change.</param>
		/// <param name="dy">Y change.</param>
		protected override void OnMouseMoved(int x, int y, int dx, int dy) {
			if (!depressed) return;

			Point cursorPos = CanvasPosToLocal(new Point(x, y));

			if (cursorPos.Y < 0) {
				cursorPos.Y = 0;
			}
			if (cursorPos.Y > ActualHeight) {
				cursorPos.Y = ActualHeight;
			}

			selectedDist = cursorPos.Y;

			ColorChanged?.Invoke(this, EventArgs.Empty);
		}

		private HSV GetColorAtHeight(int y) {
			float yPercent = y / (float)ActualHeight;
			return new HSV(yPercent * 360, 1, 1);
		}

		public void SetColor(HSV color, bool doEvents = true) {

			selectedDist = (int)(color.H / 360 * ActualHeight);

			if (doEvents && ColorChanged != null) {
				ColorChanged.Invoke(this, EventArgs.Empty);
			}
		}

		public void SetColor(Color color, bool doEvents = true) {
			SetColor(HSV.FromColor(color), doEvents);
		}

		/// <summary>
		/// Selected color.
		/// </summary>
		public HSV SelectedColor { get { return GetColorAtHeight(selectedDist); } set { SetColor(value); } }

		protected override Size Measure(Size availableSize) {
			return new Size(32, 10);
		}

		protected override Size Arrange(Size finalSize) {
			return new Size(MeasuredSize.Width, finalSize.Height);
		}

	}
}