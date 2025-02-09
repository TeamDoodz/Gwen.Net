﻿using System;

namespace Gwen.Net;

/// <summary>
/// Represents outer spacing.
/// </summary>
public struct Margin : IEquatable<Margin> {
	public int Top;
	public int Bottom;
	public int Left;
	public int Right;

	// common values
	public static Margin Zero = new Margin(0);
	public static Margin One = new Margin(1);
	public static Margin Two = new Margin(2);
	public static Margin Three = new Margin(3);
	public static Margin Four = new Margin(4);
	public static Margin Five = new Margin(5);
	public static Margin Six = new Margin(6);
	public static Margin Seven = new Margin(7);
	public static Margin Eight = new Margin(8);
	public static Margin Nine = new Margin(9);
	public static Margin Ten = new Margin(10);

	public Margin(int left, int top, int right, int bottom) {
		Top = top;
		Bottom = bottom;
		Left = left;
		Right = right;
	}

	public Margin(int horizontal, int vertical) {
		Top = vertical;
		Bottom = vertical;
		Left = horizontal;
		Right = horizontal;
	}

	public Margin(int margin) {
		Top = margin;
		Bottom = margin;
		Left = margin;
		Right = margin;
	}

	public bool Equals(Margin other) {
		return other.Top == Top && other.Bottom == Bottom && other.Left == Left && other.Right == Right;
	}

	public static bool operator ==(Margin lhs, Margin rhs) {
		return lhs.Equals(rhs);
	}

	public static bool operator !=(Margin lhs, Margin rhs) {
		return !lhs.Equals(rhs);
	}

	public static Margin operator +(Margin lhs, Margin rhs) {
		return new Margin(lhs.Left + rhs.Left, lhs.Top + rhs.Top, lhs.Right + rhs.Right, lhs.Bottom + rhs.Bottom);
	}

	public static Margin operator -(Margin lhs, Margin rhs) {
		return new Margin(lhs.Left - rhs.Left, lhs.Top - rhs.Top, lhs.Right - rhs.Right, lhs.Bottom - rhs.Bottom);
	}

	public override bool Equals(object? obj) {
		return obj is Margin margin && Equals(margin);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Top, Bottom, Left, Right);
	}

	public static explicit operator Margin(Padding padding) {
		return new Margin(padding.Left, padding.Top, padding.Right, padding.Bottom);
	}

	public override string ToString() {
		return String.Format("Left = {0} Top = {1} Right = {2} Bottom = {3}", Left, Top, Right, Bottom);
	}
}