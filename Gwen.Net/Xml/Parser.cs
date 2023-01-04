using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Gwen.Net.Control;

namespace Gwen.Net.Xml;

public delegate ControlBase ElementHandler(Parser parser, Type type, ControlBase? parent);
public delegate object AttributeValueConverter(object element, string value);
public delegate Delegate EventHandlerConverter(string attribute, string value);

/// <summary>
/// XML parser for creating controls and components using XML.
/// </summary>
public class Parser : IDisposable {
	const string READER_NULL_ERR = "The XML reader for this parser was disposed.";

	private static Dictionary<string, ElementDef> elementHandlers = new Dictionary<string, ElementDef>();

	private static Dictionary<Type, AttributeValueConverter> attributeValueConverters = new Dictionary<Type, AttributeValueConverter>();
	private static Dictionary<Type, EventHandlerConverter> eventHandlerConverters = new Dictionary<Type, EventHandlerConverter>();

	private XmlReader? reader;

	private ElementDef? currentElement;

	/// <summary>
	/// Current XML node name.
	/// </summary>
	public string Name { get { return reader?.Name ?? throw new InvalidOperationException(READER_NULL_ERR); } }

	public bool IsReaderNull => reader == null;

	static Parser() {
		XmlHelper.RegisterDefaultHandlers();

		Assembly assembly = typeof(Gwen.Net.Control.ControlBase).Assembly;
		if(assembly != null)
			ScanControls(assembly);
	}

	/// <summary>
	/// Register a XML element. All XML elements must be registered before usage. 
	/// </summary>
	/// <param name="name">Name of the element.</param>
	/// <param name="type">Type of the control or component.</param>
	/// <param name="handler">Handler function for creating the control or component.</param>
	/// <returns>True if registered successfully or false is already registered.</returns>
	public static bool RegisterElement(string name, Type type, ElementHandler handler) {
		if(!elementHandlers.ContainsKey(name)) {
			ElementDef elementDef = new ElementDef(type, handler);

			elementHandlers[name] = elementDef;

			ScanProperties(elementDef);
			ScanEvents(elementDef);

			return true;
		}

		return false;
	}

	/// <summary>
	/// Remove a XML element registration. After this the element is not usable anymore.
	/// </summary>
	/// <param name="name">Name of the element.</param>
	/// <returns>True if unregistered successfully.</returns>
	public static bool UnregisterElement(string name) {
		elementHandlers.Remove(name);

		return false;
	}

	/// <summary>
	/// Register an attribute value converter for a property value type. All types of properties must be registered
	/// to be able to be created using XML.
	/// </summary>
	/// <param name="type">Value type.</param>
	/// <param name="converter">Converter function.</param>
	public static void RegisterAttributeValueConverter(Type type, AttributeValueConverter converter) {
		if(!attributeValueConverters.ContainsKey(type))
			attributeValueConverters[type] = converter;
	}

	/// <summary>
	/// Register an event argument converter. All types of event arguments must be registered before usage.
	/// </summary>
	/// <param name="type">Event argument type.</param>
	/// <param name="converter">Converter function.</param>
	// Todo: Is this necessary? Maybe it could be avoided using reflection?
	public static void RegisterEventHandlerConverter(Type type, EventHandlerConverter converter) {
		if(!eventHandlerConverters.ContainsKey(type))
			eventHandlerConverters[type] = converter;
	}

	/// <summary>
	/// Parser constructor.
	/// </summary>
	/// <param name="stream">XML stream.</param>
	public Parser(Stream stream) {
		reader = XmlReader.Create(stream);
	}

	/// <summary>
	/// Parse XML.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	/// <returns>XML root control.</returns>
	public ControlBase? Parse(ControlBase parent) {
		if(reader == null) {
			throw new InvalidOperationException(READER_NULL_ERR);
		}

		ControlBase? container = null;

		while(reader.Read()) {
			switch(reader.NodeType) {
				case XmlNodeType.Element:
					container = ParseElement(parent);
					break;
			}
		}

		return container;
	}

	/// <summary>
	/// Parse element and call it's handler.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	/// <returns>Control.</returns>
	public ControlBase? ParseElement(ControlBase? parent) {
		if(reader == null) {
			throw new InvalidOperationException(READER_NULL_ERR);
		}

		if(elementHandlers.TryGetValue(reader.Name, out ElementDef? elementDef)) {
			currentElement = elementDef;

			return elementDef.Handler(this, elementDef.Type, parent);
		}

		return null;
	}

	/// <summary>
	/// Parse typed element and call it's handler.
	/// </summary>
	/// <typeparam name="T">Control type to be created.</typeparam>
	/// <param name="parent">Parent control.</param>
	/// <returns>Control.</returns>
	public T? ParseElement<T>(ControlBase parent) where T : ControlBase {
		Type type = typeof(T);
		XmlControlAttribute? attrib = null;
		object[] attribs = type.GetCustomAttributes(typeof(XmlControlAttribute), false);
		if(attribs.Length > 0)
			attrib = attribs[0] as XmlControlAttribute;

		if(elementHandlers.TryGetValue(attrib != null && attrib.ElementName != null ? attrib.ElementName : type.Name, out ElementDef? elementDef)) {
			if(elementDef.Type == type) {
				currentElement = elementDef;
				return elementDef.Handler(this, elementDef.Type, parent) as T;
			}
		}

		return null;
	}

	/// <summary>
	/// Parse attributes.
	/// </summary>
	/// <param name="element">Control.</param>
	public void ParseAttributes(ControlBase element) {
		if(reader == null) {
			throw new InvalidOperationException(READER_NULL_ERR);
		}

		if(reader.HasAttributes) {
			while(reader.MoveToNextAttribute()) {
				if(currentElement != null) {
					if(!SetAttributeValue(currentElement, element, reader.Name, reader.Value))
						throw new XmlException(String.Format("Attribute '{0}' not found.", reader.Name));
				} else {
					throw new XmlException("Trying to set an attribute value without an element.");
				}
			}

			reader.MoveToElement();
		}
	}

	internal void ParseComponentAttributes(Component component) {
		if(reader == null) {
			throw new InvalidOperationException(READER_NULL_ERR);
		}

		Type componentType = component.GetType();
		Type viewType = component.View?.GetType() ?? typeof(ControlBase);

		ElementDef? componentDef = null;
		ElementDef? viewDef = null;

		foreach(ElementDef elementDef in elementHandlers.Values) {
			if(elementDef.Type == componentType)
				componentDef = elementDef;
			else if(elementDef.Type == viewType)
				viewDef = elementDef;
		}

		if(componentDef == null)
			throw new XmlException("Component is not registered.");
		if(viewDef == null)
			throw new XmlException("Component view is not registered.");

		if(reader.HasAttributes) {
			while(reader.MoveToNextAttribute()) {
				if(component.View == null) {
					continue;
				}
				if(!SetAttributeValue(componentDef, component, reader.Name, reader.Value)) {
					if(!SetAttributeValue(viewDef, component.View, reader.Name, reader.Value)) {
						if(!SetComponentAttribute(component, reader.Name, reader.Value)) {
							throw new XmlException(String.Format("Attribute '{0}' not found.", reader.Name));
						}
					}
				}
			}

			reader.MoveToElement();
		}
	}

	private bool SetComponentAttribute(Component component, string attribute, string value) {
		if(component.GetValueType(attribute, out Type? type)) {
			if(type == null) {
				if(component.SetValue(attribute, value))
					return true;
			} else {
				if(attributeValueConverters.TryGetValue(type, out AttributeValueConverter? converter)) {
					if(component.SetValue(attribute, converter(component, value)))
						return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Check that the current element contains a content and moves to it.
	/// </summary>
	/// <returns>True if the element contains a content. False otherwise.</returns>
	public bool MoveToContent() {
		if(reader == null) {
			throw new InvalidOperationException(READER_NULL_ERR);
		}

		if(!reader.IsEmptyElement) {
			reader.MoveToContent();

			return true;
		}

		return false;
	}

	/// <summary>
	/// Parse content of the container element.
	/// </summary>
	/// <param name="parent">Parent control.</param>
	public void ParseContainerContent(ControlBase parent) {
		foreach(string elementName in NextElement()) {
			ParseElement(parent);
		}
	}

	/// <summary>
	/// Enumerate content of the container element.
	/// </summary>
	/// <returns>Element name.</returns>
	public IEnumerable<string> NextElement() {
		if(reader == null) {
			throw new InvalidOperationException(READER_NULL_ERR);
		}

		while(reader.Read()) {
			switch(reader.NodeType) {
				case XmlNodeType.Element:
					yield return reader.Name;
					break;
				case XmlNodeType.EndElement:
					yield break;
			}
		}
	}

	/// <summary>
	/// Get attribute value.
	/// </summary>
	/// <param name="attribute">Attribute name.</param>
	/// <returns>Attribute value. Null if an empty attribute or attribute not found.</returns>
	public string? GetAttribute(string attribute) {
		if(reader == null) {
			throw new InvalidOperationException(READER_NULL_ERR);
		}

		return reader.GetAttribute(attribute);
	}

	private bool SetAttributeValue(ElementDef elementDef, object element, string attribute, string value) {
		MemberInfo? memberInfo = elementDef.GetAttribute(attribute);
		if(memberInfo != null) {
			if(memberInfo is PropertyInfo propInfo) {
				return SetPropertyValue(element, propInfo, value);
			} else if(memberInfo is EventInfo eventInfo) {
				return SetEventValue(element, eventInfo, value);
			}
		}

		return false;
	}

	private bool SetPropertyValue(object element, PropertyInfo propertyInfo, string value) {
		if(attributeValueConverters.TryGetValue(propertyInfo.PropertyType, out AttributeValueConverter? converter)) {
			propertyInfo.SetValue(element, converter(element, value), null);
			return true;
		}

		throw new XmlException(String.Format("No converter found for an attribute '{0}' value type '{1}'.", propertyInfo.Name, propertyInfo.PropertyType.Name));
	}

	private bool SetEventValue(object element, EventInfo eventInfo, string value) {
		if(eventInfo.EventHandlerType?.IsGenericType ?? false) {
			Type[] ga = eventInfo.EventHandlerType.GetGenericArguments();
			if(ga.Length == 1) {
				if(eventHandlerConverters.TryGetValue(ga[0], out EventHandlerConverter? converter)) {
					eventInfo.AddEventHandler(element, converter(eventInfo.Name, value));
					return true;
				} else {
					throw new XmlException(String.Format("No event handler converter found for an event '{0}' event args type '{1}'.", eventInfo.Name, ga[0].Name));
				}
			}
		}

		throw new XmlException(String.Format("Event '{0}' is not a Gwen event", eventInfo.Name));
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if(disposing) {
			if(reader != null) {
				reader.Dispose();
				((IDisposable)reader).Dispose();
				reader = null;
			}
		}
	}

	private static ControlBase DefaultElementHandler(Parser parser, Type type, ControlBase? parent) {
		if(Activator.CreateInstance(type, parent) is not ControlBase element) {
			throw new Exception("Impossible");
		}

		parser.ParseAttributes(element);
		if(parser.MoveToContent()) {
			parser.ParseContainerContent(element);
		}

		return element;
	}

	/// <summary>
	/// Scan an assembly to find all controls that can be created using XML.
	/// </summary>
	/// <param name="assembly">Assembly.</param>
	public static void ScanControls(Assembly assembly) {
		foreach(Type type in assembly.GetTypes().Where(t => t.IsDefined(typeof(XmlControlAttribute), false))) {
			object[] attribs = type.GetCustomAttributes(typeof(XmlControlAttribute), false);
			if(attribs.Length > 0) {
				if(attribs[0] is XmlControlAttribute attrib) {
					ElementHandler handler;
					if(attrib.CustomHandler != null) {
						MethodInfo? mi = type.GetMethod(attrib.CustomHandler, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						if(mi != null) {
							handler = Delegate.CreateDelegate(typeof(ElementHandler), mi) as ElementHandler ?? throw new Exception("Impossible");
						} else {
							throw new Exception("Elemant handler not found.");
						}
					} else {
						handler = DefaultElementHandler;
					}

					string name = attrib.ElementName != null ? attrib.ElementName : type.Name;

					RegisterElement(name, type, handler);
				}
			}
		}
	}

	private static void ScanProperties(ElementDef elementDef) {
		foreach(var propertyInfo in elementDef.Type.GetProperties().Where(pi => pi.IsDefined(typeof(XmlPropertyAttribute), false))) {
			if(attributeValueConverters.ContainsKey(propertyInfo.PropertyType))
				elementDef.AddAttribute(propertyInfo.Name, propertyInfo);
			else
				throw new XmlException(String.Format("No converter found for an attribute '{0}' value type '{1}'.", propertyInfo.Name, propertyInfo.PropertyType.Name));
		}
	}

	private static void ScanEvents(ElementDef elementDef) {
		foreach(var eventInfo in elementDef.Type.GetEvents().Where(ei => ei.IsDefined(typeof(XmlEventAttribute), false))) {
			elementDef.AddAttribute(eventInfo.Name, eventInfo);
		}
	}

	/// <summary>
	/// Get list of controls that can be created using XML.
	/// </summary>
	/// <returns></returns>
	public static Dictionary<string, Type> GetElements() {
		Dictionary<string, Type> elements = new Dictionary<string, Type>();

		foreach(var element in elementHandlers) {
			elements[element.Key] = element.Value.Type;
		}

		return elements;
	}

	/// <summary>
	/// Get list of properties that can be set using XML.
	/// </summary>
	/// <param name="element"></param>
	/// <returns></returns>
	public static Dictionary<string, MemberInfo> GetAttributes(string element) {
		if(elementHandlers.TryGetValue(element, out ElementDef? elementDef)) {
			Dictionary<string, MemberInfo> attributes = new Dictionary<string, MemberInfo>();

			foreach(var attribute in elementDef.Attributes) {
				attributes[attribute.Key] = attribute.Value;
			}

			return attributes;
		}

		return new Dictionary<string, MemberInfo>();
	}

	public static NumberFormatInfo NumberFormatInfo => new NumberFormatInfo() { NumberGroupSeparator = "" };
	public static char[] ArraySeparator => new char[] { ',' };

	private class ElementDef {
		public Type Type { get; set; }
		public ElementHandler Handler { get; set; }

		internal Dictionary<string, MemberInfo> Attributes { get { return attributes; } }

		public ElementDef(Type type, ElementHandler handler) {
			Type = type;
			Handler = handler;
		}

		public void AddAttribute(string name, MemberInfo memberInfo) {
			attributes[name] = memberInfo;
		}

		public MemberInfo? GetAttribute(string name) {
			if(attributes.TryGetValue(name, out MemberInfo? mi))
				return mi;
			else
				return null;
		}

		private Dictionary<string, MemberInfo> attributes = new Dictionary<string, MemberInfo>();
	}
}