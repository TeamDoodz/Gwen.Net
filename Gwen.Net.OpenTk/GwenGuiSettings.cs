using System;
using System.IO;

namespace Gwen.Net.OpenTk;

public class GwenGuiSettings {
	public static GwenGuiSettings Default => new GwenGuiSettings {
		DefaultFont = "Calibri",
		Renderer = GwenGuiRenderer.GL40,
		DrawBackground = true
	};

	//Make this a source or stream?
	public FileInfo SkinFile { get; set; }

	public string DefaultFont { get; set; }

	public GwenGuiRenderer Renderer { get; set; }

	public bool DrawBackground { get; set; }

	/// <summary>
	/// Modifies this instance using the specified delegate.
	/// </summary>
	/// <returns>The same instance that this method was called on.</returns>
	public GwenGuiSettings Modify(Action<GwenGuiSettings> settingsModifier) {
		settingsModifier?.Invoke(this);
		return this;
	}

	private GwenGuiSettings() { }
}
