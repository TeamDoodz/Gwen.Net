﻿namespace Gwen.Net.RichText;

public class LineBreakPart : Part {
	public override string[] Split(ref Font? font) {
		return new string[] { "\n" };
	}
}