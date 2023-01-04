using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gwen.Net;

// TODO: This class should be made internal as it is bad practice to expose utility methods like this. The reason why this has not been done yet is because it would break everything.

/// <summary>
/// Misc utility functions. Don't use this class outside of Gwen.Net namespaces.
/// </summary>
public static class Util {
	public static int Ceil(float x) {
		return (int)Math.Ceiling(x);
	}

	public static Rectangle FloatRect(float x, float y, float w, float h) {
		return new Rectangle((int)x, (int)y, (int)w, (int)h);
	}

	public static int Clamp(int x, int min, int max) {
		if (x < min)
			return min;
		if (x > max)
			return max;
		return x;
	}

	public static float Clamp(float x, float min, float max) {
		if (x < min)
			return min;
		if (x > max)
			return max;
		return x;
	}

	// can't create extension operators
	public static Color Subtract(this Color color, Color other) {
		return new Color(color.A - other.A, color.R - other.R, color.G - other.G, color.B - other.B);
	}

	public static Color Add(this Color color, Color other) {
		return new Color(color.A + other.A, color.R + other.R, color.G + other.G, color.B + other.B);
	}

	public static Color Multiply(this Color color, float amount) {
		return new Color(color.A, (int)(color.R * amount), (int)(color.G * amount), (int)(color.B * amount));
	}

	public static Rectangle Add(this Rectangle r, Rectangle other) {
		return new Rectangle(r.X + other.X, r.Y + other.Y, r.Width + other.Width, r.Height + other.Height);
	}

	/// <summary>
	/// Splits a string but keeps the separators intact.
	/// </summary>
	/// <param name="text">String to split.</param>
	/// <param name="separators">Separator characters.</param>
	/// <returns>Split strings.</returns>
	public static string[] SplitAndKeep(string text, string separators) {
		List<string> strs = new List<string>();
		int offset = 0;
		int length = text.Length;
		int sepLen = separators.Length;
		int i = text.IndexOf(separators);
		string word;

		while (i != -1) {
			word = text.Substring(offset, i - offset);
			if (!String.IsNullOrWhiteSpace(word))
				strs.Add(word);
			offset = i + sepLen;
			i = text.IndexOf(separators, offset);
			offset -= sepLen;
		}

		strs.Add(text.Substring(offset, length - offset));

		return strs.ToArray();
	}

	public const int Ignore = -1;
	public static bool IsIgnore(int value) {
		return value == Ignore;
	}

	public const int Infinity = 0xfffffff;
	public static bool IsInfinity(int value) {
		return value > 0xffffff;
	}
}