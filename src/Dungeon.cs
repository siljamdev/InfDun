using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Concurrent;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;
using StbImageSharp;
using StbImageWriteSharp;
using AshLib;
using AshLib.Time;
using AshLib.Folders;
using AshLib.AshFiles;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;


partial class Dungeon : GameWindow{
	
	public const string version = "0.1.4";
	
	#region static
	public static ConcurrentQueue<IDisposable> resourcesMarkedForDisposal = new(); //Need to disposeOfResources of these on the same thread or else a Fatal error will occur
	
	public static DeltaHelper dh;
	
	static GLFWCallbacks.ErrorCallback GLFWErrorCallback;
	
	static void Main(string[] args){
		#if WINDOWS
			if(GetConsoleWindow() == IntPtr.Zero){
				AttachConsole(ATTACH_PARENT_PROCESS);
			}
		#endif
		
		GLFWErrorCallback = OnGLFWError;
		
		GLFWProvider.SetErrorCallback(GLFWErrorCallback);
		
		dh = new DeltaHelper();
		dh.Start();
		
		#if DEBUG
			using(Dungeon dun = new Dungeon(new NativeWindowSettings{
				Title = "InfDun - BETA v" + version,
				Vsync = VSyncMode.On,
				ClientSize = new Vector2i(640, 480),
				Icon = getIcon(),
				Flags = ContextFlags.Debug
			})){
				dun.Run();
			}
		#else
			using(Dungeon dun = new Dungeon(new NativeWindowSettings{
				Title = "InfDun - BETA",
				Vsync = VSyncMode.On,
				ClientSize = new Vector2i(640, 480),
				Icon = getIcon()
			})){
				dun.Run();
			}
		#endif
	}
	
	static WindowIcon getIcon(){
		using Stream s = AssemblyFiles.getStream("res.icon.png");
		
		//Generate the image and put it as icon
		ImageResult image = ImageResult.FromStream(s, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
		if(image == null || image.Data == null){
			return null;
		}
		
		OpenTK.Windowing.Common.Input.Image i = new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, image.Data);
		WindowIcon w = new WindowIcon(i);
		
		return w;
	}
	
	#region errors
	private static void OnGLFWError(OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode error, string description){
        Console.Error.WriteLine("[GLFW Error] " + error + ": " + description);
    }
	#endregion
	#endregion
	
	KeyBind fullscreen = new KeyBind(Keys.F11, false).addToConfigurables("keybinds.fullscreen", "Toggle fullscreen");
	KeyBind screenshot = new KeyBind(Keys.F2, false).addToConfigurables("keybinds.screenshot", "Take screenshot");
	
	KeyBind advancedMode = new KeyBind(Keys.LeftAlt, false).addToConfigurables("keybinds.advancedMode", "Toggle advanced mode");
	
	KeyBind moveUp = new KeyBind(Keys.W, true).addToConfigurables("keybinds.up", "Move up");
	KeyBind moveDown = new KeyBind(Keys.S, true).addToConfigurables("keybinds.down", "Move down");
	KeyBind moveLeft = new KeyBind(Keys.A, true).addToConfigurables("keybinds.left", "Move left");
	KeyBind moveRight = new KeyBind(Keys.D, true).addToConfigurables("keybinds.right", "Move right");
	
	KeyBind interact = new KeyBind(Keys.Space, true).addToConfigurables("keybinds.interact", "Interact");
	
	//These dont change and arent configurable
	KeyBind escape = new KeyBind(Keys.Escape, Keys.LeftShift, false);
	KeyBind help = new KeyBind(Keys.F1, false);
	KeyBind logUp = new KeyBind(Keys.Up, Keys.LeftShift, true);
	KeyBind logDown = new KeyBind(Keys.Down, Keys.LeftShift, true);
	KeyBind enter = new KeyBind(Keys.Enter, true);
	
	#if DEBUG_TIME
		KeyBind debug = new KeyBind(Keys.M, false);
	#endif
	
	#if DEBUG_LEVELS
		KeyBind skipLevel = new KeyBind(Keys.I, false);
		KeyBind zoomCamera = new KeyBind(Keys.Z, Keys.LeftShift, false);
	#endif
	
	public Dependencies dep {get; private set;}
	public AshFile config;
	
	Renderer ren;
	public SoundManager sm;
	
	public Scene sce;
	
	public bool takeScreenshotNextTick;
	public bool isFullscreened => this.WindowState == WindowState.Fullscreen;
	
	public int highscore{
		get{
			return config.GetValue<int>("highscore");
		}
		set{
			config.Set("highscore", value);
			config.Save();
			high.setText("Highscore: " + value);
			highscor.setText("Highscore: " + value);
		}
	}
	
	#if DEBUG
		DebugProc DebugMessageDelegate;
	#endif
	
	//new NativeWindowSettings{NumberOfSamples = 4}
	Dungeon(NativeWindowSettings n) : base(GameWindowSettings.Default, n){
		CenterWindow();
	}
	
	void initialize(){
		string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		dep = new Dependencies(appDataPath + "/ashproject/infdun", true, new string[]{"screenshots"}, null);
		
		#if DEBUG
			DebugMessageDelegate = OnDebugMessage;
			
			GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
			GL.Enable(EnableCap.DebugOutput);
			
			// Optionally
			GL.Enable(EnableCap.DebugOutputSynchronous);
			
			Console.WriteLine("Testing stdout");
			Console.Error.WriteLine("Testing stderr");
		#endif
		
		ren = new Renderer(this);
		sm = new SoundManager();
		
		initializeConfig();
		
		//Init statics, resources
		Scene.initialize();
		AABB.initialize();
		LineStrip.initialize();
		UiScreen.initialize();
		
		initializeScreens();
		
		highscore = highscore; //So it updates, needs to be later bc ui elements are updated as well
		
		ren.setScreen(mainMenu);
	}
	
	//These appear in options
	public (string key, object value, string description)[] getConfigurableOptions(){
		return new (string key, object value, string description)[]{
			("vsync", true, "Vsync"),
			("maxFps", 144f, "Maximum FPS"),
			("sound", true, "Sound"),
			("uiSound", true, "UI Sound"),
			("particles", true, "Particles"),
			("clock", true, "Animate turn")
		};
	}
	
	//These do not appear in options
	AshFileModel getNonConfigurableConfigModel(){
		return new AshFileModel(
			new ModelInstance(ModelInstanceOperation.Type, "highscore", 0)
		);
	}
	
	void initializeConfig(){
		AshFileModel afm = new AshFileModel(getConfigurableOptions().Select(o => new ModelInstance(ModelInstanceOperation.Type, o.key, o.value)).ToArray());
		afm.Merge(getNonConfigurableConfigModel());
		afm.Merge(KeyBind.getModel());
		
		config = dep.config;
		
		afm.deleteNotMentioned = true;
		
		config.ApplyModel(afm);
		
		//Set current version and path. Might be needed by someone (maybe)
		config.Set("version", version);
		try{ //Might not work on linux
			config.Set("path", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		}catch{}
		
		config.Save();
		
		loadConfig();
		loadControls();
	}
	
	public void loadConfig(){
		setVsync(config.GetValue<bool>("vsync"));
		this.UpdateFrequency = config.GetValue<float>("maxFps");
		
		sm.isActive = config.GetValue<bool>("sound");
		ParticleRenderer.particlesEnabled = config.GetValue<bool>("particles");
		
		UiScreen.uiSounds = config.GetValue<bool>("uiSound");
		ren.showClock = config.GetValue<bool>("clock");
	}
	
	//Reset the ones that appear in options
	public void resetConfig(){
		AshFileModel r = new AshFileModel(getConfigurableOptions().Select(h => new ModelInstance(ModelInstanceOperation.Value, h.key, h.value)).ToArray());
		config.ApplyModel(r);
		
		config.Save();
		
		loadConfig();
		
		ren.setCornerInfo("Reset config", Renderer.selectedTextColor);
	}
	
	public void loadControls(){
		foreach(KeyBind k in KeyBind.configurables){
			if(config.TryGetValue(k.configKey, out int v)){
				k.key = (Keys) v;
			}
		}
	}
	
	public void saveControls(){
		config.ApplyModel(KeyBind.getSaveModel());
		
		config.Save();
		
		ren.setCornerInfo("Saved controls", Renderer.selectedTextColor);
	}
	
	public void resetControls(){
		config.ApplyModel(KeyBind.getResetModel());
		
		config.Save();
		
		loadControls();
		
		ren.setCornerInfo("Reset controls", Renderer.selectedTextColor);
	}
	
	void onResize(int x, int y){
		GL.Viewport(0, 0, x, y);
		ren?.updateSize(x, y);
	}
	
	void setVsync(bool b){
		this.VSync = b ? VSyncMode.On : VSyncMode.Off;
	}
	
	void handleKeyboardInput(){
		//check to see if the window is focused
		if(!IsFocused){
			sce?.endFrame();
			ren.cam.endFrame();
			return;
		}
		
		#if DEBUG_TIME
			if(debug.isActive(KeyboardState)){
				Console.WriteLine(Scene.tt.MeanString());
			}
		#endif
		
		#if DEBUG_LEVELS
			if(skipLevel.isActive(KeyboardState)){
				sce?.skipLevel();
			}
			
			switch(zoomCamera.isActiveMod(KeyboardState)){
				case 1:
				Camera.zoomD += 3f;
				onResize(ren.width, ren.height);
				break;
				
				case 2:
				Camera.zoomD -= 3f;
				onResize(ren.width, ren.height);
				break;
			}
		#endif
		
		switch(escape.isActiveMod(KeyboardState)){
			case 1:
			if(ren.currentScreen != null && ren.currentScreen != deathMenu){
				ren.closeScreen();
			}else{
				ren.setScreen(pauseMenu);
			}
			
			break;
			
			case 2:
			this.Close();
			break;
		}
		
		if(screenshot.isActive(KeyboardState)){
			captureScreenshot();
			ren.setCornerInfo("Saved screenshot");
		}
		
		if(fullscreen.isActive(KeyboardState)){
			toggleFullscreen();
		}
		
		if(help.isActive(KeyboardState) && ren.currentScreen != helpMenu){
			ren.setScreen(helpMenu);
		}
		
		if(advancedMode.isActive(KeyboardState)){
			ren.toggleAdvancedMode();
		}
		
		if(ren.currentScreen != null){
			switch(logUp.isActiveMod(KeyboardState)){
				case 1:
					ren.currentScreen.scroll(-20f * (float) dh.deltaTime);
					break;
				
				case 2:
					ren.currentScreen.scroll(-60f * (float) dh.deltaTime);
					break;
				
				default:
					switch(logDown.isActiveMod(KeyboardState)){
						case 1:
							ren.currentScreen.scroll(20f * (float) dh.deltaTime);
							break;
						
						case 2:
							ren.currentScreen.scroll(60f * (float) dh.deltaTime);
							break;
					}
					break;
				
			}
			
			if(enter.isActive(KeyboardState) && (ren.currentScreen == mainMenu || ren.currentScreen == deathMenu)){
				setNewScene();
			}
			
			sce?.endFrame();
			ren.cam.endFrame();
			return;
		}
		
		if(sce?.canMovePlayer == true){
			if(interact.isActive(KeyboardState) && sce.interact()){
				
			}else if(moveUp.isActive(KeyboardState) && sce.movePlayerUp()){
				
			}else if(moveDown.isActive(KeyboardState) && sce.movePlayerDown()){
				
			}else if(moveLeft.isActive(KeyboardState) && sce.movePlayerLeft()){
				
			}else if(moveRight.isActive(KeyboardState) && sce.movePlayerRight()){
				
			}
		}
		
		sce?.endFrame();
		ren.cam.endFrame();
	}
	
	public void setNewScene(){
		sce = new Scene(ren, sm);
		ren.setScreen(null);
	}
	
	public void closeScene(){
		ren.screenAlpha = 0.6f; //Reset
		sce?.Dispose();
		sce = null;
		ren.setScreen(null);
		ren.setScreen(mainMenu);
	}
	
	void toggleFullscreen(){
		VSyncMode t = VSync;
		if(!isFullscreened){
			//MonitorInfo mi = Monitors.GetMonitorFromWindow(this);
			this.WindowState = WindowState.Fullscreen;
			//this.CurrentMonitor = mi;
			//MakeFullscreen(mi.Handle);
			VSync = t;
		}else{
			this.WindowState = WindowState.Normal;
			VSync = t;
		}
	}
	
	void captureScreenshot(){
		int width = ren.width;
		int height = ren.height;
		
		// Create a byte array to hold the pixel data
		byte[] pixels = new byte[width * height * 3]; // RGBA (4 bytes per pixel)
		
		// Read pixels from OpenGL frame buffer
		GL.ReadBuffer(ReadBufferMode.Front);
		GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, pixels);
		
		byte[] rgbPixels = new byte[width * height * 3]; // RGB (3 bytes per pixel)
		
		// Copy only RGB values
		for(int i = 0; i < height; i++){
			for(int j = 0; j < width; j++){
				rgbPixels[((height - i - 1) * width + j) * 3] = pixels[(i * width + j) *3 + 2];      // Blue
				rgbPixels[((height - i - 1) * width + j) * 3 + 1] = pixels[(i * width + j) * 3 + 1];  // Green
				rgbPixels[((height - i - 1) * width + j) * 3 + 2] = pixels[(i * width + j) * 3];  // Red
			}
		}
		
		// Write PNG using StbImageWriteSharp
		var writer = new StbImageWriteSharp.ImageWriter();
		using (var stream = File.OpenWrite(dep.path + "/screenshots/" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png")){
			writer.WritePng(rgbPixels, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlue, stream);
		}
	}
	
	#region errors
	public void checkErrors(){
		OpenTK.Graphics.OpenGL.ErrorCode errorCode = GL.GetError();
        while(errorCode != OpenTK.Graphics.OpenGL.ErrorCode.NoError){
            Console.Error.WriteLine("[OpenGL Error] " + errorCode);
			if(ren != null){
				ren.setCornerInfo("[OpenGL Error] " + errorCode, Renderer.redTextColor);
			}
			
            errorCode = GL.GetError();
        }
		sm.checkErrors();
	}
	
	#if DEBUG
		void OnDebugMessage(
			DebugSource source,     // Source of the debugging message.
			DebugType type,         // Type of the debugging message.
			int id,                 // ID associated with the message.
			DebugSeverity severity, // Severity of the message.
			int length,             // Length of the string in pMessage.
			IntPtr pMessage,        // Pointer to message string.
			IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
		{
			// In order to access the string pointed to by pMessage, you can use Marshal
			// class to copy its contents to a C# string without unsafe code. You can
			// also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
			string message = Marshal.PtrToStringUTF8(pMessage, length);
			
			Console.Error.WriteLine("[OpenGL Error] Severity: " + severity + " Source: " + source + " Type: " + type + " Id: " + id + " Message: " + message);
			
			ren?.setCornerInfo("[GL Error] " + source, Renderer.redTextColor);
		}
	#endif
	#endregion
	
	void disposeOfResources(){
		while(resourcesMarkedForDisposal.TryDequeue(out IDisposable r)){
			r.Dispose();
		}
	}
	
	protected override void OnKeyDown(KeyboardKeyEventArgs e){
		if(ren.currentScreen != null){
			if(!e.IsRepeat && e.Key != Keys.Escape && e.Key != Keys.Backspace){
				ren.currentScreen.trySetKeybind(e.Key);
			}else if(e.Key == Keys.Backspace){
				ren.currentScreen.tryDelChar();
			}
		}else{
			base.OnKeyDown(e);
		}
	}
	
	protected override void OnTextInput(TextInputEventArgs e){
		if(ren.currentScreen != null){
			string s = e.AsString;
			ren.currentScreen.tryAddStr(s);
		}
		
		base.OnTextInput(e);
	}
	
	protected override void OnLoad(){
		initialize();
		base.OnLoad();
	}
	
	protected override void OnUnload(){
		disposeOfResources();
		base.OnUnload();
	}
	
	protected override void OnResize(ResizeEventArgs args){
		onResize(args.Width, args.Height);
		base.OnResize(args);
	}
	
	protected override void OnUpdateFrame(FrameEventArgs args){
		handleKeyboardInput();
		base.OnUpdateFrame(args);
	}
	
	protected override void OnRenderFrame(FrameEventArgs args){
		ren.draw();
		Context.SwapBuffers();
		checkErrors();
		disposeOfResources();
		if(takeScreenshotNextTick){
			takeScreenshotNextTick = false;
			captureScreenshot();
			ren.setCornerInfo("Saved screenshot");
		}
		base.OnRenderFrame(args);
		dh.Frame();
	}
	
	protected override void OnMouseWheel(MouseWheelEventArgs args){
		if(ren.currentScreen != null){
			ren.currentScreen.scroll(args.OffsetY);
		}
		
		base.OnMouseWheel(args);
    }
	
	protected override void OnMouseMove(MouseMoveEventArgs e){
        ren.cam.mouse(e.X, e.Y);
		base.OnMouseMove(e);
    }
	
	protected override void OnMouseDown(MouseButtonEventArgs e){
        if(e.Button == MouseButton.Left){
			if(sce != null){
				if(ren.currentScreen == null){
					if(!ren.overlayScreen.click(ren, KeyboardState.IsKeyDown(Keys.LeftShift))){
						sce.click();
					}
				}else{
					ren.currentScreen.click(ren, KeyboardState.IsKeyDown(Keys.LeftShift));
				}
			}else{
				ren.currentScreen.click(ren, KeyboardState.IsKeyDown(Keys.LeftShift));
			}
        }
		
		base.OnMouseDown(e);
    }
	
	#if WINDOWS		
		[DllImport("kernel32.dll")]
		static extern bool AttachConsole(int dwProcessId);
		const int ATTACH_PARENT_PROCESS = -1;
		
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
	#endif
}