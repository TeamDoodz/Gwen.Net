using System;
using Gwen.Net.Control.Internal;
using Gwen.Net.Input;

namespace Gwen.Net.Control;

/// <summary>
/// Text box (editable).
/// </summary>
[Xml.XmlControl]
public class TextBox : ControlBase {
	private readonly ScrollArea scrollArea;
	private readonly Text text;

	private bool selectAll;

	private int cursorPos;
	private int cursorEnd;

	protected Rectangle selectionBounds;
	protected Rectangle caretBounds;

	protected float lastInputTime;

	protected override bool AccelOnlyFocus { get { return true; } }
	protected override bool NeedsInputChars { get { return true; } }

	/// <summary>
	/// Determines whether text should be selected when the control is focused.
	/// </summary>
	[Xml.XmlProperty]
	public bool SelectAllOnFocus { get { return selectAll; } set { selectAll = value; if(value) OnSelectAll(this, EventArgs.Empty); } }

	/// <summary>
	/// Indicates whether the text has active selection.
	/// </summary>
	public virtual bool HasSelection { get { return cursorPos != cursorEnd; } }

	/// <summary>
	/// Invoked when the text has changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? TextChanged;

	/// <summary>
	/// Invoked when the submit key has been pressed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? SubmitPressed;

	/// <summary>
	/// Current cursor position (character index).
	/// </summary>
	public int CursorPos {
		get { return cursorPos; }
		set {
			if(cursorPos == value) return;

			cursorPos = value;
			RefreshCursorBounds();
		}
	}

	public int CursorEnd {
		get { return cursorEnd; }
		set {
			if(cursorEnd == value) return;

			cursorEnd = value;
			RefreshCursorBounds();
		}
	}

	/// <summary>
	/// Text.
	/// </summary>
	[Xml.XmlProperty]
	public virtual string Text { get { return text.Content; } set { SetText(value); } }

	/// <summary>
	/// Text color.
	/// </summary>
	[Xml.XmlProperty]
	public Color TextColor { get { return text.TextColor; } set { text.TextColor = value; } }

	/// <summary>
	/// Override text color (used by tooltips).
	/// </summary>
	[Xml.XmlProperty]
	public Color TextColorOverride { get { return text.TextColorOverride; } set { text.TextColorOverride = value; } }

	/// <summary>
	/// Text override - used to display different string.
	/// </summary>
	[Xml.XmlProperty]
	public string? TextOverride { get { return text.TextOverride; } set { text.TextOverride = value; } }

	/// <summary>
	/// Font.
	/// </summary>
	[Xml.XmlProperty]
	public Font? Font {
		get { return text.Font; }
		set {
			text.Font = value;
			DoFitToText();
			Invalidate();
		}
	}

	/// <summary>
	/// Set the size of the control to be able to show the text of this property.
	/// </summary>
	[Xml.XmlProperty]
	public string FitToText { get { return text.FitToText; } set { text.FitToText = value; DoFitToText(); } }

	/// <summary>
	/// Determines whether the control can insert text at a given cursor position.
	/// </summary>
	/// <param name="text">Text to check.</param>
	/// <param name="position">Cursor position.</param>
	/// <returns>True if allowed.</returns>
	protected virtual bool IsTextAllowed(string text, int position) {
		return true;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TextBox"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public TextBox(ControlBase? parent)
		: base(parent) {
		Padding = Padding.Three;

		scrollArea = new(this) {
			Dock = Dock.Fill
		};
		scrollArea.EnableScroll(true, false);

		text = new Text(scrollArea);
		text.TextColor = Skin.Colors.TextBox.Text;
		text.BoundsChanged += (s, a) => RefreshCursorBounds();

		MouseInputEnabled = true;
		KeyboardInputEnabled = true;
		KeyboardNeeded = true;

		cursorPos = 0;
		cursorEnd = 0;
		selectAll = false;

		IsTabable = true;

		AddAccelerator("Ctrl + C", OnCopy);
		AddAccelerator("Ctrl + X", OnCut);
		AddAccelerator("Ctrl + V", OnPaste);
		AddAccelerator("Ctrl + A", OnSelectAll);

		IsVirtualControl = true;
	}

	/// <summary>
	/// Sets the label text.
	/// </summary>
	/// <param name="str">Text to set.</param>
	/// <param name="doEvents">Determines whether to invoke "text changed" event.</param>
	public virtual void SetText(string str, bool doEvents = true) {
		if(Text == str)
			return;

		text.Content = str;

		if(cursorPos > text.Length)
			cursorPos = text.Length;

		if(doEvents)
			OnTextChanged();

		RefreshCursorBounds();
	}

	/// <summary>
	/// Inserts text at current cursor position, erasing selection if any.
	/// </summary>
	/// <param name="text">Text to insert.</param>
	protected virtual void InsertText(string text) {
		// TODO: Make sure fits (implement maxlength)

		if(HasSelection) {
			EraseSelection();
		}

		if(cursorPos > this.text.Length)
			cursorPos = this.text.Length;

		if(!IsTextAllowed(text, cursorPos))
			return;

		string str = Text;
		str = str.Insert(cursorPos, text);
		SetText(str);

		cursorPos += text.Length;
		cursorEnd = cursorPos;

		RefreshCursorBounds();
	}

	/// <summary>
	/// Deletes text.
	/// </summary>
	/// <param name="startPos">Starting cursor position.</param>
	/// <param name="length">Length in characters.</param>
	public virtual void DeleteText(int startPos, int length) {
		string str = Text;
		str = str.Remove(startPos, length);
		SetText(str);

		if(cursorPos > startPos) {
			CursorPos = cursorPos - length;
		}

		CursorEnd = cursorPos;
	}

	/// <summary>
	/// Handler for text changed event.
	/// </summary>
	protected virtual void OnTextChanged() {
		if(cursorPos > text.Length) cursorPos = text.Length;
		if(cursorEnd > text.Length) cursorEnd = text.Length;

		if(TextChanged != null)
			TextChanged.Invoke(this, EventArgs.Empty);
	}

	private void DoFitToText() {
		if(!String.IsNullOrWhiteSpace(this.FitToText)) {
			Size size = Font == null ? Size.One : Skin.Renderer.MeasureText(Font, this.FitToText);
			scrollArea.MinimumSize = size;
			Invalidate();
		}
	}

	/// <summary>
	/// Handler invoked on mouse click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="down">If set to <c>true</c> mouse button is down.</param>
	protected override void OnMouseClickedLeft(int x, int y, bool down) {
		base.OnMouseClickedLeft(x, y, down);
		if(selectAll) {
			OnSelectAll(this, EventArgs.Empty);
			//m_SelectAll = false;
			return;
		}

		int c = GetClosestCharacter(x, y).X;

		if(down) {
			CursorPos = c;

			if(!Input.InputHandler.IsShiftDown)
				CursorEnd = c;

			InputHandler.MouseFocus = this;
		} else {
			if(InputHandler.MouseFocus == this) {
				CursorPos = c;
				InputHandler.MouseFocus = null;
			}
		}
	}

	/// <summary>
	/// Handler invoked on mouse double click (left) event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	protected override void OnMouseDoubleClickedLeft(int x, int y) {
		//base.OnMouseDoubleClickedLeft(x, y);
		OnSelectAll(this, EventArgs.Empty);
	}

	/// <summary>
	/// Handler invoked on mouse moved event.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="dx">X change.</param>
	/// <param name="dy">Y change.</param>
	protected override void OnMouseMoved(int x, int y, int dx, int dy) {
		base.OnMouseMoved(x, y, dx, dy);
		if(InputHandler.MouseFocus != this) return;

		int c = GetClosestCharacter(x, y).X;

		CursorPos = c;
	}

	/// <summary>
	/// Handler for character input event.
	/// </summary>
	/// <param name="chr">Character typed.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnChar(char chr) {
		base.OnChar(chr);

		if(chr == '\t') return false;

		InsertText(chr.ToString());
		return true;
	}

	/// <summary>
	/// Handler for Paste event.
	/// </summary>
	/// <param name="from">Source control.</param>
	protected override void OnPaste(ControlBase from, EventArgs args) {
		base.OnPaste(from, args);
		InsertText(Platform.GwenPlatform.GetClipboardText());
	}

	/// <summary>
	/// Handler for Copy event.
	/// </summary>
	/// <param name="from">Source control.</param>
	protected override void OnCopy(ControlBase from, EventArgs args) {
		if(!HasSelection) return;
		base.OnCopy(from, args);

		Platform.GwenPlatform.SetClipboardText(GetSelection());
	}

	/// <summary>
	/// Handler for Cut event.
	/// </summary>
	/// <param name="from">Source control.</param>
	protected override void OnCut(ControlBase from, EventArgs args) {
		if(!HasSelection) return;
		base.OnCut(from, args);

		Platform.GwenPlatform.SetClipboardText(GetSelection());
		EraseSelection();
	}

	/// <summary>
	/// Handler for Select All event.
	/// </summary>
	/// <param name="from">Source control.</param>
	protected override void OnSelectAll(ControlBase from, EventArgs args) {
		//base.OnSelectAll(from);
		cursorEnd = 0;
		cursorPos = text.Length;

		RefreshCursorBounds();
	}

	/// <summary>
	/// Handler for Return keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyReturn(bool down) {
		base.OnKeyReturn(down);
		if(down) return true;

		OnReturn();

		// Try to move to the next control, as if tab had been pressed
		OnKeyTab(true);

		// If we still have focus, blur it.
		if(HasFocus) {
			Blur();
		}

		return true;
	}

	/// <summary>
	/// Handler for Escape keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyEscape(bool down) {
		base.OnKeyEscape(down);
		if(down) return true;

		// If we still have focus, blur it.
		if(HasFocus) {
			Blur();
		}

		return true;
	}

	/// <summary>
	/// Handler for Backspace keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyBackspace(bool down) {
		base.OnKeyBackspace(down);

		if(!down) return true;
		if(HasSelection) {
			EraseSelection();
			return true;
		}

		if(cursorPos == 0) return true;

		DeleteText(cursorPos - 1, 1);

		return true;
	}

	/// <summary>
	/// Handler for Delete keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyDelete(bool down) {
		base.OnKeyDelete(down);
		if(!down) return true;
		if(HasSelection) {
			EraseSelection();
			return true;
		}

		if(cursorPos >= text.Length) return true;

		DeleteText(cursorPos, 1);

		return true;
	}

	/// <summary>
	/// Handler for Left Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyLeft(bool down) {
		base.OnKeyLeft(down);
		if(!down) return true;

		if(cursorPos > 0)
			cursorPos--;

		if(!Input.InputHandler.IsShiftDown) {
			cursorEnd = cursorPos;
		}

		RefreshCursorBounds();
		return true;
	}

	/// <summary>
	/// Handler for Right Arrow keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyRight(bool down) {
		base.OnKeyRight(down);
		if(!down) return true;

		if(cursorPos < text.Length)
			cursorPos++;

		if(!Input.InputHandler.IsShiftDown) {
			cursorEnd = cursorPos;
		}

		RefreshCursorBounds();
		return true;
	}

	/// <summary>
	/// Handler for Home keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyHome(bool down) {
		base.OnKeyHome(down);
		if(!down) return true;
		cursorPos = 0;

		if(!Input.InputHandler.IsShiftDown) {
			cursorEnd = cursorPos;
		}

		RefreshCursorBounds();
		return true;
	}

	/// <summary>
	/// Handler for End keyboard event.
	/// </summary>
	/// <param name="down">Indicates whether the key was pressed or released.</param>
	/// <returns>
	/// True if handled.
	/// </returns>
	protected override bool OnKeyEnd(bool down) {
		base.OnKeyEnd(down);
		cursorPos = text.Length;

		if(!Input.InputHandler.IsShiftDown) {
			cursorEnd = cursorPos;
		}

		RefreshCursorBounds();
		return true;
	}

	/// <summary>
	/// Handler for the return key.
	/// </summary>
	protected virtual void OnReturn() {
		if(SubmitPressed != null)
			SubmitPressed.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Returns currently selected text.
	/// </summary>
	/// <returns>Current selection.</returns>
	public string GetSelection() {
		if(!HasSelection) return String.Empty;

		int start = Math.Min(cursorPos, cursorEnd);
		int end = Math.Max(cursorPos, cursorEnd);

		string str = Text;
		return str.Substring(start, end - start);
	}

	/// <summary>
	/// Deletes selected text.
	/// </summary>
	public virtual void EraseSelection() {
		int start = Math.Min(cursorPos, cursorEnd);
		int end = Math.Max(cursorPos, cursorEnd);

		DeleteText(start, end - start);

		// Move the cursor to the start of the selection, 
		// since the end is probably outside of the string now.
		cursorPos = start;
		cursorEnd = start;
	}

	protected override void OnBoundsChanged(Rectangle oldBounds) {
		RefreshCursorBounds();

		base.OnBoundsChanged(oldBounds);
	}

	/// <summary>
	/// Returns index of the character closest to specified point (in canvas coordinates).
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	protected virtual Point GetClosestCharacter(int x, int y) {
		return new Point(text.GetClosestCharacter(text.CanvasPosToLocal(new Point(x, y))), 0);
	}

	/// <summary>
	/// Gets the coordinates of specified character.
	/// </summary>
	/// <param name="index">Character index.</param>
	/// <returns>Character coordinates (local).</returns>
	public virtual Point GetCharacterPosition(int index) {
		Point p = text.GetCharacterPosition(index);
		return new Point(p.X + text.ActualLeft + Padding.Left, p.Y + text.ActualTop + Padding.Top);
	}

	protected virtual void MakeCaretVisible() {
		Size viewSize = scrollArea.ViewableContentSize;
		int caretPos = GetCharacterPosition(cursorPos).X;
		int realCaretPos = caretPos;

		caretPos -= text.ActualLeft;

		// If the caret is already in a semi-good position, leave it.
		if(realCaretPos > scrollArea.ActualWidth * 0.1f && realCaretPos < scrollArea.ActualWidth * 0.9f)
			return;

		// The ideal position is for the caret to be right in the middle
		int idealx = (int)(-caretPos + scrollArea.ActualWidth * 0.5f);

		// Don't show too much whitespace to the right
		if(idealx + text.MeasuredSize.Width < viewSize.Width)
			idealx = -text.MeasuredSize.Width + (viewSize.Width);

		// Or the left
		if(idealx > 0)
			idealx = 0;

		scrollArea.SetScrollPosition(idealx, 0);
	}

	protected virtual void RefreshCursorBounds() {
		lastInputTime = Platform.GwenPlatform.GetTimeInSeconds();

		MakeCaretVisible();

		Point pA = GetCharacterPosition(cursorPos);
		Point pB = GetCharacterPosition(cursorEnd);

		selectionBounds.X = Math.Min(pA.X, pB.X);
		selectionBounds.Y = pA.Y;
		selectionBounds.Width = Math.Max(pA.X, pB.X) - selectionBounds.X;
		selectionBounds.Height = text.ActualHeight;

		caretBounds.X = pA.X;
		caretBounds.Y = pA.Y;
		caretBounds.Width = 1;
		caretBounds.Height = text.ActualHeight;

		Redraw();
	}

	/// <summary>
	/// Renders the focus overlay.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void RenderFocus(Skin.SkinBase skin) {
		// nothing
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		base.Render(skin);

		if(ShouldDrawBackground)
			skin.DrawTextBox(this);

		if(!HasFocus) return;

		Rectangle oldClipRegion = skin.Renderer.ClipRegion;

		Rectangle clipRect = scrollArea.Bounds;
		clipRect.Width += 1; // Make space for caret
		skin.Renderer.SetClipRegion(clipRect);

		// Draw selection.. if selected..
		if(cursorPos != cursorEnd) {
			skin.Renderer.DrawColor = Skin.Colors.TextBox.Background_Selected;
			skin.Renderer.DrawFilledRect(selectionBounds);
		}

		// Draw caret
		float time = Platform.GwenPlatform.GetTimeInSeconds() - lastInputTime;

		if((time % 1.0f) <= 0.5f) {
			skin.Renderer.DrawColor = Skin.Colors.TextBox.Caret;
			skin.Renderer.DrawFilledRect(caretBounds);
		}

		skin.Renderer.ClipRegion = oldClipRegion;
	}
}