using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
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
	
	public const string version = "0.1.2";
	
	#region static
	public static List<(int, int?, int?)> meshesMarkedForDisposal = new();
	public static List<int> texturesMarkedForDisposal = new();
	
	public static DeltaHelper dh;
	
	static GLFWCallbacks.ErrorCallback GLFWErrorCallback;
	
	static void Main(string[] args){
		if(OperatingSystem.IsWindows()){
			if(GetConsoleWindow() == IntPtr.Zero){
				AttachConsole(ATTACH_PARENT_PROCESS);
			}
		}
		
		GLFWErrorCallback = OnGLFWError;
		
		GLFWProvider.SetErrorCallback(GLFWErrorCallback);
		
		#if DEBUG
			using(Dungeon dun = new Dungeon(new NativeWindowSettings{
				Title = "Dungeon Game - BETA",
				Vsync = VSyncMode.On,
				ClientSize = new Vector2i(640, 480),
				Icon = getIcon(),
				Flags = ContextFlags.Debug
			})){
				dun.Run();
			}
		#else
			using(Dungeon dun = new Dungeon(new NativeWindowSettings{
				Title = "Dungeon Game - BETA",
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
	
	KeyBind fullscreen = new KeyBind(Keys.F11, false);
	KeyBind screenshot = new KeyBind(Keys.F2, false);
	
	KeyBind advancedMode = new KeyBind(Keys.LeftAlt, false);
	
	KeyBind moveUp = new KeyBind(Keys.W, true);
	KeyBind moveDown = new KeyBind(Keys.S, true);
	KeyBind moveLeft = new KeyBind(Keys.A, true);
	KeyBind moveRight = new KeyBind(Keys.D, true);
	
	//These are static
	KeyBind escape = new KeyBind(Keys.Escape, Keys.LeftShift, false);
	KeyBind help = new KeyBind(Keys.F1, false);
	KeyBind logUp = new KeyBind(Keys.Up, true);
	KeyBind logDown = new KeyBind(Keys.Down, true);
	KeyBind enter = new KeyBind(Keys.Enter, true);
	
	#if DEBUG_TIME
		KeyBind debug = new KeyBind(Keys.M, false);
	#endif
	
	public Dependencies dep;
	public AshFile config;
	
	bool takeScreenshotNextTick;
	
	Renderer ren;
	
	public Scene sce;
	
	public SoundManager sm;
	
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
	
	bool isFullscreened;
	
	float maxFps = 144f;
	
	#if DEBUG
		DebugProc DebugMessageDelegate;
	#endif
	
	//new NativeWindowSettings{NumberOfSamples = 4}
	Dungeon(NativeWindowSettings n) : base(GameWindowSettings.Default, n){
		CenterWindow();
	}
	
	void initialize(){
		dh = new DeltaHelper();
		dh.Start();
		
		string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		dep = new Dependencies(appDataPath + "/ashproject/infdun", true, new string[]{"screenshots"}, null);
		
		//Before config bc config modifies it
		sm = new SoundManager();
		
		initializeConfig();
		
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
		
		//Init statics, resources
		Scene.initialize();
		AABB.initialize();
		LineStrip.initialize();
		
		initializeScreens();
		
		highscore = highscore; //So it updates, needs to be later bc ui elements are updated as well
		
		ren.setScreen(mainMenu);
	}
	
	void onResize(int x, int y){
		GL.Viewport(0, 0, x, y);
		ren?.updateSize(x, y);
	}
	
	void initializeConfig(){
		int[] k = new List<Keys>{
			fullscreen.key,
			screenshot.key,
			advancedMode.key,
			moveUp.key,
			moveDown.key,
			moveLeft.key,
			moveRight.key
		}.Select(n => (int) n).ToArray();
		
		AshFileModel afm = new AshFileModel(
			new ModelInstance(ModelInstanceOperation.Type, "vsync", true),
			new ModelInstance(ModelInstanceOperation.Type, "maxFps", 144f),
			new ModelInstance(ModelInstanceOperation.Type, "controls", k),
			new ModelInstance(ModelInstanceOperation.Type, "sound", true),
			new ModelInstance(ModelInstanceOperation.Type, "particles", true),
			new ModelInstance(ModelInstanceOperation.Type, "highscore", 0)
		);
		
		config = dep.config;
		
		afm.deleteNotMentioned = true;
		
		config *= afm;
		
		//Set current version and path. Might be needed by someone (maybe)
		config.Set("version", version);
		try{ //Might not work on linux
			config.Set("path", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		}catch{}
		
		config.Save();
		
		setVsync(config.GetValue<bool>("vsync"));
		maxFps = config.GetValue<float>("maxFps");
		
		int[] ka = config.GetValue<int[]>("controls");
		if(ka.Length > 6){
			fullscreen.key = (Keys)ka[0];
			screenshot.key = (Keys)ka[1];
			advancedMode.key = (Keys)ka[2];
			moveUp.key = (Keys)ka[3];
			moveDown.key = (Keys)ka[4];
			moveLeft.key = (Keys)ka[5];
			moveRight.key = (Keys)ka[6];
		}
		
		sm.isActive = config.GetValue<bool>("sound");
		ParticleRenderer.isActive = config.GetValue<bool>("particles");
	}
	
	public void setVsync(bool b){
		if(b){
			VSync = VSyncMode.On;
		}else{
			VSync = VSyncMode.Off;
		}
	}
	
	void handleKeyboardInput(){
		// check to see if the window is focused
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
		
		switch(escape.isActiveMod(KeyboardState)){
			case 1:
			if(ren.currentScreen != null && ren.currentScreen != deathMenu){
				ren.closeScreen();
			}else{
				ren.setScreen(pauseMenu);
			}
			
			break;
			
			case 2:
			Close();
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
			if(logUp.isActive(KeyboardState)){
				ren.currentScreen.scroll(40f * (float) dh.deltaTime);
			}else if(logDown.isActive(KeyboardState)){
				ren.currentScreen.scroll(-40f * (float) dh.deltaTime);
			}
			
			if(enter.isActive(KeyboardState) && (ren.currentScreen == mainMenu || ren.currentScreen == deathMenu)){
				setNewScene();
			}
			
			sce?.endFrame();
			ren.cam.endFrame();
			return;
		}
		
		if(sce?.canMovePlayer == true){
			if(moveUp.isActive(KeyboardState) && sce.movePlayerUp()){
				
			}else if(moveDown.isActive(KeyboardState) && sce.movePlayerDown()){
				
			}else if(moveLeft.isActive(KeyboardState) && sce.movePlayerLeft()){
				
			}else if(moveRight.isActive(KeyboardState) && sce.movePlayerRight()){
				
			}
		}
		
		sce?.endFrame();
		ren.cam.endFrame();
	}
	
	void setNewScene(){
		sce = new Scene(ren, sm);
		ren.setScreen(null);
	}
	
	void closeScene(){
		ren.screenAlpha = 0.6f; //Reset
		sce?.endLife();
		sce = null;
		ren.setScreen(null);
		ren.setScreen(mainMenu);
	}
	
	void toggleFullscreen(){
		VSyncMode t = VSync;
		if(!isFullscreened){
			MonitorInfo mi = Monitors.GetMonitorFromWindow(this);
			WindowState = WindowState.Fullscreen;
			this.CurrentMonitor = mi;
			isFullscreened = true;
			VSync = t;
		}else{
			WindowState = WindowState.Normal;
			isFullscreened = false;
			VSync = t;
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
			//Console.Error.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
			
			if(ren != null){
				ren.setCornerInfo("[OpenGL Error] " + source, Renderer.redTextColor);
			}
		}
	#endif
	#endregion
	
	void dispose(){
		foreach((int VAO, int? VBO, int? EBO) in meshesMarkedForDisposal){		
			Mesh.cleanup(VAO, VBO, EBO);
		}
		meshesMarkedForDisposal.Clear();
		
		foreach(int tid in texturesMarkedForDisposal){		
			Texture2D.cleanup(tid);
		}
		texturesMarkedForDisposal.Clear();
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
		using (var stream = File.OpenWrite(dep.path + "/screenshots/" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".png")){
			writer.WritePng(rgbPixels, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlue, stream);
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
		dispose();
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
		dispose();
		if(takeScreenshotNextTick){
			captureScreenshot();
			ren.setCornerInfo("Saved screenshot");
			takeScreenshotNextTick = false;
		}
		base.OnRenderFrame(args);
		dh.Frame();
		if(VSync != VSyncMode.On){
			dh.Target(maxFps);
		}
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
	
	#region WINDOWS		
	[DllImport("kernel32.dll")]
	static extern bool AttachConsole(int dwProcessId);
	const int ATTACH_PARENT_PROCESS = -1;
	
	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();
	#endregion
}