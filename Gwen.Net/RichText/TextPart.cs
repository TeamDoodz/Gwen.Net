using System.Collections.Generic;

namespace Gwen.Net.RichText;

public class TextPart : Part {
	private string text;
	private Color? color;
	private Font? font;

	public string Text { get { return text; } }
	public Color? Color { get { return color; } }
	public Font? Font { get { return font; } protected set { font = value; } }

	public TextPart(string text) {
		this.text = text;
		color = null;
	}

	public TextPart(string text, Color color) {
		this.text = text;
		this.color = color;
	}

	public override string[] Split(ref Font? font) {
		this.font = font;

		return StringSplit(text);
	}

	protected string[] StringSplit(string str) {
		List<string> strs = new List<string>();
		int len = str.Length;
		int index = 0;
		int i;

		while(index < len) {
			i = str.IndexOfAny(seperator, index);
			if(i == index) {
				if(str[i] == ' ') {
					strs.Add(" ");
					while(index < len && str[index] == ' ')
						index++;
				} else {
					strs.Add("\n");
					index++;
					if(index < len && str[index - 1] == '\r' && str[index] == '\n')
						index++;
				}
			} else if(i != -1) {
				if(str[i] == ' ') {
					strs.Add(str.Substring(index, i - index + 1));
					index = i + 1;
				} else {
					strs.Add(str.Substring(index, i - index));
					index = i;
				}
			} else {
				strs.Add(str.Substring(index));
				break;
			}
		}

		return strs.ToArray();
	}

	private static readonly char[] seperator = new char[] { ' ', '\n', '\r' };
}