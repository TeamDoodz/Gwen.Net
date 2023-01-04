using System;
using System.Linq;

namespace Gwen.Net.Control {
	public enum StartPosition {
		CenterParent,
		CenterCanvas,
		Manual
	}
}

namespace Gwen.Net.Control.Internal {
	public abstract class WindowBase : ResizableControl {
		private bool deleteOnClose;
		private ControlBase? realParent;
		private StartPosition startPosition = StartPosition.Manual;

		protected Dragger? dragBar;

		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs>? Closed;

		/// <summary>
		/// Is window draggable.
		/// </summary>/
		[Xml.XmlProperty]
		public bool IsDraggingEnabled { 
			get => dragBar != null && dragBar.Target != null; 
			set {
				if(dragBar == null) {
					return;
				}
				dragBar.Target = value ? this : null; 
			} 
		}

		/// <summary>
		/// Determines whether the control should be disposed on close.
		/// </summary>
		[Xml.XmlProperty]
		public bool DeleteOnClose { get { return deleteOnClose; } set { deleteOnClose = value; } }

		[Xml.XmlProperty]
		public override Padding Padding { 
			get => innerPanel?.Padding ?? Padding.Zero;
			set {
				if(innerPanel != null) {
					innerPanel.Padding = value;
				}
			} 
		}

		/// <summary>
		/// Starting position of the window.
		/// </summary>
		[Xml.XmlProperty]
		public StartPosition StartPosition { get { return startPosition; } set { startPosition = value; } }

		/// <summary>
		/// Indicates whether the control is on top of its parent's children.
		/// </summary>
		public override bool IsOnTop {
			get { return Parent?.Children.Where(x => x is WindowBase).Last() == this; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowBase"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public WindowBase(ControlBase? parent)
			: base(parent?.GetCanvas() ?? null) {
			realParent = parent;

			EnableResizing();
			BringToFront();
			IsTabable = false;
			Focus();
			MinimumSize = new Size(100, 40);
			ClampMovement = true;
			KeyboardInputEnabled = false;
			MouseInputEnabled = true;
		}

		public override void Show() {
			BringToFront();
			base.Show();
		}

		public virtual void Close() {
			IsCollapsed = true;

			if(deleteOnClose) {
				Parent?.RemoveChild(this, true);
			}

			if(Closed != null)
				Closed(this, EventArgs.Empty);
		}

		public override void Touch() {
			base.Touch();
			BringToFront();
		}

		protected virtual void OnDragged(ControlBase control, EventArgs args) {
			startPosition = StartPosition.Manual;
		}

		protected override void OnResized(ControlBase control, EventArgs args) {
			startPosition = StartPosition.Manual;

			base.OnResized(control, args);
		}

		public override bool SetBounds(int x, int y, int width, int height) {
			if(startPosition == StartPosition.CenterCanvas) {
				ControlBase canvas = GetCanvas();
				x = (canvas.ActualWidth - width) / 2;
				y = (canvas.ActualHeight - height) / 2;
			} else if(startPosition == StartPosition.CenterParent) {
				Point pt = realParent?.LocalPosToCanvas(new Point(realParent.ActualWidth / 2, realParent.ActualHeight / 2)) ?? Point.Zero;
				x = pt.X - width / 2;
				y = pt.Y - height / 2;
			}

			return base.SetBounds(x, y, width, height);
		}
	}
}