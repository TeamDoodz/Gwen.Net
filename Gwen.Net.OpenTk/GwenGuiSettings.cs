using System;
using System.IO;

namespace Gwen.Net.OpenTk;

public class GwenGuiSettings {
	//Make this a source or stream?
	public FileInfo SkinFile { get; set; }
	public string DefaultFont { get; set; } = "Calibri";
	public GwenGuiRenderer Renderer { get; set; } = GwenGuiRenderer.GL40;
	public bool DrawBackground { get; set; } = true;

	public GwenGuiSettings(FileInfo skinFile) {
		SkinFile = skinFile;
	}
	public GwenGuiSettings(string skinFilePath) {
		SkinFile = new FileInfo(skinFilePath);
	}
}
