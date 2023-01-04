using System;
using System.Reflection;
using Gwen.Net.Control;

namespace Gwen.Net.Xml;

/// <summary>
/// XML based event handler.
/// </summary>
/// <typeparam name="T">Type of event arguments.</typeparam>
public class XmlEventHandler<T> where T : System.EventArgs {
	private readonly string eventName;
	private readonly string handlerName;
	private Type[] paramsType = new Type[] { typeof(ControlBase), typeof(T) };

	public XmlEventHandler(string handlerName, string eventName) {
		this.eventName = eventName;
		this.handlerName = handlerName;
	}

	public void OnEvent(ControlBase sender, T args) {
		ControlBase? handlerElement = sender.Parent;

		if(sender is Window)
			handlerElement = sender;
		else if(sender is TreeNode node)
			handlerElement = node.TreeControl?.Parent;

		while(handlerElement != null) {
			if(handlerElement.Component != null) {
				if(handlerElement.Component.HandleEvent(eventName, handlerName, sender, args)) {
					break;
				} else {
					Type type = handlerElement.Component.GetType();

					MethodInfo? methodInfo = null;
					do {
						MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						foreach(MethodInfo mi in methods) {
							if(mi.Name != handlerName)
								continue;
							ParameterInfo[] parameters = mi.GetParameters();
							if(parameters.Length != 2)
								continue;
							if(parameters[0].ParameterType != typeof(Gwen.Net.Control.ControlBase) || (parameters[1].ParameterType != typeof(T) && parameters[1].ParameterType != typeof(T).BaseType))
								continue;
							methodInfo = mi;
							break;
						}
						if(methodInfo != null)
							break;
						type = type.BaseType ?? throw new Exception("I'll come up with an error message for this later.");
					}
					while(type != null);

					if(methodInfo != null) {
						methodInfo.Invoke(handlerElement.Component, new object[] { sender, args });
						break;
					}
				}
			}

			if(handlerElement is Menu menu) {
				handlerElement = menu.ParentMenuItem;
			} else {
				handlerElement = handlerElement.Parent;
			}
		}
	}
}