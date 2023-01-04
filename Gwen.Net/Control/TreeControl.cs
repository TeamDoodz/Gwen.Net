using System;
using System.Collections.Generic;
using System.Linq;

namespace Gwen.Net.Control;
/// <summary>
/// Tree control.
/// </summary>
[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
public class TreeControl : ScrollControl {
	private readonly TreeNode rootNode;
	private bool multiSelect;

	/// <summary>
	/// List of selected nodes.
	/// </summary>
	public IEnumerable<TreeNode> SelectedNodes {
		get {
			List<TreeNode> selectedNodes = new List<TreeNode>();

			foreach(ControlBase child in rootNode.Children) {
				if(child is not TreeNode node)
					continue;
				selectedNodes.AddRange(node.SelectedChildren);
			}

			return selectedNodes;
		}
	}

	/// <summary>
	/// First selected node (and only if nodes are not multiselectable).
	/// </summary>
	public TreeNode? SelectedNode {
		get {
			if(SelectedNodes.Any())
				return SelectedNodes.First();
			else
				return null;
		}
	}

	/// <summary>
	/// Determines if multiple nodes can be selected at the same time.
	/// </summary>
	[Xml.XmlProperty]
	public bool AllowMultiSelect { get { return multiSelect; } set { multiSelect = value; } }

	/// <summary>
	/// Get the root node of the tree view. Root node is an invisible always expanded node that works
	/// as a parent node for all first tier nodes visible on the control.
	/// </summary>
	public TreeNode RootNode { get { return rootNode; } }

	/// <summary>
	/// Invoked when the node's selected state has changed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs> SelectionChanged {
		add {
			rootNode.SelectionChanged += value;
		}
		remove {
			rootNode.SelectionChanged -= value;
		}
	}

	/// <summary>
	/// Invoked when the node has been selected.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs> Selected {
		add {
			rootNode.Selected += value;
		}
		remove {
			rootNode.Selected -= value;
		}
	}

	/// <summary>
	/// Invoked when the node has been unselected.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs> Unselected {
		add {
			rootNode.Unselected += value;
		}
		remove {
			rootNode.Unselected -= value;
		}
	}

	/// <summary>
	/// Invoked when the node has been double clicked and contains no child nodes.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs> NodeDoubleClicked {
		add {
			rootNode.NodeDoubleClicked += value;
		}
		remove {
			rootNode.NodeDoubleClicked -= value;
		}
	}

	/// <summary>
	/// Invoked when the node has been expanded.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs> Expanded {
		add {
			rootNode.Expanded += value;
		}
		remove {
			rootNode.Expanded -= value;
		}
	}

	/// <summary>
	/// Invoked when the node has been collapsed.
	/// </summary>
	[Xml.XmlEvent]
	public event GwenEventHandler<EventArgs> Collapsed {
		add {
			rootNode.Collapsed += value;
		}
		remove {
			rootNode.Collapsed -= value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeControl"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public TreeControl(ControlBase? parent)
		: base(parent) {
		Padding = Padding.One;

		MouseInputEnabled = true;
		EnableScroll(true, true);
		AutoHideBars = true;

		multiSelect = false;

		rootNode = new TreeNode(this);
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		if(ShouldDrawBackground)
			skin.DrawTreeControl(this);
	}

	/// <summary>
	/// Adds a new child node.
	/// </summary>
	/// <param name="label">Node's label.</param>
	/// <returns>Newly created control.</returns>
	public TreeNode AddNode(string label, string name = "", object? userData = null) {
		return rootNode.AddNode(label, name, userData);
	}

	/// <summary>
	/// Removes all child nodes.
	/// </summary>
	public virtual void RemoveAll() {
		rootNode.DeleteAllChildren();
	}

	/// <summary>
	/// Remove node and all of it's child nodes.
	/// </summary>
	/// <param name="node">Node to remove.</param>
	public void RemoveNode(TreeNode node) {
		if(node == null)
			return;

		rootNode.RemoveNode(node);
	}

	/// <summary>
	/// Remove all nodes.
	/// </summary>
	public void RemoveAllNodes() {
		rootNode.RemoveAllNodes();
	}

	/// <summary>
	/// Opens the node and all child nodes.
	/// </summary>
	public void ExpandAll() {
		rootNode.ExpandAll();
	}

	/// <summary>
	/// Clears the selection on the node and all child nodes.
	/// </summary>
	public void UnselectAll() {
		rootNode.UnselectAll();
	}

	/// <summary>
	/// Find a node bu user data.
	/// </summary>
	/// <param name="userData">Node user data.</param>
	/// <param name="recursive">Determines whether the search should be recursive.</param>
	/// <returns>Found node or null.</returns>
	public TreeNode? FindNodeByUserData(object? userData, bool recursive = true) {
		return rootNode.FindNodeByUserData(userData, recursive);
	}

	/// <summary>
	/// Find a node by name.
	/// </summary>
	/// <param name="name">Node name</param>
	/// <param name="recursive">Determines whether the search should be recursive.</param>
	/// <returns>Found node or null.</returns>
	public TreeNode? FindNodeByName(string name, bool recursive = true) {
		return rootNode.FindNodeByName(name, recursive);
	}

	/// <summary>
	/// Handler for node added event.
	/// </summary>
	/// <param name="node">Node added.</param>
	public virtual void OnNodeAdded(TreeNode node) {
		node.LabelPressed += OnNodeSelected;
	}

	/// <summary>
	/// Handler for node selected event.
	/// </summary>
	/// <param name="Control">Node selected.</param>
	protected virtual void OnNodeSelected(ControlBase Control, EventArgs args) {
		if(!multiSelect /*|| InputHandler.InputHandler.IsKeyDown(Key.Control)*/)
			UnselectAll();
	}

	internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent) {
		TreeControl element = new TreeControl(parent);
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