using System;

namespace Gwen.Net.Control.Internal;

public class WindowTitleBar : Dragger {
	private readonly Label title;
	private readonly CloseButton closeButton;

	public Label Title { get { return title; } }
	public CloseButton CloseButton { get { return closeButton; } }

	public WindowTitleBar(ControlBase? parent)
		: base(parent) {
		title = new Label(this);
		title.Alignment = Alignment.Left | Alignment.CenterV;

		closeButton = new CloseButton(this, parent as Window ?? throw new ArgumentException("Parent control must be of type Window.", nameof(parent)));
		closeButton.IsTabable = false;
		closeButton.Name = "closeButton";

		Target = parent;
	}

	protected override Size Measure(Size availableSize) {
		title.DoMeasure(availableSize);

		if(!closeButton.IsCollapsed)
			closeButton.DoMeasure(availableSize);

		return availableSize;
	}

	protected override Size Arrange(Size finalSize) {
		title.DoArrange(new Rectangle(8, 0, title.MeasuredSize.Width, finalSize.Height));

		if(!closeButton.IsCollapsed) {
			int closeButtonSize = finalSize.Height;
			closeButton.DoArrange(new Rectangle(finalSize.Width - 6 - closeButtonSize, 0, closeButtonSize, closeButtonSize));
		}

		return finalSize;
	}
}