using System;
using Gwen.Net.Control.Internal;

namespace Gwen.Net.Control;

/// <summary>
/// Single property row.
/// </summary>
public class PropertyRow : ControlBase {
	private readonly Label label;
	private readonly Property.PropertyBase? property;
	private bool lastEditing;
	private bool lastHover;

	/// <summary>
	/// Invoked when the property value has changed.
	/// </summary>
	public event GwenEventHandler<EventArgs>? ValueChanged;

	/// <summary>
	/// Indicates whether the property value is being edited.
	/// </summary>
	public bool IsEditing => property != null && property.IsEditing;

	/// <summary>
	/// Property value.
	/// </summary>
	public string Value { 
		get => property?.Value ?? "";
		set {
			if(property != null) {
				property.Value = value;
			}
		} 
	}

	/// <summary>
	/// Indicates whether the control is hovered by mouse pointer.
	/// </summary>
	public override bool IsHovered {
		get {
			return base.IsHovered || (property != null && property.IsHovered);
		}
	}

	/// <summary>
	/// Property name.
	/// </summary>
	public string Label { get { return label.Text; } set { label.Text = value; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyRow"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	/// <param name="prop">Property control associated with this row.</param>
	public PropertyRow(ControlBase? parent, Property.PropertyBase prop)
		: base(parent) {
		Padding = new Padding(2, 2, 2, 2);

		label = new PropertyRowLabel(this);
		label.Alignment = Alignment.Left | Alignment.Top;

		property = prop;
		property.Parent = this;
		property.ValueChanged += OnValueChanged;
	}

	/// <summary>
	/// Renders the control using specified skin.
	/// </summary>
	/// <param name="skin">Skin to use.</param>
	protected override void Render(Skin.SkinBase skin) {
		/* SORRY */
		if(IsEditing != lastEditing) {
			OnEditingChanged();
			lastEditing = IsEditing;
		}

		if(IsHovered != lastHover) {
			OnHoverChanged();
			lastHover = IsHovered;
		}
		/* SORRY */

		skin.DrawPropertyRow(this, label.ActualRight, IsEditing, IsHovered | property?.IsHovered ?? false);
	}

	protected override Size Measure(Size availableSize) {
		if(Parent is Properties parent) {
			Size labelSize = label.DoMeasure(new Size(parent.LabelWidth - Padding.Left - Padding.Right, availableSize.Height)) + Padding;
			Size propertySize = property?.DoMeasure(new Size(availableSize.Width - parent.LabelWidth, availableSize.Height)) ?? Size.One + Padding;

			return new Size(labelSize.Width + propertySize.Width, Math.Max(labelSize.Height, propertySize.Height));
		}

		return Size.Zero;
	}

	protected override Size Arrange(Size finalSize) {
		if(Parent is Properties parent) {
			label.DoArrange(new Rectangle(Padding.Left, Padding.Top, parent.LabelWidth - Padding.Left - Padding.Right, label.MeasuredSize.Height));
			property?.DoArrange(new Rectangle(parent.LabelWidth + Padding.Left, Padding.Top, finalSize.Width - parent.LabelWidth - Padding.Left - Padding.Right, property.MeasuredSize.Height));

			return new Size(finalSize.Width, Math.Max(label.MeasuredSize.Height, property?.MeasuredSize.Height ?? 1) + Padding.Top + Padding.Bottom);
		}

		return Size.Zero;
	}

	protected virtual void OnValueChanged(ControlBase control, EventArgs args) {
		if(ValueChanged != null)
			ValueChanged.Invoke(this, EventArgs.Empty);
	}

	private void OnEditingChanged() {
		label.Redraw();
	}

	private void OnHoverChanged() {
		label.Redraw();
	}
}