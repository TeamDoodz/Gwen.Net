using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

[Xml.XmlControl]
public class VerticalSplitter : ControlBase {
	private readonly SplitterBar hSplitter;
	private readonly ControlBase?[] sections;

	private float hVal; // 0-1
	private int barSize; // pixels
	private int zoomedSectionIndex; // 0-3

	/// <summary>
	/// Splitter position (0 - 1)
	/// </summary>
	[Xml.XmlProperty]
	public float Value { get { return hVal; } set { SetHValue(value); } }

	/// <summary>
	/// Indicates whether any of the panels is zoomed.
	/// </summary>
	public bool IsZoomed { get { return zoomedSectionIndex != -1; } }

	/// <summary>
	/// Gets or sets a value indicating whether splitters should be visible.
	/// </summary>
	[Xml.XmlProperty]
	public bool SplittersVisible {
		get { return hSplitter.ShouldDrawBackground; }
		set { hSplitter.ShouldDrawBackground = value; }
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
	public VerticalSplitter(ControlBase? parent)
		: base(parent) {
		sections = new ControlBase[2];

		hSplitter = new SplitterBar(this);
		hSplitter.Dragged += OnHorizontalMoved;
		hSplitter.Cursor = Cursor.SizeWE;

		hVal = 0.5f;

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
		hVal = 0.5f;
		Invalidate();
	}

	public void SetHValue(float value) {
		if(value <= 1f || value >= 0)
			hVal = value;

		Invalidate();
	}

	protected void OnHorizontalMoved(ControlBase control, EventArgs args) {
		hVal = CalculateValueHorizontal();
		Invalidate();
	}

	private float CalculateValueHorizontal() {
		return hSplitter.ActualLeft / (float)(ActualWidth - hSplitter.ActualWidth);
	}

	protected override Size Measure(Size availableSize) {
		Size size = Size.Zero;

		hSplitter.DoMeasure(new Size(barSize, availableSize.Height));
		size.Width += hSplitter.Width;

		int h = (int)((availableSize.Width - barSize) * hVal);

		if(zoomedSectionIndex == -1) {
			if(sections[0] is ControlBase section0) {
				section0.DoMeasure(new Size(h, availableSize.Height));
				size.Width += section0.MeasuredSize.Width;
				size.Height = Math.Max(size.Height, section0.MeasuredSize.Height);
			}

			if(sections[1] is ControlBase section1) {
				section1.DoMeasure(new Size(availableSize.Width - barSize - h, availableSize.Height));
				size.Width += section1.MeasuredSize.Width;
				size.Height = Math.Max(size.Height, section1.MeasuredSize.Height);
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
		int h = (int)((finalSize.Width - barSize) * hVal);

		hSplitter.DoArrange(new Rectangle(h, 0, hSplitter.MeasuredSize.Width, finalSize.Height));

		if(zoomedSectionIndex == -1) {
			sections[0]?.DoArrange(new Rectangle(0, 0, h, finalSize.Height));

			sections[1]?.DoArrange(new Rectangle(h + barSize, 0, finalSize.Width - barSize - h, finalSize.Height));
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
	/// <param name="sectionIndex">Panel index (0-3).</param>
	public void Zoom(int sectionIndex) {
		UnZoom();

		if(sections[sectionIndex] != null) {
			for(int i = 0; i < 2; i++) {
				if(i != sectionIndex && sections[i] is ControlBase section)
					section.IsHidden = true;
			}
			zoomedSectionIndex = sectionIndex;

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
			if(sections[i] is ControlBase section)
				section.IsHidden = false;
		}

		Invalidate();
		OnZoomChanged();
	}
}