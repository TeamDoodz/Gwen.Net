using Gwen.Net.Control;

namespace Gwen.Net.DragDrop;

public class Package {
	public string Name { get; set; } = "Unnamed Package";
	public object? UserData { get; set; }
	public bool IsDraggable { get; set; }
	public ControlBase? DrawControl { get; set; }
	public Point HoldOffset { get; set; }
}