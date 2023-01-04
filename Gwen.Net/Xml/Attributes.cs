using System;

namespace Gwen.Net.Xml;

/// <summary>
/// Attribute to indicate that a property is usable from XML.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class XmlPropertyAttribute : Attribute {
}

/// <summary>
/// Attribute to indicate that a event is usable from XML.
/// </summary>
[AttributeUsage(AttributeTargets.Event)]
public class XmlEventAttribute : Attribute {
}

/// <summary>
/// Attribute to indicate that a control is able to be created from XML.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class XmlControlAttribute : Attribute {
	/// <summary>
	/// Name of XML element. If this is null, the class name will be used instead.
	/// </summary>
	public string? ElementName { get; set; }

	/// <summary>
	/// Function name of the custom handler for the element creation. If this is null, no custom handler will be used.
	/// </summary>
	public string? CustomHandler { get; set; }
}