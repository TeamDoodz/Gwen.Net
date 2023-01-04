using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Properties table.
/// </summary>
public class Properties : ContentControl {
	private readonly SplitterBar splitterBar;
	private int labelWidth;

	internal const int DEFAULT_LABEL_WIDTH = 80;

	/// <summary>
	/// Width of the first column (property names).
	/// </summary>
	internal int LabelWidth { get { return labelWidth; } set { if(value == labelWidth) return; labelWidth = value; Invalidate(); } }

	/// <summary>
	/// Invoked when a property value has been changed.
	/// </summary>
	public event GwenEventHandler<EventArgs>? ValueChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="Properties"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public Properties(ControlBase? parent)
		: base(parent) {
		splitterBar = new SplitterBar(this);
		splitterBar.Width = 3;
		splitterBar.Cursor = Cursor.SizeWE;
		splitterBar.Dragged += OnSplitterMoved;
		splitterBar.ShouldDrawBackground = false;

		labelWidth = DEFAULT_LABEL_WIDTH;

		innerPanel = new Layout.VerticalLayout(this);
	}

	protected override Size Measure(Size availableSize) {
		availableSize -= Padding;

		Size size = innerPanel?.DoMeasure(availableSize) ?? Size.One;

		splitterBar.DoMeasure(new Size(availableSize.Width, size.Height));

		return size + Padding;
	}

	protected override Size Arrange(Size finalSize) {
		finalSize -= Padding;

		innerPanel?.DoArrange(Padding.Left, Padding.Top, finalSize.Width, finalSize.Height);

		splitterBar.DoArrange(Padding.Left + labelWidth - 2, Padding.Top, splitterBar.MeasuredSize.Width, innerPanel?.MeasuredSize.Height ?? 1);

		return new Size(finalSize.Width, innerPanel?.MeasuredSize.Height ?? 1) + Padding;
	}

	/// <summary>
	/// Handles the splitter moved event.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnSplitterMoved(ControlBase control, EventArgs args) {
		LabelWidth = splitterBar.ActualLeft - Padding.Left;

		if(Parent is PropertyTreeNode node) {
			node.PropertyTree.LabelWidth = LabelWidth;
		}
	}

	/// <summary>
	/// Adds a new text property row.
	/// </summary>
	/// <param name="label">Property name.</param>
	/// <param name="value">Initial value.</param>
	/// <returns>Newly created row.</returns>
	public PropertyRow Add(string label, string value = "") {
		return Add(label, new Property.Text(this), value);
	}

	/// <summary>
	/// Adds a new property row.
	/// </summary>
	/// <param name="label">Property name.</param>
	/// <param name="prop">Property control.</param>
	/// <param name="value">Initial value.</param>
	/// <returns>Newly created row.</returns>
	public PropertyRow Add(string label, Property.PropertyBase prop, string value = "") {
		PropertyRow row = new PropertyRow(this, prop);
		row.Label = label;
		row.ValueChanged += OnRowValueChanged;

		prop.SetValue(value, true);

		splitterBar.BringToFront();
		return row;
	}

	private void OnRowValueChanged(ControlBase control, EventArgs args) {
		if(ValueChanged != null)
			ValueChanged.Invoke(control, EventArgs.Empty);
	}

	/// <summary>
	/// Deletes all rows.
	/// </summary>
	public void DeleteAll() {
		innerPanel?.DeleteAllChildren();
	}
}