using OpenTK.Windowing.Desktop;

namespace Gwen.Net.OpenTk;

public static class GwenGuiFactory {
	public static IGwenGui CreateFromGame(GameWindow window, GwenGuiSettings settings) {
		return new GwenGui(window, settings);
	}
}
