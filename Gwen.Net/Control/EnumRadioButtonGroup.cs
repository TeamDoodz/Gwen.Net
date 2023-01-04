using System;

namespace Gwen.Net.Control;
public class EnumRadioButtonGroup<T> : RadioButtonGroup where T : struct, Enum {
	public EnumRadioButtonGroup(ControlBase? parent) : base(parent) {
		T[] vals = Enum.GetValues<T>();
		for(int i = 0; i < vals.Length; i++) {
			string name = Enum.GetNames<T>()[i];
			LabeledRadioButton lrb = AddOption(name);
			lrb.UserData = vals.GetValue(i);
		}
	}

	public T? SelectedValue {
		get {
			if(Selected == null) return null;
			return (T)(Selected.UserData ?? throw new Exception("EnumRadioButtonGroup user data is null. This shouldn't happen!"));
		}
		set {
			foreach(ControlBase child in Children) {
				if(child.UserData == null) {
					throw new Exception("EnumRadioButtonGroup child user data is null. This shouldn't happen!");
				}
				if(child is LabeledRadioButton radioButton && child.UserData.Equals(value)) {
					radioButton.RadioButton.Press();
				}
			}
		}
	}
}