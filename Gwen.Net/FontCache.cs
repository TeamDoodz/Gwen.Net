using System;
using System.Collections.Generic;

namespace Gwen.Net;

public class FontCache : IDisposable {
	public static Font GetFont(Renderer.RendererBase renderer, string faceName, int size = 10, FontStyle style = 0) {
		if (instance == null)
			instance = new FontCache();

		return instance.InternalGetFont(renderer, faceName, size, style);
	}

	public static void FreeCache() {
		if (instance != null)
			instance.Dispose();
	}

	private Font InternalGetFont(Renderer.RendererBase renderer, string faceName, int size, FontStyle style) {
		string id = String.Format("{0};{1};{2}", faceName, size, (int)style);

		if (!cache.TryGetValue(id, out Font? font)) {
			font = new Font(renderer, faceName, size);

			if ((style & FontStyle.Bold) != 0)
				font.Bold = true;
			if ((style & FontStyle.Italic) != 0)
				font.Italic = true;
			if ((style & FontStyle.Underline) != 0)
				font.Underline = true;
			if ((style & FontStyle.Strikeout) != 0)
				font.Strikeout = true;

			cache[id] = font;
		}

		return font;
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
		instance = null;
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			foreach (var font in cache) {
				font.Value.Dispose();
			}

			cache.Clear();
		}
	}

	private static FontCache? instance = null;

	private readonly Dictionary<string, Font> cache = new Dictionary<string, Font>();
}