using System;
using System.Collections.Generic;
using System.Linq;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;
/// <summary>
/// Tree control node.
/// </summary>
[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
public class TreeNode : ContentControl {
	private TreeControl? treeControl;
	protected TreeToggleButton? toggleButton;
	protected TreeNodeLabel? title;
	private bool root = false;

	private bool selected;
	private bool selectable;

	/// <summary>
	/// Root node of the tree view.
	/// </summary>
	private TreeNode? RootNode { get { return treeControl?.RootNode; } }

	/// <summary>
	/// Parent tree control.
	/// </summary>
	public TreeControl? TreeControl { get { return treeControl; } }

	/// <summary>
	/// Indicates whether this is a root node.
	/// </summary>
	public bool IsRoot { get { return root; } set { root = value; } }

	/// <summary>
	/// Determines whether the node is selectable.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsSelectable { get { return selectable; } set { selectable = value; } }

	public int NodeCount { get { return Children.Count; } }

	/// <summary>
	/// Indicates whether the node is selected.
	/// </summary>
	[Xml.XmlProperty]
	public bool IsSelected {
		get { return selected; }
		set {
			if(!IsSelectable)
				return;
			if(IsSelected == value)
				return;

			if(value && TreeControl != null && !TreeControl.AllowMultiSelect)
				RootNode?.UnselectAll();

			selected = value;

			if(title != null)
				title.ToggleState = value;

			if(SelectionChanged != null)
				SelectionChanged.Invoke(this, EventArgs.Empty);

			// propagate to root parent (tree)
			if(RootNode != null && RootNode.SelectionChanged != null)
				RootNode.SelectionChanged.Invoke(this, EventArgs.Empty);

			if(value) {
				if(Selected != null)
					Selected.Invoke(this, EventArgs.Empty);

				if(RootNode != null && RootNode.Selected != null)
					RootNode.Selected.Invoke(this, EventArgs.Empty);
			} else {
				if(Unselected != null)
					Unselected.Invoke(this, EventArgs.Empty);

				if(RootNode != null && RootNode.Unselected != null)
					RootNode.Unselected.Invoke(this, EventArgs.Empty);
			}
		}
	}

	/// <summary>
	/// Node's label.
	/// </summary>
	[Xml.XmlProperty]
	public string Text { 
		get => title?.Text ?? "";
		set {
			if(title != null) {
				title.Text = value;
			}
		} 
	}

	/// <summary>
	/// List of selected nodes.
	/// </summary>
	public IEnumerable<TreeNode> SelectedChildren {
		get {
			List<TreeNode> Trees = new List<TreeNode>();

			foreach(ControlBase child in Children) {
				if(child is not TreeNode node)
					continue;
				Trees.AddRange(node.SelectedChildren);
			}

			if(this.IsSelected) {
				Trees.Add(this);
			}

			return Trees;
		}
	}

	/// <summary>
	/// Invoked when the node label has been pressed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? LabelPressed;

	/// <summary>
	/// Invoked when the node's selected state has changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? SelectionChanged;

	/// <summary>
	/// Invoked when the node has been selected.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Selected;

	/// <summary>
	/// Invoked when the node has been double clicked and contains no child nodes.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? NodeDoubleClicked;

	/// <summary>
	/// Invoked when the node has been unselected.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Unselected;

	/// <summary>
	/// Invoked when the node has been expanded.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Expanded;

	/// <summary>
	/// Invoked when the node has been collapsed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs>? Collapsed;

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeNode"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public TreeNode(ControlBase? parent)
		: base(parent) {
		// Make sure that the tree control has only one root node.
		if(treeControl == null && parent is TreeControl parentTree) {
			treeControl = parentTree;
			root = true;
		} else {
			toggleButton = new TreeToggleButton(this);
			toggleButton.Toggled += OnToggleButtonPress;

			title = new TreeNodeLabel(this);
			title.DoubleClicked += OnDoubleClickName;
			title.Clicked += OnClickName;
		}

		innerPanel = new Layout.VerticalLayout(this);
		innerPanel.Collapse(!root, false); // Root node is always expanded

		selected = false;
		selectable = true;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		if(!root && innerPanel != null) {
			int bottom = 0;
			if(innerPanel.Children.Count > 0) {
				bottom = innerPanel.Children.Last().ActualTop + innerPanel.ActualTop;
			}

			skin.DrawTreeNode(this, innerPanel.IsVisible, IsSelected, title?.ActualHeight ?? 0, title?.ActualWidth ?? 0, (int)(toggleButton?.ActualTop ?? 0 + toggleButton?.ActualHeight ?? 0 * 0.5f), bottom, RootNode == Parent, toggleButton?.ActualWidth ?? 0);
		}
	}

	protected override Size Measure(Size availableSize) {
		if(!root && innerPanel != null) {
			Size buttonSize = toggleButton?.DoMeasure(availableSize) ?? Size.One;
			Size labelSize = title?.DoMeasure(availableSize) ?? Size.One;
			Size innerSize = Size.Zero;

			if(innerPanel.Children.Count == 0) {
				if(toggleButton != null) {
					toggleButton.Hide();
					toggleButton.ToggleState = false;
				}
				innerPanel.Collapse(true, false);
			} else {
				toggleButton?.Show();
				if(!innerPanel.IsCollapsed)
					innerSize = innerPanel.DoMeasure(availableSize);
			}

			return new Size(Math.Max(buttonSize.Width + labelSize.Width, toggleButton?.MeasuredSize.Width ?? 0 + innerSize.Width), Math.Max(buttonSize.Height, labelSize.Height) + innerSize.Height) + Padding;
		} else {
			return innerPanel?.DoMeasure(availableSize) ?? Size.One + Padding;
		}
	}

	protected override Size Arrange(Size finalSize) {
		if(!root) {
			toggleButton?.DoArrange(new Rectangle(Padding.Left, Padding.Top + (title?.MeasuredSize.Height ?? 0 - toggleButton.MeasuredSize.Height) / 2, toggleButton.MeasuredSize.Width, toggleButton.MeasuredSize.Height));
			title?.DoArrange(new Rectangle(Padding.Left + toggleButton?.MeasuredSize.Width ?? 0, Padding.Top, title.MeasuredSize.Width, title.MeasuredSize.Height));

			if(innerPanel != null && !innerPanel.IsCollapsed)
				innerPanel.DoArrange(new Rectangle(Padding.Left + toggleButton?.MeasuredSize.Width ?? 1, Padding.Top + Math.Max(toggleButton?.MeasuredSize.Height ?? 1, title?.MeasuredSize.Height ?? 1), innerPanel.MeasuredSize.Width, innerPanel.MeasuredSize.Height));
		} else {
			innerPanel?.DoArrange(new Rectangle(Padding.Left, Padding.Top, innerPanel.MeasuredSize.Width, innerPanel.MeasuredSize.Height));
		}

		return MeasuredSize;
	}

	/// <summary>
	/// Adds a new child node.
	/// </summary>
	/// <param name="label">Node's label.</param>
	/// <returns>Newly created control.</returns>
	public TreeNode AddNode(string label, string name = "", object? userData = null) {
		TreeNode node = new(this) {
			Text = label,
			Name = name,
			UserData = userData
		};

		return node;
	}

	public TreeNode InsertNode(int index, string label, string name = "", object? userData = null) {
		TreeNode node = AddNode(label, name, userData);
		if(index == 0)
			node.SendToBack();
		else if(index < Children.Count)
			node.BringNextToControl(Children[index], false);

		return node;
	}

	/// <summary>
	/// Remove node and all of it's child nodes.
	/// </summary>
	/// <param name="node">Node to remove.</param>
	public void RemoveNode(TreeNode node) {
		if(node == null)
			return;

		node.RemoveAllNodes();

		RemoveChild(node, true);

		Invalidate();
	}

	/// <summary>
	/// Remove all nodes.
	/// </summary>
	public void RemoveAllNodes() {
		while(NodeCount > 0) {
			if(Children[0] is not TreeNode node)
				continue;

			RemoveNode(node);
		}

		Invalidate();
	}

	/// <summary>
	/// Opens the node.
	/// </summary>
	public void Open() {
		innerPanel?.Show();
		if(toggleButton != null)
			toggleButton.ToggleState = true;

		if(Expanded != null)
			Expanded.Invoke(this, EventArgs.Empty);
		if(RootNode != null && RootNode.Expanded != null)
			RootNode.Expanded.Invoke(this, EventArgs.Empty);

		Invalidate();
	}

	/// <summary>
	/// Closes the node.
	/// </summary>
	public void Close() {
		innerPanel?.Collapse();
		if(toggleButton != null)
			toggleButton.ToggleState = false;

		if(Collapsed != null)
			Collapsed.Invoke(this, EventArgs.Empty);
		if(RootNode != null && RootNode.Collapsed != null)
			RootNode.Collapsed.Invoke(this, EventArgs.Empty);

		Invalidate();
	}

	/// <summary>
	/// Opens the node and all child nodes.
	/// </summary>
	public void ExpandAll() {
		Open();
		foreach(ControlBase child in Children) {
			if(child is not TreeNode node)
				continue;
			node.ExpandAll();
		}
	}

	/// <summary>
	/// Clears the selection on the node and all child nodes.
	/// </summary>
	public void UnselectAll() {
		IsSelected = false;
		if(title != null)
			title.ToggleState = false;

		foreach(ControlBase child in Children) {
			if(child is not TreeNode node)
				continue;
			node.UnselectAll();
		}
	}

	/// <summary>
	/// Find a node bu user data.
	/// </summary>
	/// <param name="userData">Node user data.</param>
	/// <param name="recursive">Determines whether the search should be recursive.</param>
	/// <returns>Found node or null.</returns>
	public TreeNode? FindNodeByUserData(object? userData, bool recursive = true) {
		TreeNode? node = this.Children.Where(x => x is TreeNode && x.UserData == userData).FirstOrDefault() as TreeNode;
		if(node != null)
			return node;

		if(recursive) {
			foreach(ControlBase child in this.Children) {
				node = child as TreeNode;
				if(node != null) {
					node = node.FindNodeByUserData(userData, true);
					if(node != null)
						return node;
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Find a node by name.
	/// </summary>
	/// <param name="name">Node name</param>
	/// <param name="recursive">Determines whether the search should be recursive.</param>
	/// <returns>Found node or null.</returns>
	public TreeNode? FindNodeByName(string name, bool recursive = true) {
		return FindChildByName(name, recursive) as TreeNode;
	}

	/// <summary>
	/// Handler for the toggle button.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnToggleButtonPress(ControlBase control, EventArgs args) {
		if(toggleButton != null && toggleButton.ToggleState) {
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
		if(toggleButton == null) {
			return;
		}

		if(!toggleButton.IsVisible) {
			// Invoke double click events only if node hasn't child nodes.
			// Otherwise toggle expand/collapse.
			if(NodeDoubleClicked != null)
				NodeDoubleClicked.Invoke(this, EventArgs.Empty);

			if(RootNode != null && RootNode.NodeDoubleClicked != null)
				RootNode.NodeDoubleClicked.Invoke(this, EventArgs.Empty);

			return;
		}

		toggleButton.Toggle();
	}

	/// <summary>
	/// Handler for label click.
	/// </summary>
	/// <param name="control">Event source.</param>
	protected virtual void OnClickName(ControlBase control, EventArgs args) {
		if(LabelPressed != null)
			LabelPressed.Invoke(this, EventArgs.Empty);
		IsSelected = !IsSelected;
	}

	public void SetImage(string textureName) {
		title?.SetImage(textureName);
	}

	protected override void OnChildAdded(ControlBase child) {
		if(child is TreeNode node) {
			node.treeControl = treeControl;

			treeControl?.OnNodeAdded(node);
		}

		base.OnChildAdded(child);
	}

	[Xml.XmlEvent]
	public override event GwenEventHandler<ClickedEventArgs>? Clicked {
		add {
			if(value == null || title == null) {
				return;
			}
			title.Clicked += delegate (ControlBase sender, ClickedEventArgs args) { value(this, args); };
		}
		remove {
			if(value == null || title == null) {
				return;
			}
			title.Clicked -= delegate (ControlBase sender, ClickedEventArgs args) { value(this, args); };
		}
	}

	[Xml.XmlEvent]
	public override event GwenEventHandler<ClickedEventArgs>? DoubleClicked {
		add {
			if(value != null && title != null) {
				title.DoubleClicked += delegate (ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
		}
		remove {
			if(value == null || title == null) {
				return;
			}
			title.DoubleClicked -= delegate (ControlBase sender, ClickedEventArgs args) { value(this, args); };
		}
	}

	[Xml.XmlEvent]
	public override event GwenEventHandler<ClickedEventArgs>? RightClicked {
		add {
			if(value == null || title == null) {
				return;
			}
			title.RightClicked += delegate (ControlBase sender, ClickedEventArgs args) { value(this, args); };
		}
		remove {
			if(value == null || title == null) {
				return;
			}
			title.RightClicked -= delegate (ControlBase sender, ClickedEventArgs args) { value(this, args); };
		}
	}

	[Xml.XmlEvent]
	public override event GwenEventHandler<ClickedEventArgs>? DoubleRightClicked {
		add {
			if(value != null && title != null) {
				title.DoubleRightClicked += delegate (ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
		}
		remove {
			if(value == null || title == null) {
				return;
			}
			title.DoubleRightClicked -= delegate (ControlBase sender, ClickedEventArgs args) { value(this, args); };
		}
	}

	internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent) {
		TreeNode element = new TreeNode(parent);
		parser.ParseAttributes(element);
		if(parser.MoveToContent()) {
			foreach(string elementName in parser.NextElement()) {
				if(elementName == "TreeNode") {
					parser.ParseElement<TreeNode>(element);
				}
			}
		}

		return element;
	}
}