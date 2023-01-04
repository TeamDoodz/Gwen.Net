using System;
using System.Collections.Generic;
using System.Linq;
using Gwen.Net.Control;

namespace Gwen.Net.Anim;

public class Animation {
	protected ControlBase? control;

	//private static List<Animation> g_AnimationsListed = new List<Animation>(); // unused
	private static readonly Dictionary<ControlBase, List<Animation>> animations = new Dictionary<ControlBase, List<Animation>>();

	protected virtual void Think() {

	}

	public virtual bool Finished {
		get { throw new InvalidOperationException("Pure virtual function call"); }
	}

	public static void Add(ControlBase control, Animation animation) {
		animation.control = control;
		if(!animations.ContainsKey(control))
			animations[control] = new List<Animation>();
		animations[control].Add(animation);
	}

	public static void Cancel(ControlBase control) {
		if(animations.ContainsKey(control)) {
			animations[control].Clear();
			animations.Remove(control);
		}
	}

	internal static void GlobalThink() {
		foreach(KeyValuePair<ControlBase, List<Animation>> pair in animations) {
			var valCopy = pair.Value.TakeWhile(x => true); // list copy so foreach won't break when we remove elements
			foreach(Animation animation in valCopy) {
				animation.Think();
				if(animation.Finished) {
					pair.Value.Remove(animation);
				}
			}
		}
	}
}