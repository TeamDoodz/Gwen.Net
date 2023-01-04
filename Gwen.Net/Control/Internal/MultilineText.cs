using System;
using System.Collections.Generic;
using System.Linq;

namespace Gwen.Net.Control.Internal;

/// <summary>
/// Multi line text.
/// </summary>
[Xml.XmlControl]
public class MultilineText : ControlBase {
	private List<Text> textLines = new List<Text>();

	private Font? font;

	private int lineHeight;

	/// <summary>
	/// Get or set text line.
	/// </summary>
	/// <param name="index">Line index.</param>
	/// <returns>Text.</returns>
	public string this[int index] {
		get {
			if(index < 0 && index >= textLines.Count)
				throw new ArgumentOutOfRangeException("index");

			return textLines[index].Content;
		}
		set {
			if(index < 0 && index >= textLines.Count)
				throw new ArgumentOutOfRangeException("index");

			textLines[index].Content = value;

			Invalidate();
		}
	}

	/// <summary>
	/// Returns the number of lines that are in the Multiline Text Box.
	/// </summary>
	public int TotalLines {
		get {
			return textLines.Count;
		}
	}

	/// <summary>
	/// Height of the text line in pixels.
	/// </summary>
	public int LineHeight {
		get {
			if(lineHeight == 0)
				lineHeight = Util.Ceil(Font?.FontMetrics.LineSpacingPixels ?? 0);

			return lineHeight;
		}
	}

	/// <summary>
	/// Gets and sets the text to display to the user. Each line is seperated by
	/// an Environment.NetLine character.
	/// </summary>
	[Xml.XmlProperty]
	public string Text {
		get {
			return String.Join(Environment.NewLine, textLines.Select(t => t.Content));
		}
		set {
			SetText(value);
		}
	}

	/// <summary>
	/// Font.
	/// </summary>
	[Xml.XmlProperty]
	public Font? Font {
		get { return font; }
		set {
			font = value;

			foreach(Text textCtrl in textLines)
				textCtrl.Font = value;

			lineHeight = 0;
			Invalidate();
		}
	}

	public MultilineText(ControlBase? parent)
		: base(parent) {

	}

	/// <summary>
	/// Sets the text.
	/// </summary>
	/// <param name="text">Text to set.</param>
	public void SetText(string text) {
		string[] lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		int index;
		for(index = 0; index < lines.Length; index++) {
			if(textLines.Count > index)
				textLines[index].Content = lines[index];
			else
				InsertLine(index, lines[index]);
		}
		for(; index < textLines.Count; index++) {
			RemoveLine(lines.Length);
		}

		Invalidate();
	}

	/// <summary>
	/// Inserts text at a position.
	/// </summary>
	/// <param name="text">Text to insert.</param>
	/// <param name="position">Position where to insert.</param>
	public Point InsertText(string text, Point position) {
		if(position.Y < 0 || position.Y >= textLines.Count)
			throw new ArgumentOutOfRangeException("position");

		if(position.X < 0 || position.X > textLines[position.Y].Content.Length)
			throw new ArgumentOutOfRangeException("position");

		if(text.Contains("\r") || text.Contains("\n")) {
			string[] newLines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

			string oldLineStart = textLines[position.Y].Content.Substring(0, position.X);
			string oldLineEnd = textLines[position.Y].Content.Substring(position.X);

			textLines[position.Y].Content = oldLineStart + newLines[0]; // First line
			for(int i = 1; i < newLines.Length - 1; i++) {
				InsertLine(position.Y + i, newLines[i]); // Middle lines
			}
			InsertLine(position.Y + newLines.Length - 1, newLines[newLines.Length - 1] + oldLineEnd); // Last line

			Invalidate();

			return new Point(newLines[newLines.Length - 1].Length, position.Y + newLines.Length - 1);
		} else {
			string str = textLines[position.Y].Content;
			str = str.Insert(position.X, text);
			textLines[position.Y].Content = str;

			Invalidate();

			return new Point(position.X + text.Length, position.Y);
		}
	}

	/// <summary>
	/// Add line to the end.
	/// </summary>
	/// <param name="text">Text to add.</param>
	public void AddLine(string text) {
		InsertLine(textLines.Count, text);
	}

	/// <summary>
	/// Insert a new line.
	/// </summary>
	/// <param name="index">Index where to insert.</param>
	/// <param name="text">Text to insert.</param>
	public void InsertLine(int index, string text) {
		if(index < 0 || index > textLines.Count)
			throw new ArgumentOutOfRangeException("index");

		Text textCtrl = new Text(this);
		textCtrl.Font = font;
		textCtrl.AutoSizeToContents = false;
		textCtrl.TextColor = Skin.Colors.TextBox.Text;
		textCtrl.Content = text;

		textLines.Insert(index, textCtrl);
		Invalidate();
	}

	/// <summary>
	/// Replace text line.
	/// </summary>
	/// <param name="index">Index what to replace.</param>
	/// <param name="text">New text.</param>
	public void ReplaceLine(int index, string text) {
		if(index < 0 || index >= textLines.Count)
			throw new ArgumentOutOfRangeException("index");

		textLines[index].Content = text;

		Invalidate();
	}

	/// <summary>
	/// Remove the line at the index.
	/// </summary>
	/// <param name="index">Index to remove.</param>
	public void RemoveLine(int index) {
		if(index < 0 || index >= textLines.Count)
			throw new ArgumentOutOfRangeException("index");

		RemoveChild(textLines[index], true);
		textLines.RemoveAt(index);

		Invalidate();
	}

	/// <summary>
	/// Remove all text.
	/// </summary>
	public void Clear() {
		foreach(Text textCtrl in textLines) {
			RemoveChild(textCtrl, true);
		}

		textLines.Clear();

		Invalidate();
	}

	/// <summary>
	/// Gets the coordinates of specified character position in the text.
	/// </summary>
	/// <param name="position">Character position.</param>
	/// <returns>Character position in local coordinates.</returns>
	public Point GetCharacterPosition(Point position) {
		if(position.Y < 0 || position.Y >= textLines.Count)
			throw new ArgumentOutOfRangeException("position");

		if(position.X < 0 || position.X > textLines[position.Y].Content.Length)
			throw new ArgumentOutOfRangeException("position");

		string currLine = textLines[position.Y].Content.Substring(0, Math.Min(position.X, textLines[position.Y].Length));

		Point p = new Point(Font == null ? 1 : Skin.Renderer.MeasureText(Font, currLine).Width, position.Y * LineHeight);

		return new Point(p.X + Padding.Left, p.Y + Padding.Top);
	}

	/// <summary>
	/// Returns position of the character closest to specified point.
	/// </summary>
	/// <param name="p">Point in local coordinates.</param>
	/// <returns>Character position.</returns>
	public Point GetClosestCharacter(Point p) {
		p.X -= Padding.Left;
		p.Y -= Padding.Top;

		Point best = new Point(0, 0);

		/* Find the appropriate Y (always pick a row whichever the mouse currently is on) */
		best.Y = Util.Clamp(p.Y / LineHeight, 0, textLines.Count - 1);

		/* Find the best X, closest char */
		best.X = textLines[best.Y].GetClosestCharacter(p);

		return best;
	}

	protected override Size Measure(Size availableSize) {
		availableSize -= Padding;

		int width = 0;
		int height = 0;
		int lineHeight = LineHeight;

		foreach(Text line in textLines) {
			Size size = line.DoMeasure(availableSize);
			availableSize.Height -= lineHeight;
			if(size.Width > width)
				width = size.Width;
			height += lineHeight;
		}

		return new Size(width + 2, height) + Padding;
	}

	protected override Size Arrange(Size finalSize) {
		finalSize -= Padding;

		int width = finalSize.Width;
		int y = Padding.Top;
		int lineHeight = LineHeight;

		foreach(Text line in textLines) {
			line.DoArrange(new Rectangle(Padding.Left, y, width, line.MeasuredSize.Height));
			y += lineHeight;
		}

		y += Padding.Bottom;

		return new Size(finalSize.Width + 2 + Padding.Left + Padding.Right, y);
	}
}