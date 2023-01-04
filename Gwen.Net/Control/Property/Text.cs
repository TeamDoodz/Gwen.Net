using System;

namespace Gwen.Net.Control.Property
{
    /// <summary>
    /// Text property.
    /// </summary>
    public class Text : PropertyBase
    {
        protected readonly TextBox textBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Text(Control.ControlBase parent)
            : base(parent)
        {
            textBox = new TextBox(this);
            textBox.Dock = Dock.Fill;
            textBox.Padding = Padding.Zero;
            textBox.ShouldDrawBackground = false;
            textBox.TextChanged += OnValueChanged;
        }

        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get { return textBox.Text; }
            set { base.Value = value; }
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void SetValue(string value, bool fireEvents = false)
        {
            textBox.SetText(value, fireEvents);
        }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing
        {
            get { return textBox.HasFocus; }
        }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get { return base.IsHovered | textBox.IsHovered; }
        }

        /*
		protected override Size Measure(Size availableSize)
		{
			return m_TextBox.DoMeasure(availableSize);
		}

		protected override Size Arrange(Size finalSize)
		{
			m_TextBox.DoArrange(new Rectangle(0, 0, finalSize.Width, m_TextBox.MeasuredSize.Height));

			return new Size(finalSize.Width, m_TextBox.MeasuredSize.Height);
		}
		*/
    }
}