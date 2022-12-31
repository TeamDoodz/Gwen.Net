using System;

namespace Gwen.Net
{
    public struct HSV
    {
        public float H;
        public float S;
        public float V;

		public HSV(float H, float S, float V) {
			this.H = H;
			this.S = S;
			this.V = V;
		}

		public HSV(float H) : this(H, 0.5f, 0.5f) {}

		public Color ToColor() {
			return HSVToColor(H, S, V);
		}

		public static HSV FromColor(Color color) {
			HSV hsv = new HSV();

			float r = (float)color.R / 255.0f;
			float g = (float)color.G / 255.0f;
			float b = (float)color.B / 255.0f;

			float max = Math.Max(r, Math.Max(g, b));
			float min = Math.Min(r, Math.Min(g, b));

			hsv.V = max;

			float delta = max - min;

			if (max != 0) {
				hsv.S = delta / max;
			} else {
				hsv.S = 0;
			}

			if (delta != 0) {
				if (r == max)
					hsv.H = (g - b) / delta;
				else if (g == max)
					hsv.H = 2 + (b - r) / delta;
				else
					hsv.H = 4 + (r - g) / delta;

				hsv.H *= 60;
				if (hsv.H < 0)
					hsv.H += 360;
			} else {
				hsv.H = 0;
			}

			return hsv;
		}

		public static Color HSVToColor(float h, float s, float v) {
			int hi = Convert.ToInt32(Math.Floor(h / 60)) % 6;
			float f = h / 60 - (float)Math.Floor(h / 60);

			v = v * 255;
			int va = Convert.ToInt32(v);
			int p = Convert.ToInt32(v * (1 - s));
			int q = Convert.ToInt32(v * (1 - f * s));
			int t = Convert.ToInt32(v * (1 - (1 - f) * s));

			if (hi == 0)
				return new Color(255, va, t, p);
			if (hi == 1)
				return new Color(255, q, va, p);
			if (hi == 2)
				return new Color(255, p, va, t);
			if (hi == 3)
				return new Color(255, p, q, va);
			if (hi == 4)
				return new Color(255, t, p, va);
			return new Color(255, va, p, q);
		}
	}
}