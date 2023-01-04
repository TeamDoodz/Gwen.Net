using System;

namespace Gwen.Net;

public readonly struct Anchor : IEquatable<Anchor> {
	public byte Top { get; }
	public byte Bottom { get; }
	public byte Left { get; }
	public byte Right { get; }

	public static Anchor LeftTop { get; } = new Anchor(0, 0, 0, 0);
	public static Anchor RightTop { get; } = new Anchor(100, 0, 100, 0);
	public static Anchor LeftBottom { get; } = new Anchor(0, 100, 0, 100);
	public static Anchor RightBottom { get; } = new Anchor(100, 100, 100, 100);

	public Anchor(byte left, byte top, byte right, byte bottom) {
		Top = top;
		Bottom = bottom;
		Left = left;
		Right = right;
	}

	public bool Equals(Anchor other) {
		return other.Top == Top && other.Bottom == Bottom && other.Left == Left && other.Right == Right;
	}

	public static bool operator ==(Anchor lhs, Anchor rhs) {
		return lhs.Equals(rhs);
	}

	public static bool operator !=(Anchor lhs, Anchor rhs) {
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object? obj) {
		if (ReferenceEquals(null, obj)) return false;
		if (obj.GetType() != typeof(Anchor)) return false;
		return Equals((Anchor)obj);
	}

	public override int GetHashCode() {
		unchecked {
			int result = Top;
			result |= Bottom << 8;
			result |= Left << 16;
			result |= Right << 24;
			return result;
		}
	}
}