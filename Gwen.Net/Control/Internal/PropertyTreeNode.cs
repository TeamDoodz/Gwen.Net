using System;

namespace Gwen.Net.Control.Internal;

/// <summary>
/// Properties node.
/// </summary>
public class PropertyTreeNode : ContentControl {
	public const int TreeIndentation = 14;

	protected readonly PropertyTree propertyTree;
	protected readonly TreeToggleButton toggleButton;
	protected readonly TreeNodeLabel title;
	protected readonly Properties properties;

	public PropertyTree PropertyTree { get { return propertyTree; } }

	public Properties Properties { get { return properties; } }

	/// <summary>
	/// Node's label.
	/// </summary>
	public string Text { get { return title.Text; } set { title.Text = value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyTreeNode"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public PropertyTreeNode(ControlBase parent)
		: base(parent) {
		propertyTree = parent as PropertyTree ?? throw new ArgumentException("Parent control must be of type PropertyTree.", nameof(parent));

		toggleButton = new TreeToggleButton(this);
		toggleButton.Toggled += OnToggleButtonPress;

		title = new TreeNodeLabel(this);
		title.DoubleClicked += OnDoubleClickName;

		properties = new Properties(this);

		innerPanel = properties;

		title.TextColorOverride = Skin.Colors.Properties.Title;
	}

	protected override Size Measure(Size availableSize) {
		Size buttonSize = toggleButton.DoMeasure(availableSize);
		Size labelSize = title.DoMeasure(availableSize);
		Size innerSize = Size.Zero;

		if(innerPanel != null && !innerPanel.IsCollapsed)
			innerSize = innerPanel.DoMeasure(availableSize);

		return new Size(Math.Max(buttonSize.Width + labelSize.Width, TreeIndentation + innerSize.Width), Math.Max(buttonSize.Height, labelSize.Height) + innerSize.Height);
	}

	protected override Size Arrange(Size finalSize) {
		toggleButton.DoArrange(new Rectangle(0, (title.MeasuredSize.Height - toggleButton.MeasuredSize.Height) / 2, toggleButton.MeasuredSize.Width, toggleButton.MeasuredSize.Height));
		title.DoArrange(new Rectangle(toggleButton.MeasuredSize.Width, 0, finalSize.Width - toggleButton.MeasuredSize.Width, title.MeasuredSize.Height));

		if(innerPanel != null && !innerPanel.IsCollapsed)
			innerPanel.DoArrange(new Rectangle(TreeIndentation, Math.Max(toggleButton.MeasuredSize.Height, title.MeasuredSize.Height), finalSize.Width - TreeIndentation, innerPanel.MeasuredSize.Height));

		return new Size(finalSize.Width, MeasuredSize.Height);
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		if(innerPanel == null) {
			return;
		}
		skin.DrawPropertyTreeNode(this, innerPanel.ActualLeft, innerPanel.ActualTop);
	}

	/// <summary>
	/// Opens the node.
	/// </summary>
	public void Open() {
		innerPanel?.Show();
		if(toggleButton != null)
			toggleButton.ToggleState = true;

		Invalidate();
	}

	/// <summary>
	/// Closes the node.
	/// </summary>
	public void Close() {
		innerPanel?.Collapse();
		if(toggleButton != null)
			toggleButton.ToggleState = false;

		Invalidate();
	}

	/// <summary>
	/// Opens the node and all child nodes.
	/// </summary>
	public void Expand() {
		Open();
		foreach(ControlBase child in Children) {
			if(child is not TreeNode node)
				continue;
			node.ExpandAll();
		}
	}

	/// <summary>
	/// Handler for the toggle button.
	/// </summary>
	/// <param name="control">Event source.</param>
	/// <param name="args">Event args.</param>
	protected virtual void OnToggleButtonPress(ControlBase control, EventArgs args) {
		if(toggleButton.ToggleState) {
			Open();
		} else {
			Close();
		}
	}

	/// <summary>
	/// Handler for label double click.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnDoubleClickName(ControlBase control, EventArgs args) {
		if(!toggleButton.IsVisible)
			return;
		toggleButton.Toggle();
	}
}