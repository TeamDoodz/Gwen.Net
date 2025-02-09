﻿
namespace Gwen.Net.Skin.Texturing;

public struct SubRect {
	public float[] uv;
}

/// <summary>
/// 3x3 texture grid.
/// </summary>
public struct Bordered {
	private Texture? texture;

	private readonly SubRect[] rects;

	private Margin margin;

	private float width;
	private float height;

	public Bordered(Texture texture, float x, float y, float w, float h, Margin inMargin, float drawMarginScale = 1.0f)
		: this() {
		rects = new SubRect[9];
		for(int i = 0; i < rects.Length; i++) {
			rects[i].uv = new float[4];
		}

		Init(texture, x, y, w, h, inMargin, drawMarginScale);
	}

	void DrawRect(Renderer.RendererBase render, int i, int x, int y, int w, int h) {
		if(texture == null) {
			throw new System.Exception("Texture is null. This shouldn't happen!");
		}

		render.DrawTexturedRect(texture,
								new Rectangle(x, y, w, h),
								rects[i].uv[0], rects[i].uv[1], rects[i].uv[2], rects[i].uv[3]);
	}

	void SetRect(int num, float x, float y, float w, float h) {
		if(texture == null) {
			throw new System.Exception("Texture is null. This shouldn't happen!");
		}

		float texw = texture.Width;
		float texh = texture.Height;

		//x -= 1.0f;
		//y -= 1.0f;

		rects[num].uv[0] = x / texw;
		rects[num].uv[1] = y / texh;

		rects[num].uv[2] = (x + w) / texw;
		rects[num].uv[3] = (y + h) / texh;

		//	rects[num].uv[0] += 1.0f / m_Texture->width;
		//	rects[num].uv[1] += 1.0f / m_Texture->width;
	}

	private void Init(Texture texture, float x, float y, float w, float h, Margin inMargin, float drawMarginScale = 1.0f) {
		this.texture = texture;

		margin = inMargin;

		SetRect(0, x, y, margin.Left, margin.Top);
		SetRect(1, x + margin.Left, y, w - margin.Left - margin.Right, margin.Top);
		SetRect(2, (x + w) - margin.Right, y, margin.Right, margin.Top);

		SetRect(3, x, y + margin.Top, margin.Left, h - margin.Top - margin.Bottom);
		SetRect(4, x + margin.Left, y + margin.Top, w - margin.Left - margin.Right,
				h - margin.Top - margin.Bottom);
		SetRect(5, (x + w) - margin.Right, y + margin.Top, margin.Right, h - margin.Top - margin.Bottom - 1);

		SetRect(6, x, (y + h) - margin.Bottom, margin.Left, margin.Bottom);
		SetRect(7, x + margin.Left, (y + h) - margin.Bottom, w - margin.Left - margin.Right, margin.Bottom);
		SetRect(8, (x + w) - margin.Right, (y + h) - margin.Bottom, margin.Right, margin.Bottom);

		margin.Left = (int)(margin.Left * drawMarginScale);
		margin.Right = (int)(margin.Right * drawMarginScale);
		margin.Top = (int)(margin.Top * drawMarginScale);
		margin.Bottom = (int)(margin.Bottom * drawMarginScale);

		width = w - x;
		height = h - y;
	}

	// can't have this as default param
	public void Draw(Renderer.RendererBase render, Rectangle r) {
		Draw(render, r, Color.White);
	}

	public void Draw(Renderer.RendererBase render, Rectangle r, Color col) {
		if(texture == null)
			return;

		render.DrawColor = col;

		if(r.Width < width && r.Height < height) {
			render.DrawTexturedRect(texture, r, rects[0].uv[0], rects[0].uv[1], rects[8].uv[2], rects[8].uv[3]);
			return;
		}

		DrawRect(render, 0, r.X, r.Y, margin.Left, margin.Top);
		DrawRect(render, 1, r.X + margin.Left, r.Y, r.Width - margin.Left - margin.Right, margin.Top);
		DrawRect(render, 2, (r.X + r.Width) - margin.Right, r.Y, margin.Right, margin.Top);

		DrawRect(render, 3, r.X, r.Y + margin.Top, margin.Left, r.Height - margin.Top - margin.Bottom);
		DrawRect(render, 4, r.X + margin.Left, r.Y + margin.Top, r.Width - margin.Left - margin.Right,
				 r.Height - margin.Top - margin.Bottom);
		DrawRect(render, 5, (r.X + r.Width) - margin.Right, r.Y + margin.Top, margin.Right,
				 r.Height - margin.Top - margin.Bottom);

		DrawRect(render, 6, r.X, (r.Y + r.Height) - margin.Bottom, margin.Left, margin.Bottom);
		DrawRect(render, 7, r.X + margin.Left, (r.Y + r.Height) - margin.Bottom,
				 r.Width - margin.Left - margin.Right, margin.Bottom);
		DrawRect(render, 8, (r.X + r.Width) - margin.Right, (r.Y + r.Height) - margin.Bottom, margin.Right,
				 margin.Bottom);
	}
}