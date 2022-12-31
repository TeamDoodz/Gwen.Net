using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gwen.Net;

/// <summary>
/// Font style.
/// </summary>
[Flags]
public enum FontStyle {
	Normal = 0,
	Bold = (1 << 0),
	Italic = (1 << 1),
	Underline = (1 << 2),
	Strikeout = (1 << 3)
}
