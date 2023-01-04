namespace Gwen.Net.Anim.Size;

class Width : TimedAnimation {
	private int startSize;
	private int delta;
	private bool hide;

	public Width(int startSize, int endSize, float length, bool hide = false, float delay = 0.0f, float ease = 1.0f)
		: base(length, delay, ease) {
		this.startSize = startSize;
		delta = endSize - this.startSize;
		this.hide = hide;
	}

	protected override void OnStart() {
		base.OnStart();
		//m_Control.ActualWidth = m_StartSize;
	}

	protected override void Run(float delta) {
		base.Run(delta);
		//m_Control.ActualWidth = (int)Math.Round(m_StartSize + (m_Delta * delta));
	}

	protected override void OnFinish() {
		base.OnFinish();
		//m_Control.ActualWidth = m_StartSize + m_Delta;
		if(control != null) {
			control.IsHidden = hide;
		}
	}
}