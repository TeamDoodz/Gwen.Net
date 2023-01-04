//#define USE_KNUTH_PLASS_LINE_BREAKING

using System;
using System.Collections.Generic;
using Gwen.Net.Control.Internal;
using Gwen.Net.RichText;

namespace Gwen.Net.Control;

/// <summary>
/// Multiline label with text chunks having different color/font.
/// </summary>
[Xml.XmlControl]
public class RichLabel : ControlBase {
	private Document document;
	private bool needsRebuild;
	private int buildWidth;
	private Size textSize;
	private bool updating;

	public Document Document { get { return document; } set { document = value; needsRebuild = true; Invalidate(); } }

	[Xml.XmlEvent]
	public event GwenEventHandler<LinkClickedEventArgs>? LinkClicked;

	/// <summary>
	/// Initializes a new instance of the <see cref="RichLabel"/> class.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public RichLabel(ControlBase? parent)
		: base(parent) {
		buildWidth = 0;
		textSize = Size.Zero;
		updating = false;
		document = new();
	}

	protected void Rebuild() {
		updating = true;

		DeleteAllChildren();

		Size size = Size.Zero;

		if(document != null && document.Paragraphs.Count > 0) {
#if USE_KNUTH_PLASS_LINE_BREAKING
				LineBreaker lineBreaker = new RichText.KnuthPlass.LineBreaker(Skin.Renderer, Skin.DefaultFont);
#else
			LineBreaker lineBreaker = new RichText.Simple.LineBreaker(Skin.Renderer, Skin.DefaultFont);
#endif

			int y = 0;
			int x;
			int width;
			int height;

			foreach(Paragraph paragraph in document.Paragraphs) {
				if(paragraph is ImageParagraph imageParagraph) {

					ImagePanel image = new ImagePanel(this);
					image.ImageName = imageParagraph.ImageName ?? "";
					if(imageParagraph.ImageSize != null)
						image.Size = (Size)imageParagraph.ImageSize;
					if(imageParagraph.TextureRect != null)
						image.TextureRect = (Rectangle)imageParagraph.TextureRect;
					if(imageParagraph.ImageColor != null)
						image.ImageColor = (Color)imageParagraph.ImageColor;

					image.DoMeasure(Size.Infinity);
					image.DoArrange(paragraph.Margin.Left, y + paragraph.Margin.Top, image.MeasuredSize.Width, image.MeasuredSize.Height);

					size.Width = Math.Max(size.Width, image.MeasuredSize.Width + paragraph.Margin.Left + paragraph.Margin.Right);

					y += image.MeasuredSize.Height + paragraph.Margin.Top + paragraph.Margin.Bottom;
				} else {
					List<TextBlock> textBlocks = lineBreaker.LineBreak(paragraph, buildWidth);

					if(textBlocks != null) {
						x = paragraph.Margin.Left;
						y += paragraph.Margin.Top;
						width = 0;
						height = 0;

						foreach(TextBlock textBlock in textBlocks) {
							if(textBlock.Part is LinkPart linkPart) {
								LinkLabel link = new LinkLabel(this) {
									Text = textBlock.Text,
									Link = linkPart.Link,
									Font = linkPart.Font
								};
								link.LinkClicked += OnLinkClicked;
								if(linkPart.Color != null)
									link.TextColor = (Color)linkPart.Color;
								if(linkPart.HoverColor != null)
									link.HoverColor = (Color)linkPart.HoverColor;
								if(linkPart.HoverFont != null)
									link.HoverFont = linkPart.HoverFont;

								link.DoMeasure(Size.Infinity);
								link.DoArrange(new Rectangle(x + textBlock.Position.X, y + textBlock.Position.Y, textBlock.Size.Width, textBlock.Size.Height));

								width = Math.Max(width, link.ActualRight);
								height = Math.Max(height, link.ActualBottom);
							} else if(textBlock.Part is TextPart textPart) {
								Text text = new Text(this);
								text.Content = textBlock.Text;
								text.Font = textPart.Font;
								if(textPart.Color != null)
									text.TextColor = (Color)textPart.Color;

								text.DoMeasure(Size.Infinity);
								text.DoArrange(new Rectangle(x + textBlock.Position.X, y + textBlock.Position.Y, textBlock.Size.Width, textBlock.Size.Height));

								width = Math.Max(width, text.ActualRight + 1);
								height = Math.Max(height, text.ActualBottom + 1);
							}
						}

						size.Width = Math.Max(size.Width, width + paragraph.Margin.Right);

						y = height + paragraph.Margin.Bottom;
					}
				}
			}

			size.Height = y;
		}

		textSize = size;

		needsRebuild = false;

		updating = false;
	}

	protected override Size Measure(Size availableSize) {
		if(needsRebuild || availableSize.Width != buildWidth) {
			buildWidth = availableSize.Width;
			Rebuild();
		}

		return textSize;
	}

	protected override Size Arrange(Size finalSize) {
		if(needsRebuild || finalSize.Width != buildWidth) {
			buildWidth = finalSize.Width;
			Rebuild();
		}

		return textSize;
	}

	private void OnLinkClicked(ControlBase control, LinkClickedEventArgs args) {
		if(LinkClicked != null)
			LinkClicked(this, args);
	}

	public override void Invalidate() {
		// We don't want to cause the re-layout when creating text objects in the layout
		if(updating)
			return;

		base.Invalidate();
	}
}