using System;

namespace Gwen.Net.Control;

/// <summary>
/// Single table row.
/// </summary>
public class TableRow : ControlBase {
	// [omeg] todo: get rid of this
	public const int MaxColumns = 5;

	private int columnCount;
	private bool evenRow;
	private readonly Label?[] columns;

	internal Label? GetColumn(int index) {
		return columns[index];
	}

	/// <summary>
	/// Invoked when the row has been selected.
	/// </summary>
	public event GwenEventHandler<ItemSelectedEventArgs>? Selected;

	/// <summary>
	/// Column count.
	/// </summary>
	public int ColumnCount { get { return columnCount; } set { SetColumnCount(value); } }

	/// <summary>
	/// Indicates whether the row is even or odd (used for alternate coloring).
	/// </summary>
	public bool EvenRow { get { return evenRow; } set { evenRow = value; } }

	/// <summary>
	/// Text of the first column.
	/// </summary>
	[Xml.XmlProperty]
	public string Text { get { return GetText(0); } set { SetCellText(0, value); } }

	/// <summary>
	/// Initializes a new instance of the <see cref="TableRow"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public TableRow(ControlBase? parent)
		: base(parent) {
		columns = new Label[MaxColumns];
		if(parent is ListBox box)
			columnCount = box.ColumnCount;
		else if(parent is Table table)
			columnCount = table.ColumnCount;
		KeyboardInputEnabled = true;
	}

	/// <summary>
	/// Sets the number of columns.
	/// </summary>
	/// <param name="columnCount">Number of columns.</param>
	protected void SetColumnCount(int columnCount) {
		if(columnCount == this.columnCount) return;

		if(columnCount >= MaxColumns)
			throw new ArgumentException("Invalid column count", nameof(columnCount));

		for(int i = 0; i < MaxColumns; i++) {
			if(i < columnCount) {
				if(columns[i] != null) {
					continue;
				}

				columns[i] = new(this) {
					Padding = Padding.Three,
					Margin = new Margin(0, 0, 2, 0), // to separate them slightly
					TextColor = Skin.Colors.ListBox.Text_Normal
				};
			} else if(columns[i] is Label columni) {
				RemoveChild(columni, true);
				columns[i] = null;
			}
		}

		this.columnCount = columnCount;
	}

	/// <summary>
	/// Sets the column width (in pixels).
	/// </summary>
	/// <param name="columnIndex">Column index.</param>
	/// <param name="width">Column width.</param>
	public void SetColumnWidth(int columnIndex, int width) {
		if(columns[columnIndex] is not Label column)
			return;
		if(column.Width == width)
			return;

		column .Width = width;
	}

	/// <summary>
	/// Sets the text of a specified cell.
	/// </summary>
	/// <param name="columnIndex">Column number.</param>
	/// <param name="text">Text to set.</param>
	public void SetCellText(int columnIndex, string text) {
		if(columnIndex >= columnCount)
			throw new ArgumentException("Invalid column index", nameof(columnIndex));

		if(columns[columnIndex] is not Label column) {
			column = new Label(this);
			column.Padding = Padding.Three;
			column.Margin = new Margin(0, 0, 2, 0); // to separate them slightly
			column.TextColor = Skin.Colors.ListBox.Text_Normal;
			columns[columnIndex] = column;
		}

		column.Text = text;
	}

	/// <summary>
	/// Sets the contents of a specified cell.
	/// </summary>
	/// <param name="column">Column number.</param>
	/// <param name="control">Cell contents.</param>
	/// <param name="enableMouseInput">Determines whether mouse input should be enabled for the cell.</param>
	public void SetCellContents(int column, ControlBase control, bool enableMouseInput = false) {
		if(columns[column] is not Label columnLabel)
			return;

		control.Parent = columnLabel;
		columnLabel.MouseInputEnabled = enableMouseInput;
	}

	/// <summary>
	/// Gets the contents of a specified cell.
	/// </summary>
	/// <param name="column">Column number.</param>
	/// <returns>Control embedded in the cell.</returns>
	public ControlBase? GetCellContents(int column) {
		return columns[column];
	}

	protected virtual void OnRowSelected() {
		if(Selected != null)
			Selected.Invoke(this, new ItemSelectedEventArgs(this));
	}

	protected override Size Measure(Size availableSize) {
		int width = 0;
		int height = 0;

		for(int i = 0; i < columnCount; i++) {
			if(columns[i] is not Label columnLabel)
				continue;

			Size size = columnLabel.DoMeasure(new Size(availableSize.Width - width, availableSize.Height));

			width += size.Width;
			height = Math.Max(height, size.Height);
		}

		return new Size(width, height);
	}

	protected override Size Arrange(Size finalSize) {
		int x = 0;
		int height = 0;

		for(int i = 0; i < columnCount; i++) {
			if(columns[i] is not Label columnLabel)
				continue;

			if(i == columnCount - 1)
				columnLabel.DoArrange(new Rectangle(x, 0, finalSize.Width - x, columnLabel.MeasuredSize.Height));
			else
				columnLabel.DoArrange(new Rectangle(x, 0, columnLabel.MeasuredSize.Width, columnLabel.MeasuredSize.Height));
			x += columnLabel.MeasuredSize.Width;
			height = Math.Max(height, columnLabel.MeasuredSize.Height);
		}

		return new Size(finalSize.Width, height);
	}

	/// <summary>
	/// Sets the text color for all cells.
	/// </summary>
	/// <param name="color">Text color.</param>
	public void SetTextColor(Color color) {
		for(int i = 0; i < columnCount; i++) {
			if(columns[i] is Label columnLabel) {
				columnLabel.TextColor = color;
			}
		}
	}

	/// <summary>
	/// Returns text of a specified row cell (default first).
	/// </summary>
	/// <param name="column">Column index.</param>
	/// <returns>Column cell text.</returns>
	public string GetText(int column = 0) {
		return columns[column]?.Text ?? "";
	}

	/// <summary>
	/// Handler for Copy event.
	/// </summary>
	/// <param name="from">Source control.</param>
	protected override void OnCopy(ControlBase from, EventArgs args) {
		Platform.GwenPlatform.SetClipboardText(Text);
	}
}