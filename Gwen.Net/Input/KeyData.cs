using Gwen.Net.Control;

namespace Gwen.Net.Input;

/// <summary>
/// Keyboard state.
/// </summary>
public class KeyData {
	public bool[] KeyState { get; }
	public float[] NextRepeat { get; }
	public ControlBase? Target { get; set; }
	public bool LeftMouseDown { get; set; }
	public bool RightMouseDown { get; set; }

	public KeyData() {
		KeyState = new bool[(int)GwenMappedKey.Count];
		NextRepeat = new float[(int)GwenMappedKey.Count];
		// everything is initialized to 0 by default
	}
}