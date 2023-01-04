namespace Gwen.Net.Anim.Size;

class Height : TimedAnimation {
	private int startSize;
	private int delta;
	private bool hide;

	public Height(int startSize, int endSize, float length, bool hide = false, float delay = 0.0f, float ease = 1.0f)
		: base(length, delay, ease) {
		this.startSize = startSize;
		delta = endSize - this.startSize;
		this.hide = hide;
	}

	protected override void OnStart() {
		base.OnStart();
		//m_Control.ActualHeight = m_StartSize;
	}

	protected override void Run(float delta) {
		base.Run(delta);
		//m_Control.ActualHeight = (int)(m_StartSize + (m_Delta * delta));
	}

	protected override void OnFinish() {
		base.OnFinish();
		//m_Control.ActualHeight = m_StartSize + m_Delta;
		if(control != null) {
			control.IsHidden = hide;
		}
	}
}