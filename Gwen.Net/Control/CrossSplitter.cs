using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;
/// <summary>
/// Splitter control.
/// </summary>
[Xml.XmlControl]
public class CrossSplitter : ControlBase {
	private readonly SplitterBar vSplitter;
	private readonly SplitterBar hSplitter;
	private readonly SplitterBar cSplitter;

	private readonly ControlBase?[] sections;

	private float hVal; // 0-1
	private float vVal; // 0-1
	private int barSize; // pixels

	private int zoomedSectionIndex; // 0-3

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
	public CrossSplitter(ControlBase? parent)
		: base(parent) {
		sections = new ControlBase[4];

		vSplitter = new SplitterBar(this);
		vSplitter.Dragged += OnVerticalMoved;
		vSplitter.Cursor = Cursor.SizeNS;

		hSplitter = new SplitterBar(this);
		hSplitter.Dragged += OnHorizontalMoved;
		hSplitter.Cursor = Cursor.SizeWE;

		cSplitter = new SplitterBar(this);
		cSplitter.Dragged += OnCenterMoved;
		cSplitter.Cursor = Cursor.SizeAll;

		hVal = 0.5f;
		vVal = 0.5f;

		SetPanel(0, null);
		SetPanel(1, null);
		SetPanel(2, null);
		SetPanel(3, null);

		SplitterSize = 5;
		SplittersVisible = false;

		zoomedSectionIndex = -1;
	}

	/// <summary>
	/// Centers the panels so that they take even amount of space.
	/// </summary>
	public void CenterPanels() {
		hVal = 0.5f;
		vVal = 0.5f;
		Invalidate();
	}

	/// <summary>
	/// Indicates whether any of the panels is zoomed.
	/// </summary>
	public bool IsZoomed { get { return zoomedSectionIndex != -1; } }

	/// <summary>
	/// Gets or sets a value indicating whether splitters should be visible.
	/// </summary>
	[Xml.XmlProperty]
	public bool SplittersVisible {
		get { return cSplitter.ShouldDrawBackground; }
		set {
			cSplitter.ShouldDrawBackground = value;
			vSplitter.ShouldDrawBackground = value;
			hSplitter.ShouldDrawBackground = value;
		}
	}

	/// <summary>
	/// Gets or sets the size of the splitter.
	/// </summary>
	[Xml.XmlProperty]
	public int SplitterSize { get { return barSize; } set { barSize = value; } }

	protected void OnCenterMoved(ControlBase control, EventArgs args) {
		CalculateValueCenter();
		Invalidate();
	}

	protected void OnVerticalMoved(ControlBase control, EventArgs args) {
		vVal = CalculateValueVertical();
		Invalidate();
	}

	protected void OnHorizontalMoved(ControlBase control, EventArgs args) {
		hVal = CalculateValueHorizontal();
		Invalidate();
	}

	private void CalculateValueCenter() {
		hVal = cSplitter.ActualLeft / (float)(ActualWidth - cSplitter.ActualWidth);
		vVal = cSplitter.ActualTop / (float)(ActualHeight - cSplitter.ActualHeight);
	}

	private float CalculateValueVertical() {
		return vSplitter.ActualTop / (float)(ActualHeight - vSplitter.ActualHeight);
	}

	private float CalculateValueHorizontal() {
		return hSplitter.ActualLeft / (float)(ActualWidth - hSplitter.ActualWidth);
	}

	protected override Size Measure(Size availableSize) {
		Size size = Size.Zero;

		vSplitter.DoMeasure(new Size(availableSize.Width, barSize));
		hSplitter.DoMeasure(new Size(barSize, availableSize.Height));
		cSplitter.DoMeasure(new Size(barSize, barSize));
		size = new Size(hSplitter.Width, vSplitter.Height);

		int h = (int)((availableSize.Width - barSize) * hVal);
		int v = (int)((availableSize.Height - barSize) * vVal);

		if(zoomedSectionIndex == -1) {
			if(sections[0] is ControlBase section0) {
				section0.DoMeasure(new Size(h, v));
				size += section0.MeasuredSize;
			}
			if(sections[1] is ControlBase section1) {
				section1.DoMeasure(new Size(availableSize.Width - barSize - h, v));
				size += section1.MeasuredSize;
			}
			if(sections[2] is ControlBase section2) {
				section2.DoMeasure(new Size(h, availableSize.Height - barSize - v));
				size += section2.MeasuredSize;
			}
			if(sections[3] is ControlBase section3) {
				section3.DoMeasure(new Size(availableSize.Width - barSize - h, availableSize.Height - barSize - v));
				size += section3.MeasuredSize;
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
		int v = (int)((finalSize.Height - barSize) * vVal);

		vSplitter.DoArrange(new Rectangle(0, v, vSplitter.MeasuredSize.Width, vSplitter.MeasuredSize.Height));
		hSplitter.DoArrange(new Rectangle(h, 0, hSplitter.MeasuredSize.Width, hSplitter.MeasuredSize.Height));
		cSplitter.DoArrange(new Rectangle(h, v, cSplitter.MeasuredSize.Width, cSplitter.MeasuredSize.Height));

		if(zoomedSectionIndex == -1) {
			sections[0]?.DoArrange(new Rectangle(0, 0, h, v));

			sections[1]?.DoArrange(new Rectangle(h + barSize, 0, finalSize.Width - barSize - h, v));

			sections[2]?.DoArrange(new Rectangle(0, v + barSize, h, finalSize.Height - barSize - v));

			sections[3]?.DoArrange(new Rectangle(h + barSize, v + barSize, finalSize.Width - barSize - h, finalSize.Height - barSize - v));
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
			else if(sections[2] == null)
				SetPanel(2, child);
			else if(sections[3] == null)
				SetPanel(3, child);
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
			for(int i = 0; i < 4; i++) {
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

		for(int i = 0; i < 4; i++) {
			if(sections[i] is ControlBase sectioni)
				sectioni.IsHidden = false;
		}

		Invalidate();
		OnZoomChanged();
	}

	protected override void Render(Skin.SkinBase skin) {
		skin.DrawBorder(this, BorderType.TreeControl);
	}
}