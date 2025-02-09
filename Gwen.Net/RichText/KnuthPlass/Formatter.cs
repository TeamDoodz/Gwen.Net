﻿using System;
using System.Collections.Generic;

namespace Gwen.Net.RichText.KnuthPlass
{
    internal abstract class Formatter
    {
        protected Renderer.RendererBase m_Renderer;
        protected Font defaultFont;

        public Formatter(Renderer.RendererBase renderer, Font defaultFont)
        {
            m_Renderer = renderer;
            this.defaultFont = defaultFont;
        }

        public void MeasureText(Font font, string text, out int width, out int height)
        {
            Size p = m_Renderer.MeasureText(font, text);
            width = p.Width;
            height = p.Height;
        }

        public Size MeasureText(Font font, string text)
        {
            Size size = m_Renderer.MeasureText(font, text);

            return size;
        }

        public abstract List<Node> FormatParagraph(Paragraph paragraph);
    }
}