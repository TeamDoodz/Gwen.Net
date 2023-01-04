using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

[Xml.XmlControl]
public class HorizontalSplitter : ControlBase {
	private readonly SplitterBar vSplitter;
	private readonly ControlBase?[] sections;

	private float vVal; // 0-1
	private int barSize; // pixels
	private int zoomedSectionIndex; // 0-1

	/// <summary>
	/// Splitter position (0 - 1)
	/// </summary>
	[Xml.XmlProperty]
	public float Value { get { return vVal; } set { SetVValue(value); } }

	/// <summary>
	/// Indicates whether any of the panels is zoomed.
	/// </summary>
	public bool IsZoomed { get { return zoomedSectionIndex != -1; } }

	/// <summary>
	/// Gets or sets a value indicating whether splitters should be visible.
	/// </summary>
	[Xml.XmlProperty]
	public bool SplittersVisible {
		get { return vSplitter.ShouldDrawBackground; }
		set { vSplitter.ShouldDrawBackground = value; }
	}

	/// <summary>
	/// Gets or sets the size of the splitter.
	/// </summary>
	[Xml.XmlProperty]
	public int SplitterSize { get { return barSize; } set { barSize = value; } }

	/// <summary>
	/// Invoked when one of the panels has been zoomed (maximized).
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? PanelZoomed;

	/// <summary>
	/// Invoked when one of the panels has been unzoomed (restored).
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? PanelUnZoomed;

	/// <summary>
	/// Invoked when the zoomed panel has been changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? ZoomChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="CrossSplitter"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public HorizontalSplitter(ControlBase? parent)
		: base(parent) {
		sections = new ControlBase[2];

		vSplitter = new SplitterBar(this);
		vSplitter.Dragged += OnVerticalMoved;
		vSplitter.Cursor = Cursor.SizeNS;

		vVal = 0.5f;

		SetPanel(0, null);
		SetPanel(1, null);

		SplitterSize = 5;
		SplittersVisible = false;

		zoomedSectionIndex = -1;
	}

	/// <summary>
	/// Centers the panels so that they take even amount of space.
	/// </summary>
	public void CenterPanels() {
		vVal = 0.5f;
		Invalidate();
	}

	public void SetVValue(float value) {
		if(value <= 1f || value >= 0)
			vVal = value;

		Invalidate();
	}

	protected void OnVerticalMoved(ControlBase control, EventArgs args) {
		vVal = CalculateValueVertical();
		Invalidate();
	}

	private float CalculateValueVertical() {
		return vSplitter.ActualTop / (float)(ActualHeight - vSplitter.ActualHeight);
	}

	protected override Size Measure(Size availableSize) {
		Size size = Size.Zero;

		vSplitter.DoMeasure(new Size(availableSize.Width, barSize));
		size.Height += vSplitter.Height;

		int v = (int)((availableSize.Height - barSize) * vVal);

		if(zoomedSectionIndex == -1) {
			if(sections[0] is ControlBase section0) {
				section0.DoMeasure(new Size(availableSize.Width, v));
				size.Height += section0.MeasuredSize.Height;
				size.Width = Math.Max(size.Width, section0.MeasuredSize.Width);
			}
			if(sections[0] is ControlBase section1) {
				section1.DoMeasure(new Size(availableSize.Width, availableSize.Height - barSize - v));
				size.Height += section1.MeasuredSize.Height;
				size.Width = Math.Max(size.Width, section1.MeasuredSize.Width);
			}
		} else {
			if(sections[zoomedSectionIndex] is ControlBase zoomedSection) {
				zoomedSection.DoMeasure(availableSize);
				size = zoomedSection.MeasuredSize;
			}
		}

		return size;
	}

	protected override Size Arrange(Size finalSize) {
		int v = (int)((finalSize.Height - barSize) * vVal);

		vSplitter.DoArrange(new Rectangle(0, v, vSplitter.MeasuredSize.Width, vSplitter.MeasuredSize.Height));

		if(zoomedSectionIndex == -1) {
			sections[0]?.DoArrange(new Rectangle(0, 0, finalSize.Width, v));

			sections[1]?.DoArrange(new Rectangle(0, v + barSize, finalSize.Width, finalSize.Height - barSize - v));
		} else {
			sections[zoomedSectionIndex]?.DoArrange(new Rectangle(0, 0, finalSize.Width, finalSize.Height));
		}

		return finalSize;
	}

	/// <summary>
	/// Assigns a control to the specific inner section.
	/// </summary>
	/// <param name="index">Section index (0-3).</param>
	/// <param name="panel">Control to assign.</param>
	public void SetPanel(int index, ControlBase? panel) {
		sections[index] = panel;

		if(panel != null) {
			panel.Parent = this;
		}

		Invalidate();
	}

	/// <summary>
	/// Gets the specific inner section.
	/// </summary>
	/// <param name="index">Section index (0-3).</param>
	/// <returns>Specified section.</returns>
	public ControlBase? GetPanel(int index) {
		return sections[index];
	}

	protected override void OnChildAdded(ControlBase child) {
		if(!(child is SplitterBar)) {
			if(sections[0] == null)
				SetPanel(0, child);
			else if(sections[1] == null)
				SetPanel(1, child);
			else
				throw new Exception("Too many panels added.");
		}

		base.OnChildAdded(child);
	}

	/// <summary>
	/// Internal handler for the zoom changed event.
	/// </summary>
	protected void OnZoomChanged() {
		if(ZoomChanged != null)
			ZoomChanged.Invoke(this, EventArgs.Empty);

		if(zoomedSectionIndex == -1) {
			if(PanelUnZoomed != null)
				PanelUnZoomed.Invoke(this, EventArgs.Empty);
		} else {
			if(PanelZoomed != null)
				PanelZoomed.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>
	/// Maximizes the specified panel so it fills the entire control.
	/// </summary>
	/// <param name="section">Panel index (0-3).</param>
	public void Zoom(int section) {
		UnZoom();

		if(sections[section] != null) {
			for(int i = 0; i < 2; i++) {
				if(i != section && sections[i] is ControlBase sectioni)
					sectioni.IsHidden = true;
			}
			zoomedSectionIndex = section;

			Invalidate();
		}
		OnZoomChanged();
	}

	/// <summary>
	/// Restores the control so all panels are visible.
	/// </summary>
	public void UnZoom() {
		zoomedSectionIndex = -1;

		for(int i = 0; i < 2; i++) {
			if(sections[i] is ControlBase sectioni)
				sectioni.IsHidden = false;
		}

		Invalidate();
		OnZoomChanged();
	}
}