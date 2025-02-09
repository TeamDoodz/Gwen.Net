﻿using System;
using Gwen.Net.Control;

namespace Gwen.Net;

public enum Dock {
	None = ControlBase.InternalFlags.DockNone,
	Left = ControlBase.InternalFlags.DockLeft,
	Top = ControlBase.InternalFlags.DockTop,
	Right = ControlBase.InternalFlags.DockRight,
	Bottom = ControlBase.InternalFlags.DockBottom,
	Fill = ControlBase.InternalFlags.DockFill,
}