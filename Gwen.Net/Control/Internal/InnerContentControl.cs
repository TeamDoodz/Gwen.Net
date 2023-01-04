using System;
using Gwen.Net.Control;

namespace Gwen.Net.Control.Internal
{
    public class InnerContentControl : ContentControl
    {
        public InnerContentControl(ControlBase parent)
            : base(parent)
        {
            MouseInputEnabled = false;
            KeyboardInputEnabled = false;
        }

        protected override void OnChildAdded(ControlBase child)
        {
            if (innerPanel == null)
                innerPanel = Children[0];

            base.OnChildAdded(child);
        }

        protected override Size Measure(Size availableSize)
        {
            if (innerPanel != null)
            {
                return innerPanel.DoMeasure(availableSize - Padding) + Padding;
            }

            return Size.Zero;
        }

        protected override Size Arrange(Size finalSize)
        {
            if (innerPanel != null)
                innerPanel.DoArrange(new Rectangle(Padding.Left, Padding.Top, finalSize - Padding));

            return finalSize;
        }

        public override ControlBase? FindChildByName(string name, bool recursive = false)
        {
            if (innerPanel != null && innerPanel.Name == name)
                return innerPanel;

            return base.FindChildByName(name, recursive);
        }
    }
}