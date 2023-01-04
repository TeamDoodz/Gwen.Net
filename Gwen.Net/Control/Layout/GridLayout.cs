using System;
using System.Collections.Generic;

namespace Gwen.Net.Control.Layout;
/// <summary>
/// GridLayout column widths or row heights.
/// </summary>
/// <remarks>
/// Cell size can be one of
/// a) Single.NaN: Auto sized. Size is the smallest size the control can be drawn.
/// b) 0.0 - 1.0: Remaining space filled proportionally.
/// c) More than 1.0: Absolute cell size.
/// </remarks>
public class GridCellSizes : List<float> {
	public GridCellSizes(IEnumerable<float> sizes)
		: base(sizes) {
	}

	public GridCellSizes(int count)
		: base(count) {
	}

	public GridCellSizes(params float[] sizes)
		: base(sizes) {
	}
}

/// <summary>
/// Arrange child controls into columns and rows by adding them in column and row order.
/// Add every column of the first row, then every column of the second row etc.
/// </summary>
[Xml.XmlControl]
public class GridLayout : ControlBase {
	private int columnCount;

	private float[] requestedColumnWidths = Array.Empty<float>();
	private float[] requestedRowHeights = Array.Empty<float>();

	private Size totalFixedSize;
	private Size totalAutoFixedSize;

	private int[] columnWidths = Array.Empty<int>();
	private int[] rowHeights = Array.Empty<int>();

	public const float AutoSize = float.NaN;
	public const float Fill = 1.0f;

	/// <summary>
	/// Number of columns. This can be used when all cells are auto size.
	/// </summary>
	[Xml.XmlProperty]
	public int ColumnCount { get { return columnCount; } set { columnCount = value; Invalidate(); } }

	/// <summary>
	/// Column widths. <see cref="GridCellSizes"/>
	/// </summary>
	[Xml.XmlProperty]
	public GridCellSizes ColumnWidths { set { SetColumnWidths(value.ToArray()); } }

	/// <summary>
	/// Row heights. <see cref="GridCellSizes"/>
	/// </summary>
	[Xml.XmlProperty]
	public GridCellSizes RowHeights { set { SetRowHeights(value.ToArray()); } }

	/// <summary>
	/// Initializes a new instance of the <see cref="GridLayout"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public GridLayout(ControlBase? parent)
		: base(parent) {
		columnCount = 1;
	}

	/// <summary>
	/// Set column widths. <see cref="GridCellSizes"/>
	/// </summary>
	/// <param name="widths">Array of widths.</param>
	public void SetColumnWidths(params float[] widths) {
		totalFixedSize.Width = 0;
		float relTotalWidth = 0.0f;
		foreach(float w in widths) {
			if(w >= 0.0f && w <= 1.0f)
				relTotalWidth += w;
			else if(w > 1.0f)
				totalFixedSize.Width += (int)w;
		}

		if(relTotalWidth > 1.0f)
			throw new ArgumentException("Relative widths exceed total value of 1.0 (100%).");

		requestedColumnWidths = widths;
		columnCount = widths.Length;
		Invalidate();
	}

	/// <summary>
	/// Set row heights. <see cref="GridCellSizes"/>
	/// </summary>
	/// <param name="heights">Array of heights.</param>
	public void SetRowHeights(params float[] heights) {
		totalFixedSize.Height = 0;
		float relTotalHeight = 0.0f;
		foreach(float h in heights) {
			if(h >= 0.0f && h <= 1.0f)
				relTotalHeight += h;
			else if(h > 1.0f)
				totalFixedSize.Height += (int)h;
		}

		if(relTotalHeight > 1.0f)
			throw new ArgumentException("Relative heights exceed total value of 1.0 (100%).");

		requestedRowHeights = heights;
		Invalidate();
	}

	protected override Size Measure(Size availableSize) {
		availableSize -= Padding;

		if(columnWidths == null || columnWidths.Length != columnCount)
			columnWidths = new int[columnCount];

		int rowCount = (this.Children.Count + columnCount - 1) / columnCount;
		if(rowHeights == null || rowHeights.Length != rowCount)
			rowHeights = new int[rowCount];

		int columnIndex;
		for(columnIndex = 0; columnIndex < columnCount; columnIndex++) {
			columnWidths[columnIndex] = 0;
		}

		int rowIndex;
		for(rowIndex = 0; rowIndex < rowCount; rowIndex++) {
			rowHeights[rowIndex] = 0;
		}

		Size cellAvailableSize = availableSize;
		columnIndex = 0;
		rowIndex = 0;
		foreach(ControlBase child in this.Children) {
			Size size;
			if(child.IsCollapsed) {
				size = Size.Zero;
			} else {
				size = cellAvailableSize;
				if(requestedColumnWidths.Length != 0) {
					float w = requestedColumnWidths[columnIndex];
					if(w >= 0.0f && w <= 1.0f)
						size.Width = (int)(w * (availableSize.Width - totalFixedSize.Width));
					else if(w > 1.0f)
						size.Width = (int)w;
				}
				if(requestedRowHeights.Length != 0) {
					float h = requestedRowHeights[rowIndex];
					if(h >= 0.0f && h <= 1.0f)
						size.Height = (int)(h * (availableSize.Height - totalFixedSize.Height));
					else if(h > 1.0f)
						size.Height = (int)h;
				}

				size = child.DoMeasure(size);
			}

			if(columnWidths[columnIndex] < size.Width)
				columnWidths[columnIndex] = size.Width;

			if(rowHeights[rowIndex] < size.Height)
				rowHeights[rowIndex] = size.Height;

			cellAvailableSize.Width -= columnWidths[columnIndex];

			columnIndex++;
			if(columnIndex == columnCount) {
				cellAvailableSize.Width = availableSize.Width;
				cellAvailableSize.Height -= rowHeights[rowIndex];
				columnIndex = 0;
				rowIndex++;
			}
		}

		totalAutoFixedSize = Size.Zero;

		int width = 0;
		for(columnIndex = 0; columnIndex < columnCount; columnIndex++) {
			if(requestedColumnWidths.Length != 0) {
				float w = requestedColumnWidths[columnIndex];
				if(w > 1.0f) {
					if(columnWidths[columnIndex] < w)
						columnWidths[columnIndex] = (int)w;

					totalAutoFixedSize.Width += columnWidths[columnIndex];
				} else if(Single.IsNaN(w)) {
					totalAutoFixedSize.Width += columnWidths[columnIndex];
				}
			} else {
				totalAutoFixedSize.Width += columnWidths[columnIndex];
			}

			width += columnWidths[columnIndex];
		}

		int height = 0;
		for(rowIndex = 0; rowIndex < rowCount; rowIndex++) {
			if(requestedRowHeights.Length != 0) {
				float h = requestedRowHeights[rowIndex];
				if(h > 1.0f) {
					if(rowHeights[rowIndex] < h)
						rowHeights[rowIndex] = (int)h;

					totalAutoFixedSize.Height += rowHeights[rowIndex];
				} else if(Single.IsNaN(h)) {
					totalAutoFixedSize.Height += rowHeights[rowIndex];
				}
			} else {
				totalAutoFixedSize.Height += rowHeights[rowIndex];
			}

			height += rowHeights[rowIndex];
		}

		return new Size(width, height) + Padding;
	}

	protected override Size Arrange(Size finalSize) {
		int y = Padding.Top;
		int x = Padding.Left;
		int columnIndex = 0;
		int rowIndex = 0;

		foreach(ControlBase child in this.Children) {
			int width = columnWidths[columnIndex];
			int height = rowHeights[rowIndex];

			if(!child.IsCollapsed) {
				if(requestedColumnWidths.Length != 0) {
					float w = requestedColumnWidths[columnIndex];
					if(w >= 0.0f && w <= 1.0f)
						width = Math.Max(0, (int)(w * (finalSize.Width - totalAutoFixedSize.Width)));
					else if(w > 1.0f)
						width = (int)w;
				}
				if(requestedRowHeights.Length != 0) {
					float h = requestedRowHeights[rowIndex];
					if(h >= 0.0f && h <= 1.0f)
						height = Math.Max(0, (int)(h * (finalSize.Height - totalAutoFixedSize.Height)));
					else if(h > 1.0f)
						height = (int)h;
				}

				child.DoArrange(new Rectangle(x, y, width, height));
			}

			x += width;
			columnIndex++;
			if(columnIndex == columnCount) {
				x = Padding.Left;
				y += height;
				columnIndex = 0;
				rowIndex++;
			}
		}

		return finalSize;
	}
}