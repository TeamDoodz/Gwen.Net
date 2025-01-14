﻿using System;
using System.Collections.Generic;

namespace Gwen.Net;

/// <summary>
/// Represent ARGB color.
/// </summary>
public struct Color {
	/// <summary>
	/// Red value.
	/// </summary>
	public byte R { get; set; }
	/// <summary>
	/// Green value.
	/// </summary>
	public byte G { get; set; }
	/// <summary>
	/// Blue value.
	/// </summary>
	public byte B { get; set; }
	/// <summary>
	/// Alpha value.
	/// </summary>
	public byte A { get; set; }

	/// <summary>
	/// Initializes new color. Alpha value is 255.
	/// </summary>
	/// <param name="r">Red value.</param>
	/// <param name="g">Green value.</param>
	/// <param name="b">Blue value.</param>
	public Color(int r, int g, int b) {
		R = (byte)r;
		G = (byte)g;
		B = (byte)b;
		A = 255;
	}

	/// <summary>
	/// Initializes new color.
	/// </summary>
	/// <param name="a">Alpha value.</param>
	/// <param name="r">Red value.</param>
	/// <param name="g">Green value.</param>
	/// <param name="b">Blue value.</param>
	public Color(int a, int r, int g, int b) {
		R = (byte)r;
		G = (byte)g;
		B = (byte)b;
		A = (byte)a;
	}

	/// <summary>
	/// Initializes new color.
	/// </summary>
	/// <param name="value">32 bit color value as 0xAARRGGBB. Alpha value 0 is treated as 255.</param>
	public Color(uint value) {
		unchecked {
			R = (byte)((value >> 16) & 0xff);
			G = (byte)((value >> 8) & 0xff);
			B = (byte)((value) & 0xff);
			A = (byte)((value >> 24) & 0xff);

			if (A == 0) {
				A = 255;
			}
		}
	}

	/// <summary>
	/// Get color by name.
	/// </summary>
	/// <param name="name">HTML color name supported by browsers.</param>
	/// <returns>Color if named value exists, color black otherwise.</returns>
	public static Color FromName(string name) {
		if (namedColors.TryGetValue(name, out Color color)) {
			return color;
		} else {
			return Black;
		}
	}

	/// <summary>
	/// Linearly interpolates between two colors.
	/// </summary>
	public static Color Lerp(Color toColor, Color fromColor, float amount) {
		Color delta = toColor.Subtract(fromColor);
		delta = delta.Multiply(amount);
		return fromColor.Add(delta);
	}

	public override string ToString() {
		return String.Format("R = {0} G = {1} B = {2} A = {3}", R, G, B, A);
	}

	public static Color AliceBlue => new Color(0xF0F8FF);
	public static Color AntiqueWhite => new Color(0xFAEBD7);
	public static Color Aqua => new Color(0x00FFFF);
	public static Color Aquamarine => new Color(0x7FFFD4);
	public static Color Azure => new Color(0xF0FFFF);
	public static Color Beige => new Color(0xF5F5DC);
	public static Color Bisque => new Color(0xFFE4C4);
	public static Color Black => new Color(0x000000);
	public static Color BlanchedAlmond => new Color(0xFFEBCD);
	public static Color Blue => new Color(0x0000FF);
	public static Color BlueViolet => new Color(0x8A2BE2);
	public static Color Brown => new Color(0xA52A2A);
	public static Color BurlyWood => new Color(0xDEB887);
	public static Color CadetBlue => new Color(0x5F9EA0);
	public static Color Chartreuse => new Color(0x7FFF00);
	public static Color Chocolate => new Color(0xD2691E);
	public static Color Coral => new Color(0xFF7F50);
	public static Color CornflowerBlue => new Color(0x6495ED);
	public static Color Cornsilk => new Color(0xFFF8DC);
	public static Color Crimson => new Color(0xDC143C);
	public static Color Cyan => new Color(0x00FFFF);
	public static Color DarkBlue => new Color(0x00008B);
	public static Color DarkCyan => new Color(0x008B8B);
	public static Color DarkGoldenRod => new Color(0xB8860B);
	public static Color DarkGray => new Color(0xA9A9A9);
	public static Color DarkGrey => new Color(0xA9A9A9);
	public static Color DarkGreen => new Color(0x006400);
	public static Color DarkKhaki => new Color(0xBDB76B);
	public static Color DarkMagenta => new Color(0x8B008B);
	public static Color DarkOliveGreen => new Color(0x556B2F);
	public static Color DarkOrange => new Color(0xFF8C00);
	public static Color DarkOrchid => new Color(0x9932CC);
	public static Color DarkRed => new Color(0x8B0000);
	public static Color DarkSalmon => new Color(0xE9967A);
	public static Color DarkSeaGreen => new Color(0x8FBC8F);
	public static Color DarkSlateBlue => new Color(0x483D8B);
	public static Color DarkSlateGray => new Color(0x2F4F4F);
	public static Color DarkSlateGrey => new Color(0x2F4F4F);
	public static Color DarkTurquoise => new Color(0x00CED1);
	public static Color DarkViolet => new Color(0x9400D3);
	public static Color DeepPink => new Color(0xFF1493);
	public static Color DeepSkyBlue => new Color(0x00BFFF);
	public static Color DimGray => new Color(0x696969);
	public static Color DimGrey => new Color(0x696969);
	public static Color DodgerBlue => new Color(0x1E90FF);
	public static Color FireBrick => new Color(0xB22222);
	public static Color FloralWhite => new Color(0xFFFAF0);
	public static Color ForestGreen => new Color(0x228B22);
	public static Color Fuchsia => new Color(0xFF00FF);
	public static Color Gainsboro => new Color(0xDCDCDC);
	public static Color GhostWhite => new Color(0xF8F8FF);
	public static Color Gold => new Color(0xFFD700);
	public static Color GoldenRod => new Color(0xDAA520);
	public static Color Gray => new Color(0x808080);
	public static Color Grey => new Color(0x808080);
	public static Color Green => new Color(0x008000);
	public static Color GreenYellow => new Color(0xADFF2F);
	public static Color HoneyDew => new Color(0xF0FFF0);
	public static Color HotPink => new Color(0xFF69B4);
	public static Color IndianRed => new Color(0xCD5C5C);
	public static Color Indigo => new Color(0x4B0082);
	public static Color Ivory => new Color(0xFFFFF0);
	public static Color Khaki => new Color(0xF0E68C);
	public static Color Lavender => new Color(0xE6E6FA);
	public static Color LavenderBlush => new Color(0xFFF0F5);
	public static Color LawnGreen => new Color(0x7CFC00);
	public static Color LemonChiffon => new Color(0xFFFACD);
	public static Color LightBlue => new Color(0xADD8E6);
	public static Color LightCoral => new Color(0xF08080);
	public static Color LightCyan => new Color(0xE0FFFF);
	public static Color LightGoldenRodYellow => new Color(0xFAFAD2);
	public static Color LightGray => new Color(0xD3D3D3);
	public static Color LightGrey => new Color(0xD3D3D3);
	public static Color LightGreen => new Color(0x90EE90);
	public static Color LightPink => new Color(0xFFB6C1);
	public static Color LightSalmon => new Color(0xFFA07A);
	public static Color LightSeaGreen => new Color(0x20B2AA);
	public static Color LightSkyBlue => new Color(0x87CEFA);
	public static Color LightSlateGray => new Color(0x778899);
	public static Color LightSlateGrey => new Color(0x778899);
	public static Color LightSteelBlue => new Color(0xB0C4DE);
	public static Color LightYellow => new Color(0xFFFFE0);
	public static Color Lime => new Color(0x00FF00);
	public static Color LimeGreen => new Color(0x32CD32);
	public static Color Linen => new Color(0xFAF0E6);
	public static Color Magenta => new Color(0xFF00FF);
	public static Color Maroon => new Color(0x800000);
	public static Color MediumAquaMarine => new Color(0x66CDAA);
	public static Color MediumBlue => new Color(0x0000CD);
	public static Color MediumOrchid => new Color(0xBA55D3);
	public static Color MediumPurple => new Color(0x9370DB);
	public static Color MediumSeaGreen => new Color(0x3CB371);
	public static Color MediumSlateBlue => new Color(0x7B68EE);
	public static Color MediumSpringGreen => new Color(0x00FA9A);
	public static Color MediumTurquoise => new Color(0x48D1CC);
	public static Color MediumVioletRed => new Color(0xC71585);
	public static Color MidnightBlue => new Color(0x191970);
	public static Color MintCream => new Color(0xF5FFFA);
	public static Color MistyRose => new Color(0xFFE4E1);
	public static Color Moccasin => new Color(0xFFE4B5);
	public static Color NavajoWhite => new Color(0xFFDEAD);
	public static Color Navy => new Color(0x000080);
	public static Color OldLace => new Color(0xFDF5E6);
	public static Color Olive => new Color(0x808000);
	public static Color OliveDrab => new Color(0x6B8E23);
	public static Color Orange => new Color(0xFFA500);
	public static Color OrangeRed => new Color(0xFF4500);
	public static Color Orchid => new Color(0xDA70D6);
	public static Color PaleGoldenRod => new Color(0xEEE8AA);
	public static Color PaleGreen => new Color(0x98FB98);
	public static Color PaleTurquoise => new Color(0xAFEEEE);
	public static Color PaleVioletRed => new Color(0xDB7093);
	public static Color PapayaWhip => new Color(0xFFEFD5);
	public static Color PeachPuff => new Color(0xFFDAB9);
	public static Color Peru => new Color(0xCD853F);
	public static Color Pink => new Color(0xFFC0CB);
	public static Color Plum => new Color(0xDDA0DD);
	public static Color PowderBlue => new Color(0xB0E0E6);
	public static Color Purple => new Color(0x800080);
	public static Color RebeccaPurple => new Color(0x663399);
	public static Color Red => new Color(0xFF0000);
	public static Color RosyBrown => new Color(0xBC8F8F);
	public static Color RoyalBlue => new Color(0x4169E1);
	public static Color SaddleBrown => new Color(0x8B4513);
	public static Color Salmon => new Color(0xFA8072);
	public static Color SandyBrown => new Color(0xF4A460);
	public static Color SeaGreen => new Color(0x2E8B57);
	public static Color SeaShell => new Color(0xFFF5EE);
	public static Color Sienna => new Color(0xA0522D);
	public static Color Silver => new Color(0xC0C0C0);
	public static Color SkyBlue => new Color(0x87CEEB);
	public static Color SlateBlue => new Color(0x6A5ACD);
	public static Color SlateGray => new Color(0x708090);
	public static Color SlateGrey => new Color(0x708090);
	public static Color Snow => new Color(0xFFFAFA);
	public static Color SpringGreen => new Color(0x00FF7F);
	public static Color SteelBlue => new Color(0x4682B4);
	public static Color Tan => new Color(0xD2B48C);
	public static Color Teal => new Color(0x008080);
	public static Color Thistle => new Color(0xD8BFD8);
	public static Color Tomato => new Color(0xFF6347);
	public static Color Turquoise => new Color(0x40E0D0);
	public static Color Violet => new Color(0xEE82EE);
	public static Color Wheat => new Color(0xF5DEB3);
	public static Color White => new Color(0xFFFFFF);
	public static Color WhiteSmoke => new Color(0xF5F5F5);
	public static Color Yellow => new Color(0xFFFF00);
	public static Color YellowGreen => new Color(0x9ACD32);
	public static Color GwenPink => new Color(255, 65, 199);
	public static Color Transparent => new Color(0, 255, 255, 255);

	private static readonly Dictionary<string, Color> namedColors = new Dictionary<string, Color>() {
		{ "AliceBlue"               , new Color(0xF0F8FF) },
		{ "AntiqueWhite"            , new Color(0xFAEBD7) },
		{ "Aqua"                    , new Color(0x00FFFF) },
		{ "Aquamarine"              , new Color(0x7FFFD4) },
		{ "Azure"                   , new Color(0xF0FFFF) },
		{ "Beige"                   , new Color(0xF5F5DC) },
		{ "Bisque"                  , new Color(0xFFE4C4) },
		{ "Black"                   , new Color(0x000000) },
		{ "BlanchedAlmond"          , new Color(0xFFEBCD) },
		{ "Blue"                    , new Color(0x0000FF) },
		{ "BlueViolet"              , new Color(0x8A2BE2) },
		{ "Brown"                   , new Color(0xA52A2A) },
		{ "BurlyWood"               , new Color(0xDEB887) },
		{ "CadetBlue"               , new Color(0x5F9EA0) },
		{ "Chartreuse"              , new Color(0x7FFF00) },
		{ "Chocolate"               , new Color(0xD2691E) },
		{ "Coral"                   , new Color(0xFF7F50) },
		{ "CornflowerBlue"          , new Color(0x6495ED) },
		{ "Cornsilk"                , new Color(0xFFF8DC) },
		{ "Crimson"                 , new Color(0xDC143C) },
		{ "Cyan"                    , new Color(0x00FFFF) },
		{ "DarkBlue"                , new Color(0x00008B) },
		{ "DarkCyan"                , new Color(0x008B8B) },
		{ "DarkGoldenRod"           , new Color(0xB8860B) },
		{ "DarkGray"                , new Color(0xA9A9A9) },
		{ "DarkGrey"                , new Color(0xA9A9A9) },
		{ "DarkGreen"               , new Color(0x006400) },
		{ "DarkKhaki"               , new Color(0xBDB76B) },
		{ "DarkMagenta"             , new Color(0x8B008B) },
		{ "DarkOliveGreen"          , new Color(0x556B2F) },
		{ "DarkOrange"              , new Color(0xFF8C00) },
		{ "DarkOrchid"              , new Color(0x9932CC) },
		{ "DarkRed"                 , new Color(0x8B0000) },
		{ "DarkSalmon"              , new Color(0xE9967A) },
		{ "DarkSeaGreen"            , new Color(0x8FBC8F) },
		{ "DarkSlateBlue"           , new Color(0x483D8B) },
		{ "DarkSlateGray"           , new Color(0x2F4F4F) },
		{ "DarkSlateGrey"           , new Color(0x2F4F4F) },
		{ "DarkTurquoise"           , new Color(0x00CED1) },
		{ "DarkViolet"              , new Color(0x9400D3) },
		{ "DeepPink"                , new Color(0xFF1493) },
		{ "DeepSkyBlue"             , new Color(0x00BFFF) },
		{ "DimGray"                 , new Color(0x696969) },
		{ "DimGrey"                 , new Color(0x696969) },
		{ "DodgerBlue"              , new Color(0x1E90FF) },
		{ "FireBrick"               , new Color(0xB22222) },
		{ "FloralWhite"             , new Color(0xFFFAF0) },
		{ "ForestGreen"             , new Color(0x228B22) },
		{ "Fuchsia"                 , new Color(0xFF00FF) },
		{ "Gainsboro"               , new Color(0xDCDCDC) },
		{ "GhostWhite"              , new Color(0xF8F8FF) },
		{ "Gold"                    , new Color(0xFFD700) },
		{ "GoldenRod"               , new Color(0xDAA520) },
		{ "Gray"                    , new Color(0x808080) },
		{ "Grey"                    , new Color(0x808080) },
		{ "Green"                   , new Color(0x008000) },
		{ "GreenYellow"             , new Color(0xADFF2F) },
		{ "HoneyDew"                , new Color(0xF0FFF0) },
		{ "HotPink"                 , new Color(0xFF69B4) },
		{ "IndianRed"               , new Color(0xCD5C5C) },
		{ "Indigo"                  , new Color(0x4B0082) },
		{ "Ivory"                   , new Color(0xFFFFF0) },
		{ "Khaki"                   , new Color(0xF0E68C) },
		{ "Lavender"                , new Color(0xE6E6FA) },
		{ "LavenderBlush"           , new Color(0xFFF0F5) },
		{ "LawnGreen"               , new Color(0x7CFC00) },
		{ "LemonChiffon"            , new Color(0xFFFACD) },
		{ "LightBlue"               , new Color(0xADD8E6) },
		{ "LightCoral"              , new Color(0xF08080) },
		{ "LightCyan"               , new Color(0xE0FFFF) },
		{ "LightGoldenRodYellow"    , new Color(0xFAFAD2) },
		{ "LightGray"               , new Color(0xD3D3D3) },
		{ "LightGrey"               , new Color(0xD3D3D3) },
		{ "LightGreen"              , new Color(0x90EE90) },
		{ "LightPink"               , new Color(0xFFB6C1) },
		{ "LightSalmon"             , new Color(0xFFA07A) },
		{ "LightSeaGreen"           , new Color(0x20B2AA) },
		{ "LightSkyBlue"            , new Color(0x87CEFA) },
		{ "LightSlateGray"          , new Color(0x778899) },
		{ "LightSlateGrey"          , new Color(0x778899) },
		{ "LightSteelBlue"          , new Color(0xB0C4DE) },
		{ "LightYellow"             , new Color(0xFFFFE0) },
		{ "Lime"                    , new Color(0x00FF00) },
		{ "LimeGreen"               , new Color(0x32CD32) },
		{ "Linen"                   , new Color(0xFAF0E6) },
		{ "Magenta"                 , new Color(0xFF00FF) },
		{ "Maroon"                  , new Color(0x800000) },
		{ "MediumAquaMarine"        , new Color(0x66CDAA) },
		{ "MediumBlue"              , new Color(0x0000CD) },
		{ "MediumOrchid"            , new Color(0xBA55D3) },
		{ "MediumPurple"            , new Color(0x9370DB) },
		{ "MediumSeaGreen"          , new Color(0x3CB371) },
		{ "MediumSlateBlue"         , new Color(0x7B68EE) },
		{ "MediumSpringGreen"       , new Color(0x00FA9A) },
		{ "MediumTurquoise"         , new Color(0x48D1CC) },
		{ "MediumVioletRed"         , new Color(0xC71585) },
		{ "MidnightBlue"            , new Color(0x191970) },
		{ "MintCream"               , new Color(0xF5FFFA) },
		{ "MistyRose"               , new Color(0xFFE4E1) },
		{ "Moccasin"                , new Color(0xFFE4B5) },
		{ "NavajoWhite"             , new Color(0xFFDEAD) },
		{ "Navy"                    , new Color(0x000080) },
		{ "OldLace"                 , new Color(0xFDF5E6) },
		{ "Olive"                   , new Color(0x808000) },
		{ "OliveDrab"               , new Color(0x6B8E23) },
		{ "Orange"                  , new Color(0xFFA500) },
		{ "OrangeRed"               , new Color(0xFF4500) },
		{ "Orchid"                  , new Color(0xDA70D6) },
		{ "PaleGoldenRod"           , new Color(0xEEE8AA) },
		{ "PaleGreen"               , new Color(0x98FB98) },
		{ "PaleTurquoise"           , new Color(0xAFEEEE) },
		{ "PaleVioletRed"           , new Color(0xDB7093) },
		{ "PapayaWhip"              , new Color(0xFFEFD5) },
		{ "PeachPuff"               , new Color(0xFFDAB9) },
		{ "Peru"                    , new Color(0xCD853F) },
		{ "Pink"                    , new Color(0xFFC0CB) },
		{ "Plum"                    , new Color(0xDDA0DD) },
		{ "PowderBlue"              , new Color(0xB0E0E6) },
		{ "Purple"                  , new Color(0x800080) },
		{ "RebeccaPurple"           , new Color(0x663399) },
		{ "Red"                     , new Color(0xFF0000) },
		{ "RosyBrown"               , new Color(0xBC8F8F) },
		{ "RoyalBlue"               , new Color(0x4169E1) },
		{ "SaddleBrown"             , new Color(0x8B4513) },
		{ "Salmon"                  , new Color(0xFA8072) },
		{ "SandyBrown"              , new Color(0xF4A460) },
		{ "SeaGreen"                , new Color(0x2E8B57) },
		{ "SeaShell"                , new Color(0xFFF5EE) },
		{ "Sienna"                  , new Color(0xA0522D) },
		{ "Silver"                  , new Color(0xC0C0C0) },
		{ "SkyBlue"                 , new Color(0x87CEEB) },
		{ "SlateBlue"               , new Color(0x6A5ACD) },
		{ "SlateGray"               , new Color(0x708090) },
		{ "SlateGrey"               , new Color(0x708090) },
		{ "Snow"                    , new Color(0xFFFAFA) },
		{ "SpringGreen"             , new Color(0x00FF7F) },
		{ "SteelBlue"               , new Color(0x4682B4) },
		{ "Tan"                     , new Color(0xD2B48C) },
		{ "Teal"                    , new Color(0x008080) },
		{ "Thistle"                 , new Color(0xD8BFD8) },
		{ "Tomato"                  , new Color(0xFF6347) },
		{ "Turquoise"               , new Color(0x40E0D0) },
		{ "Violet"                  , new Color(0xEE82EE) },
		{ "Wheat"                   , new Color(0xF5DEB3) },
		{ "White"                   , new Color(0xFFFFFF) },
		{ "WhiteSmoke"              , new Color(0xF5F5F5) },
		{ "Yellow"                  , new Color(0xFFFF00) },
		{ "YellowGreen"             , new Color(0x9ACD32) },
		{ "GwenPink"                , new Color(255, 65, 199) },
		{ "Transparent"             , new Color(0, 255, 255, 255) }
	};
}