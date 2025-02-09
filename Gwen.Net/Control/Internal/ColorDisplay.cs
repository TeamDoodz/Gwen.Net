﻿namespace Gwen.Net.Control.Internal;

/// <summary>
/// Color square.
/// </summary>
public class ColorDisplay : ControlBase {
	private Color color;
	//private bool m_DrawCheckers;

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorDisplay"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public ColorDisplay(ControlBase parent) : base(parent) {
		Size = new Size(BaseUnit * 2);
		color = new Color(255, 255, 0, 0);
		//m_DrawCheckers = true;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		skin.DrawColorDisplay(this, color);
	}

	/// <summary>
	/// Current color.
	/// </summary>
	public Color Color { get { return color; } set { color = value; } }
	//public bool DrawCheckers { get { return m_DrawCheckers; } set { m_DrawCheckers = value; } }
	public int R { get { return color.R; } set { color = new Color(color.A, value, color.G, color.B); } }
	public int G { get { return color.G; } set { color = new Color(color.A, color.R, value, color.B); } }
	public int B { get { return color.B; } set { color = new Color(color.A, color.R, color.G, value); } }
	public int A { get { return color.A; } set { color = new Color(value, color.R, color.G, color.B); } }
}