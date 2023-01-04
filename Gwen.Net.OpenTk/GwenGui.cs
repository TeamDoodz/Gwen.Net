using System;
using Gwen.Net.Control;
using Gwen.Net.OpenTk.Exceptions;
using Gwen.Net.OpenTk.Input;
using Gwen.Net.OpenTk.Platform;
using Gwen.Net.OpenTk.Renderers;
using Gwen.Net.Platform;
using Gwen.Net.Skin;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;

namespace Gwen.Net.OpenTk;

internal class GwenGui : IGwenGui {
	const string NOT_LOADED_ERR = "Gui must be loaded to perform this action. Did you not call Load()?";

	private OpenTKRendererBase? renderer;
	private SkinBase? skin;
	private Canvas? canvas;
	private OpenTkInputTranslator? input;

	public GwenGuiSettings Settings { get; }

	public GameWindow Parent { get; }

	public Canvas Root => canvas ?? throw new InvalidOperationException(NOT_LOADED_ERR);

	public bool IsLoaded { get; private set; } = false;

	internal GwenGui(GameWindow parent, GwenGuiSettings settings) {
		Parent = parent;
		Settings = settings;
	}

	public void Load() {
		GwenPlatform.Init(new NetCorePlatform(SetCursor));
		AttachToWindowEvents();
		renderer = ResolveRenderer(Settings.Renderer);
		skin = new TexturedBase(renderer, "DefaultSkin2.png") {
			DefaultFont = new Font(renderer, "Calibri", 11)
		};
		canvas = new Canvas(skin);
		input = new OpenTkInputTranslator(canvas);

		canvas.SetSize(Parent.Size.X, Parent.Size.Y);
		canvas.ShouldDrawBackground = true;
		canvas.BackgroundColor = skin.Colors.ModalBackground;

		IsLoaded = true;
	}

	public void Render() {
		if(canvas == null) {
			throw new InvalidOperationException(NOT_LOADED_ERR);
		}

		canvas.RenderCanvas();
	}

	public void Resize(Vector2i size) {
		if(canvas == null || renderer == null) {
			throw new InvalidOperationException(NOT_LOADED_ERR);
		}

		renderer.Resize(size.X, size.Y);
		canvas.SetSize(size.X, size.Y);
	}

	public void Dispose() {
		if(canvas == null || skin == null || renderer == null) {
			throw new InvalidOperationException(NOT_LOADED_ERR);
		}

		DetachWindowEvents();
		canvas.Dispose();
		skin.Dispose();
		renderer.Dispose();
	}

	private void AttachToWindowEvents() {
		Parent.KeyUp += Parent_KeyUp;
		Parent.KeyDown += Parent_KeyDown;
		Parent.TextInput += Parent_TextInput;
		Parent.MouseDown += Parent_MouseDown;
		Parent.MouseUp += Parent_MouseUp;
		Parent.MouseMove += Parent_MouseMove;
		Parent.MouseWheel += Parent_MouseWheel;
	}

	private void DetachWindowEvents() {
		Parent.KeyUp -= Parent_KeyUp;
		Parent.KeyDown -= Parent_KeyDown;
		Parent.TextInput -= Parent_TextInput;
		Parent.MouseDown -= Parent_MouseDown;
		Parent.MouseUp -= Parent_MouseUp;
		Parent.MouseMove -= Parent_MouseMove;
		Parent.MouseWheel -= Parent_MouseWheel;
	}

	private void Parent_KeyUp(KeyboardKeyEventArgs obj)
		=> input?.ProcessKeyUp(obj);

	private void Parent_KeyDown(KeyboardKeyEventArgs obj)
		=> input?.ProcessKeyDown(obj);

	private void Parent_TextInput(TextInputEventArgs obj)
		=> input?.ProcessTextInput(obj);

	private void Parent_MouseDown(MouseButtonEventArgs obj)
		=> input?.ProcessMouseButton(obj);

	private void Parent_MouseUp(MouseButtonEventArgs obj)
		=> input?.ProcessMouseButton(obj);

	private void Parent_MouseMove(MouseMoveEventArgs obj)
		=> input?.ProcessMouseMove(obj);

	private void Parent_MouseWheel(MouseWheelEventArgs obj)
		=> input?.ProcessMouseWheel(obj);

	private void SetCursor(MouseCursor mouseCursor) {
		Parent.Cursor = mouseCursor;
	}

	private static OpenTKRendererBase ResolveRenderer(GwenGuiRenderer gwenGuiRenderer) {
		return gwenGuiRenderer switch {
			GwenGuiRenderer.GL10 => throw new NotSupportedException("OpenGL 1 is not supported."),
			GwenGuiRenderer.GL20 => throw new NotSupportedException("OpenGL 2 is not supported."),
			GwenGuiRenderer.GL40 => new OpenTKGL40Renderer(),
			_ => throw new RendererNotFoundException(gwenGuiRenderer),
		};
	}
}
