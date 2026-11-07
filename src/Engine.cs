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


partial class Engine : GameWindow{	
	//Need to disposeOfResources of these on the same thread or else a Fatal error will occur
	public ConcurrentQueue<IDisposable> resourcesMarkedForDisposal = new();
	
	//Utilities
	public DeltaHelper dh;
	public Dependencies dep {get; private set;}
	public AshFile config;
	
	public SoundManager sm;
	public Screens sc {get; private set;}
	
	//Renderers, hold state
	public Scene sce;
	public UIRenderer uren {get; private set;}
	public InfoRenderer iren {get; private set;}
	public MainMenuRenderer mmren {get; private set;}
	
	//Drawers, they ONLY draw. A renderer is injected into them to draw
	public RectDrawer2D rd {get; private set;}
	public TextureDrawer2D td {get; private set;}
	public FontDrawer2D fd {get; private set;}
	
	//Keybinds
	KeyBind fullscreen = new KeyBind(Keys.F11, false).addToConfigurables("keybinds.fullscreen", "Toggle fullscreen");
	KeyBind screenshot = new KeyBind(Keys.F2, false).addToConfigurables("keybinds.screenshot", "Take screenshot");
	
	KeyBind advancedMode = new KeyBind(Keys.LeftAlt, false).addToConfigurables("keybinds.advancedMode", "Toggle advanced mode");
	
	KeyBind moveUp = new KeyBind(Keys.W, true).addToConfigurables("keybinds.up", "Move up");
	KeyBind moveDown = new KeyBind(Keys.S, true).addToConfigurables("keybinds.down", "Move down");
	KeyBind moveLeft = new KeyBind(Keys.A, true).addToConfigurables("keybinds.left", "Move left");
	KeyBind moveRight = new KeyBind(Keys.D, true).addToConfigurables("keybinds.right", "Move right");
	
	KeyBind interact = new KeyBind(Keys.Space, true).addToConfigurables("keybinds.interact", "Interact");
	KeyBind seeInterctions = new KeyBind(Keys.E, true).addToConfigurables("keybinds.seeInterctions", "See interactions");
	KeyBind skipTime = new KeyBind(Keys.T, true).addToConfigurables("keybinds.skipTime", "Skip turn");
	
	//These dont change and arent configurable
	KeyBind escape = new KeyBind(Keys.Escape, Keys.LeftShift, false);
	KeyBind help = new KeyBind(Keys.F1, false);
	KeyBind logUp = new KeyBind(Keys.Up, Keys.LeftShift, true);
	KeyBind logDown = new KeyBind(Keys.Down, Keys.LeftShift, true);
	KeyBind enter = new KeyBind(Keys.Enter, true);
	KeyBind one = new KeyBind(Keys.D1, true);
	KeyBind two = new KeyBind(Keys.D2, true);
	KeyBind three = new KeyBind(Keys.D3, true);
	KeyBind four = new KeyBind(Keys.D4, true);
	KeyBind five = new KeyBind(Keys.D5, true);
	KeyBind six = new KeyBind(Keys.D6, true);
	KeyBind seven = new KeyBind(Keys.D7, true);
	KeyBind eight = new KeyBind(Keys.D8, true);
	KeyBind nine = new KeyBind(Keys.D9, true);
	
	#if DEBUG_TIME
		KeyBind debug = new KeyBind(Keys.M, false);
	#endif
	
	#if DEBUG_LEVELS
		KeyBind skipLevel = new KeyBind(Keys.I, false);
		KeyBind getKey = new KeyBind(Keys.K, false);
		KeyBind zoomCamera = new KeyBind(Keys.Z, Keys.LeftShift, false);
		KeyBind resetCamera = new KeyBind(Keys.X, false);
		KeyBind freeCamera = new KeyBind(Keys.C, false);
	#endif
	
	#if DEBUG_ENTITIES
		KeyBind instaKill = new KeyBind(Keys.J, false);
	#endif
	
	//Flags
	public bool takeScreenshotNextFrame;
	public bool isFullscreened => this.WindowState == WindowState.Fullscreen;
	
	public int width => ClientSize.X;
	public int height => ClientSize.Y;
	
	public Vector2 mousePosition {get; private set;}
	
	//Events
	public event Action<int, int> onResize;
	public event Action<float, float> onMouseMove;
	public event Action onFrameStart;
	public event Action onFrameEnd;
	public event Action<bool> onParticleActivation;
	public event Action<float> onMouseScroll;
	
	public event Action<int> onHighscoreUpdate;
	
	public int highscore{
		get{
			return config.GetValue<int>("highscore");
		}
		set{
			config.Set("highscore", value);
			config.Save();
			
			//Screens
			onHighscoreUpdate?.Invoke(value);
		}
	}
	
	#if DEBUG
		DebugProc DebugMessageDelegate;
	#endif
	
	//new NativeWindowSettings{NumberOfSamples = 4}
	Engine(NativeWindowSettings n) : base(GameWindowSettings.Default, n){
		CenterWindow();
	}
	
	void initialize(){
		//Init dependencies
		string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		dep = new Dependencies(appDataPath + "/ashproject/infdun", true, new string[]{"screenshots"}, null);
		
		//Init deltahelper
		dh = new DeltaHelper();
		dh.Start();
		
		#if DEBUG
			DebugMessageDelegate = OnDebugMessage;
			
			GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
			GL.Enable(EnableCap.DebugOutput);
			
			//Optionally
			GL.Enable(EnableCap.DebugOutputSynchronous);
			
			Console.WriteLine("Testing stdout");
			Console.Error.WriteLine("Testing stderr");
		#endif
		
		//Enable blending (transparency)
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		
		//VERY IMPORTANT: INIT OPENAL BEFORE SOUND CREATION
		sm = new SoundManager();
		
		//Some needed initializations
		Scene.initialize();
		AABB2D.initialize();
		LineStrip.initialize();
		UiScreen.initialize();
		
		//Create ui mesh
		float[] uiVertices = { //y is in -1 so starting pos of the text is in the left upper corner
			1f, -1f,
			1f, 0f,
			0f, -1f,
			1f, 0f,
			0f, 0f,
			0f, -1f,
		};
		Mesh uiMesh = new Mesh("2", uiVertices, PrimitiveType.Triangles, "ui");
		
		//Init drawers
		rd = new RectDrawer2D(uiMesh);
		td = new TextureDrawer2D(uiMesh);
		
		//Create font
		fd = new BitmapFontDrawer2D(uiMesh, Texture2D.fromAssembly("res.textures.font.png", TextureParams.Default), 16, 16);
		
		//Init renderers
		uren = new UIRenderer();
		iren = new InfoRenderer();
		mmren = new MainMenuRenderer(this);
		
		//Init config
		initializeConfig();
		
		//Init screens
		sc = new Screens(this, uren);
		
		loadTextures();
		
		uren.setScreen(sc.mainMenu);
		
		highscore = highscore; //So it updates, needs to be later bc ui elements are updated as well
	}
	
	//These appear in options
	public (string key, object value, string description)[] getConfigurableOptions(){
		return new (string key, object value, string description)[]{
			("vsync", true, "Vsync"),
			("maxFps", 144f, "Maximum FPS"),
			("sound", true, "Sound"),
			("uiSound", true, "UI Sound"),
			("particles", true, "Particles"),
			("maxParticles", 1000, "Maximum particles"),
			("clock", true, "Animate turn"),
			("mainMenuMouse", true, "Move menu background with mouse")
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
		config.Set("version", BuildInfo.Version);
		try{ //Might not work on linux
			config.Set("path", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		}catch{}
		
		config.Save();
		
		loadConfig();
		loadControls();
		
		ParticleDrawer.maxParticles = config.GetValue<int>("maxParticles"); //Load only once
	}
	
	public void loadConfig(){
		setVsync(config.GetValue<bool>("vsync"));
		this.UpdateFrequency = config.GetValue<float>("maxFps");
		
		sm.isActive = config.GetValue<bool>("sound");
		onParticleActivation?.Invoke(config.GetValue<bool>("particles"));
		
		UiScreen.uiSounds = config.GetValue<bool>("uiSound");
		SceneOverlay.showClock = config.GetValue<bool>("clock");
		MainMenuRenderer.animateMouse = config.GetValue<bool>("mainMenuMouse");
	}
	
	//Reset the ones that appear in options
	public void resetConfig(){
		AshFileModel r = new AshFileModel(getConfigurableOptions().Select(h => new ModelInstance(ModelInstanceOperation.Value, h.key, h.value)).ToArray());
		config.ApplyModel(r);
		
		config.Save();
		
		loadConfig();
		
		iren.setCornerInfo("Reset config", Renderer.selectedTextColor);
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
		
		iren.setCornerInfo("Saved controls", Renderer.selectedTextColor);
	}
	
	public void resetControls(){
		config.ApplyModel(KeyBind.getResetModel());
		
		config.Save();
		
		loadControls();
		
		iren.setCornerInfo("Reset controls", Renderer.selectedTextColor);
	}
	
	//UI textures
	void loadTextures(){
		td.textureBook.Add("tick", Texture2D.fromAssembly("res.textures.ui.tick.png", TextureParams.Default)); //Used for UiCheck
		td.textureBook.Add("screenshot", Texture2D.fromAssembly("res.textures.ui.screenshotIcon.png", TextureParams.Default));
		td.textureBook.Add("next", Texture2D.fromAssembly("res.textures.ui.nextIcon.png", TextureParams.Default));
		td.textureBook.Add("previous", Texture2D.fromAssembly("res.textures.ui.previousIcon.png", TextureParams.Default));
		td.textureBook.Add("icon", Texture2D.fromAssembly("res.icon.png", TextureParams.Default));
		td.textureBook.Add("info", Texture2D.fromAssembly("res.textures.ui.infoIcon.png", TextureParams.Default));
		td.textureBook.Add("help", Texture2D.fromAssembly("res.textures.ui.helpIcon.png", TextureParams.Default));
		td.textureBook.Add("file", Texture2D.fromAssembly("res.textures.ui.fileIcon.png", TextureParams.Default));
		
		td.textureBook.Add("heart", Texture2D.fromAssembly("res.textures.ui.heart.png", TextureParams.Default));
		td.textureBook.Add("key", Texture2D.fromAssembly("res.textures.ui.key.png", TextureParams.Default));
		td.textureBook.Add("clock", Texture2D.fromAssembly("res.textures.ui.clock.png", TextureParams.Default));
	}
	
	void handleKeyboardInput(){
		//check to see if the window is focused
		if(!IsFocused){
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
			
			if(getKey.isActive(KeyboardState)){
				sce?.cheatKey();
			}
			
			switch(zoomCamera.isActiveMod(KeyboardState)){
				case 1:
				Camera2D.zoomD += 3f;
				onResize(width, height);
				break;
				
				case 2:
				Camera2D.zoomD -= 3f;
				onResize(width, height);
				break;
			}
			
			if(resetCamera.isActive(KeyboardState)){
				Camera2D.zoomD = 18.4f;
				onResize(width, height);
			}
			
			if(freeCamera.isActive(KeyboardState)){
				Camera2D.free = !Camera2D.free;
				iren.setCornerInfo("Free cam " + (Camera2D.free ? "on" : "off"));
				
				if(!Camera2D.free){
					sce.resetCam();
				}
			}
		#endif
		
		#if DEBUG_ENTITIES
			if(sce != null && instaKill.isActive(KeyboardState)){
				sce.instaKill();
			}
		#endif
		
		switch(escape.isActiveMod(KeyboardState)){
			case 1:
			if(uren.currentScreen != null && uren.currentScreen != sc.deathMenu){
				uren.closeScreen();
			}else{
				uren.setScreen(sc.pauseMenu);
			}
			
			break;
			
			case 2:
			this.Close();
			break;
		}
		
		if(screenshot.isActive(KeyboardState)){
			captureScreenshot();
			iren.setCornerInfo("Saved screenshot");
		}
		
		if(fullscreen.isActive(KeyboardState)){
			toggleFullscreen();
		}
		
		if(help.isActive(KeyboardState) && uren.currentScreen != sc.helpMenu){
			uren.setScreen(sc.helpMenu);
		}
		
		if(advancedMode.isActive(KeyboardState)){
			iren.toggleAdvancedMode();
		}
		
		//Scrolling
		if(uren.currentScreen != null){
			switch(logUp.isActiveMod(KeyboardState)){
				case 1:
					uren.currentScreen.scroll(-20f * (float) dh.deltaTime);
					break;
				
				case 2:
					uren.currentScreen.scroll(-60f * (float) dh.deltaTime);
					break;
				
				default:
					switch(logDown.isActiveMod(KeyboardState)){
						case 1:
							uren.currentScreen.scroll(20f * (float) dh.deltaTime);
							break;
						
						case 2:
							uren.currentScreen.scroll(60f * (float) dh.deltaTime);
							break;
					}
					break;
				
			}
			
			//Enter to play
			if(enter.isActive(KeyboardState) && (uren.currentScreen == sc.mainMenu || uren.currentScreen == sc.deathMenu)){
				setNewScene();
			}
			
			return;
		}
		
		#if DEBUG_LEVELS
			if(Camera2D.free && sce != null){
				Vector2 tot = Vector2.Zero;
				if(moveUp.isActive(KeyboardState)){
					tot += new Vector2(0f, 1f);
				}
				if(moveDown.isActive(KeyboardState)){
					tot += new Vector2(0f, -1f);
				}
				if(moveLeft.isActive(KeyboardState)){
					tot += new Vector2(-1f, 0f);
				}
				if(moveRight.isActive(KeyboardState)){
					tot += new Vector2(1f, 0f);
				}
				if(tot != Vector2.Zero){
					tot = tot.Normalized();
					tot *= 20f;
					tot *= (float) dh.deltaTime;
					sce.cam.moveFast(-sce.cam.position + tot);
				}
				
				if(interact.isActive(KeyboardState)){
					sce.skipTime();
				}
			}else
		#endif
		if(sce?.canMovePlayer == true){
			if(one.isActive(KeyboardState) && sce.interact(1)){
				
			}else if(two.isActive(KeyboardState) && sce.interact(2)){
				
			}else if(three.isActive(KeyboardState) && sce.interact(3)){
				
			}else if(four.isActive(KeyboardState) && sce.interact(4)){
				
			}else if(five.isActive(KeyboardState) && sce.interact(5)){
				
			}else if(six.isActive(KeyboardState) && sce.interact(6)){
				
			}else if(seven.isActive(KeyboardState) && sce.interact(7)){
				
			}else if(eight.isActive(KeyboardState) && sce.interact(8)){
				
			}else if(nine.isActive(KeyboardState) && sce.interact(9)){
				
			}else if(interact.isActive(KeyboardState) && sce.interact()){
				
			}else if(moveUp.isActive(KeyboardState) && sce.movePlayerUp()){
				
			}else if(moveDown.isActive(KeyboardState) && sce.movePlayerDown()){
				
			}else if(moveLeft.isActive(KeyboardState) && sce.movePlayerLeft()){
				
			}else if(moveRight.isActive(KeyboardState) && sce.movePlayerRight()){
				
			}else if(skipTime.isActive(KeyboardState)){
				sce.skipTime();
			}
		}
		
		if(sce != null && seeInterctions.isActive(KeyboardState)){
			sce.showInteractions = true;
		}
	}
	
	void draw(){
		onFrameStart?.Invoke();
		
		//Clear
		GL.ClearColor(Renderer.backgroundColor);
		GL.Clear(ClearBufferMask.ColorBufferBit);
		
		//Draw
		if(sce != null){
			sce.draw();
		}else{
			mmren.draw();
		}
		
		uren.draw();
		iren.draw();
		
		//Swap buffers
		Context.SwapBuffers();
		
		//Handle errors and resources
		checkErrors();
		disposeOfResources();
		
		if(takeScreenshotNextFrame){
			takeScreenshotNextFrame = false;
			captureScreenshot();
			iren.setCornerInfo("Saved screenshot");
		}
	}
	
	public void setNewScene(){
		sce?.Dispose();
		sce = new Scene(sm);
		uren.setScreen(null);
	}
	
	public void closeScene(){
		//ren.screenAlpha = 0.6f; //Reset
		sce?.Dispose();
		sce = null;
		uren.setScreen(null);
		uren.setScreen(sc.mainMenu);
	}
	
	void setVsync(bool b){
		this.VSync = b ? VSyncMode.On : VSyncMode.Off;
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
		#if !DEBUG
			OpenTK.Graphics.OpenGL.ErrorCode errorCode = GL.GetError();
			while(errorCode != OpenTK.Graphics.OpenGL.ErrorCode.NoError){
				Console.Error.WriteLine("[OpenGL Error] " + errorCode);
				iren?.setCornerInfo("[OpenGL Error] " + errorCode, Renderer.redTextColor);
				
				errorCode = GL.GetError();
			}
		#endif
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
			
			iren?.setCornerInfo("[GL Error] " + source, Renderer.redTextColor);
		}
	#endif
	#endregion
	
	void disposeOfResources(){
		while(resourcesMarkedForDisposal.TryDequeue(out IDisposable r)){
			r.Dispose();
		}
	}
	
	protected override void OnKeyDown(KeyboardKeyEventArgs e){
		if(!uren.handleKeyDown(e)){
			base.OnKeyDown(e);
		}
	}
	
	protected override void OnTextInput(TextInputEventArgs e){
		uren.handleTextInput(e.AsString);
		
		base.OnTextInput(e);
	}
	
	protected override void OnLoad(){
		initialize();
		base.OnLoad();
	}
	
	protected override void OnUnload(){
		disposeOfResources();
		uren.Dispose();
		iren.Dispose();
		sce?.Dispose();
		mmren.Dispose();
		
		base.OnUnload();
	}
	
	protected override void OnResize(ResizeEventArgs args){
		GL.Viewport(0, 0, width, height);
		
		onResize?.Invoke(width, height);
		
		base.OnResize(args);
	}
	
	protected override void OnUpdateFrame(FrameEventArgs args){
		handleKeyboardInput();
		onFrameEnd?.Invoke();
		
		base.OnUpdateFrame(args);
	}
	
	protected override void OnRenderFrame(FrameEventArgs args){
		draw();
		
		base.OnRenderFrame(args);
		dh.Frame();
	}
	
	protected override void OnMouseWheel(MouseWheelEventArgs args){
		if(!uren.handleMouseScroll(args.OffsetY)){
			onMouseScroll?.Invoke(args.OffsetY);
		}
        
		base.OnMouseWheel(args);
    }
	
	protected override void OnMouseMove(MouseMoveEventArgs e){
		mousePosition = new Vector2(e.X, e.Y);
		onMouseMove?.Invoke(e.X, e.Y);
		base.OnMouseMove(e);
    }
	
	protected override void OnMouseDown(MouseButtonEventArgs e){
        if(e.Button == MouseButton.Left){
			if(!(uren.currentScreen?.click(uren, KeyboardState.IsKeyDown(Keys.LeftShift)) ?? false)){
				sce?.click();
			}
        }
		
		base.OnMouseDown(e);
    }
	
	#region static
	public static Engine instance;
	
	static GLFWCallbacks.ErrorCallback GLFWErrorCallback;
	
	static void Main(string[] args){
		#if WINDOWS
			if(GetConsoleWindow() == IntPtr.Zero){
				AttachConsole(ATTACH_PARENT_PROCESS);
			}
		#endif
		
		GLFWErrorCallback = OnGLFWError;
		
		GLFWProvider.SetErrorCallback(GLFWErrorCallback);
		
		
		using(instance = new Engine(new NativeWindowSettings{
			Title = "InfDun - v" + BuildInfo.Version,
			Vsync = VSyncMode.On,
			ClientSize = new Vector2i(640, 480),
			Icon = getIcon(),
			#if DEBUG
				Flags = ContextFlags.Debug
			#endif
		})){
			instance.Run();
		}
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
	
	#if WINDOWS		
		[DllImport("kernel32.dll")]
		static extern bool AttachConsole(int dwProcessId);
		const int ATTACH_PARENT_PROCESS = -1;
		
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
	#endif
	
	#region errors
	private static void OnGLFWError(OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode error, string description){
        Console.Error.WriteLine("[GLFW Error] " + error + ": " + description);
    }
	#endregion
	#endregion
}