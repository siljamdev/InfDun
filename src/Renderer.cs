using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class Renderer{
	#region static
	static readonly Color4 backgroundColor = new Color4(0f, 0f, 0f, 1f);
	
	public static readonly Vector2 textSize = new Vector2(15f, 18f);
	public static readonly Vector2 middleTextSize = new Vector2(22f, 28f);
	public static readonly Vector2 bigTextSize = new Vector2(28f, 35f);
	
	public const float separation = 40f;
	public const float fieldSeparation = 30f;
	
	public static readonly Color3 textColor = new Color3("EFEFEF");
	public static readonly Color3 selectedTextColor = new Color3("FFFF8F");
	public static readonly Color3 titleTextColor = new Color3("DFDFFF");
	public static readonly Color3 fieldTextColor = new Color3("D6D6D6");
	public static readonly Color3 redTextColor = new Color3("FF8888");
	
	public static readonly Color3 buttonColor = new Color3("555577");
	public static readonly Color3 greenButtonColor = new Color3("557755");
	public static readonly Color3 redButtonColor = new Color3("775555");
	public static readonly Color3 fieldColor = new Color3("222222");
	public static readonly Color3 fieldSelectedColor = new Color3("505050");
	
	public static readonly Color3 black = Color3.Black;
	#endregion
	
	public int width{get; private set;}
	public int height{get; private set;}
	
	public Camera cam{get; private set;}
	public FontRenderer fr{get; private set;}
	public ParticleRenderer uipr{get; private set;} //UI particle renderer
	
	public Mesh uiMesh{get; private set;}
	
	public Shader uiShader{get; private set;}
	public Shader rectShader{get; private set;}
	
	MainMenuRenderer mmren;
	
	public Matrix4 projection{get; private set;}
	
	public UiScreen overlayScreen;
	public UiScreen currentScreen;
	
	public Dungeon dun;
	
	Dictionary<string, Texture2D> uiTexturesBook = new();
	
	public Stack<UiScreen> screens{get; private set;}
	
	Stopwatch sw;
	string corner;
	Color3 cornerColor;
	
	Texture2D clock;
	Matrix4 clockLeft = Matrix4.CreateScale(new Vector3(45f, 45f, 0f)) * Matrix4.CreateTranslation(new Vector3(-22.5f, 22.5f, 0f));
	Matrix4 clockRight;
	
	public bool showClock;
	
	float ramMB = 0f;
	float ramUpdateCounter = 0f;
	
	bool advancedMode;
	
	//Used to animate death
	public float screenAlpha = 0.6f;
	
	public Renderer(Dungeon st){
		dun = st;
		
		width = st.ClientSize.X;
		height = st.ClientSize.Y;
		
		//Enable transparency (blending)
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		
		//other utilities
		cam = new Camera(this);
		cam.onViewChange += AABB.setView;
		cam.onViewChange += LineStrip.setView;
		
		sw = new Stopwatch();
		
		clockRight = Matrix4.CreateTranslation(new Vector3(22.5f, -22.5f, 0f)) * Matrix4.CreateTranslation(new Vector3(-width/2f, -height/2f + 45f, 0f));
		
		screens = new Stack<UiScreen>();
		
		float[] vertices = { //y is in -1 so starting pos of the text is in the left upper corner
			1f, -1f,
			1f, 0f,
			0f, -1f,
			1f, 0f,
			0f, 0f,
			0f, -1f,
		};
		
		uiMesh = new Mesh("2", vertices, PrimitiveType.Triangles, "ui");
		
		uiShader = Shader.fromAssembly("shaders.ui");
		rectShader = Shader.fromAssembly("shaders.rect");
		
		mmren = new MainMenuRenderer();
		
		//Bitmap font
		fr = new BitmapFontRenderer(uiMesh, Texture2D.fromAssembly("res.textures.font.png", TextureParams.Default), 16, 16);
		
		uipr = new ParticleRenderer();
		
		//load ui textures
		addTexture("tick", Texture2D.fromAssembly("res.textures.ui.tick.png", TextureParams.Default));
		addTexture("screenshot", Texture2D.fromAssembly("res.textures.ui.screenshotIcon.png", TextureParams.Default));
		addTexture("next", Texture2D.fromAssembly("res.textures.ui.nextIcon.png", TextureParams.Default));
		addTexture("previous", Texture2D.fromAssembly("res.textures.ui.previousIcon.png", TextureParams.Default));
		addTexture("icon", Texture2D.fromAssembly("res.icon.png", TextureParams.Default));
		addTexture("info", Texture2D.fromAssembly("res.textures.ui.infoIcon.png", TextureParams.Default));
		addTexture("help", Texture2D.fromAssembly("res.textures.ui.helpIcon.png", TextureParams.Default));
		addTexture("file", Texture2D.fromAssembly("res.textures.ui.fileIcon.png", TextureParams.Default));
		
		addTexture("heart", Texture2D.fromAssembly("res.textures.ui.heart.png", TextureParams.Default));
		
		clock = Texture2D.fromAssembly("res.textures.ui.clock.png", TextureParams.Default);
		
		overlayScreen = new UiScreen(
			new UiImage(Placement.TopLeft, 0f, 0f, 40, 40, "heart").setColor(new Color3("AA0000"))
		);
		
		overlayScreen.updateProj(this);
	}
	
	public void setScreen(UiScreen s){
		if(currentScreen != null){
			currentScreen.close(this);
			screens.Push(currentScreen);
		}
		
		if(s == null){
			screens.Clear();
		}
		
		currentScreen = s;
		currentScreen?.updateProj(this);
	}
	
	public void closeScreen(){
		if(dun.sce == null && screens.Count == 0){ //Not close main menu
			return;
		}
		
		if(screens.Count == 0){
			currentScreen?.close(this);
			currentScreen = null;
			return;
		}
		
		currentScreen?.close(this);
		currentScreen = screens.Pop();
		currentScreen?.updateProj(this);
	}
	
	public void setCornerInfo(string s, Color3 c){
		corner = s;
		cornerColor = c;
		sw.Restart();
		
		Console.WriteLine(s);
	}
	
	public void setCornerInfo(string s){
		corner = s;
		cornerColor = textColor;
		sw.Restart();
		
		Console.WriteLine(s);
	}
	
	public void updateSize(int w, int h){
		width = w;
		height = h;
		projection = Matrix4.CreateOrthographic((float) width, (float) height, -1.0f, 1.0f);
		
		uiShader.setMatrix4("projection", projection);
		rectShader.setMatrix4("projection", projection);
		
		mmren.updateSize(w, h);
		
		cam.updateSize(width, height);
		
		fr.setProjection(projection);
		dun.sce?.setProjection(projection);
		AABB.setProjection(projection);
		LineStrip.setProjection(projection);
		
		overlayScreen.updateProj(this);
		currentScreen?.updateProj(this);
		
		clockRight = Matrix4.CreateTranslation(new Vector3(22.5f, -22.5f, 0f)) * Matrix4.CreateTranslation(new Vector3(-width/2f, -height/2f + 45f, 0f));
	}
	
	//Helper method
	public void addTexture(string n, Texture2D t){
		uiTexturesBook.Add(n, t);
	}
	
	public void toggleAdvancedMode(){
		advancedMode = !advancedMode;
		
		if(advancedMode){
			setCornerInfo("Advanced mode enabled");
		}else{
			setCornerInfo("Advanced mode diabled");
		}
	}
	
	//Solid rect
	public void drawRect(Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		Matrix4 model = Matrix4.CreateScale(new Vector3(sca.X, sca.Y, 0f)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		rectShader.use();
		rectShader.setMatrix4("model", model);
		rectShader.setMatrix4("view", Matrix4.Identity); //These are for local rects
		rectShader.setVector4("col", col, alpha);
		
		uiMesh.draw();
	}
	
	public void drawRect(float xp, float xy, float sx, float sy, Color3 c, float a = 1f){
		drawRect(new Vector2(xp, xy), new Vector2(sx, sy), c, a);
	}
	
	public void drawTexture(string n, Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		if(!uiTexturesBook.ContainsKey(n)){
			return;
		}
		
		Matrix4 model = Matrix4.CreateScale(new Vector3(sca.X, sca.Y, 0f)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		uiShader.use();
		uiShader.setMatrix4("model", model);
		uiShader.setVector4("col", col, alpha);
		
		uiTexturesBook[n].bind();
		
		uiMesh.draw();
	}
	
	public void drawTexture(string n, float xpos, float ypos, float xsca, float ysca, Color3 col, float alpha = 1f){
		drawTexture(n, new Vector2(xpos, ypos), new Vector2(xsca, ysca), col, alpha);
	}
	
	public void drawTexture(string n, float xpos, float ypos, float sca, Color3 col, float alpha = 1f){
		drawTexture(n, new Vector2(xpos, ypos), new Vector2(sca, sca), col, alpha);
	}
	
	void drawClock(){
		Matrix4 rot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float) (360d * dun.sce.smoothTime / dun.sce.smoothTimeMax)));
		
		Matrix4 model = clockLeft * rot * clockRight;
		
		uiShader.use();
		uiShader.setMatrix4("model", model);
		uiShader.setVector4("col", Color3.White, 0.6f);
		
		clock.bind();
		
		uiMesh.draw();
	}
	
	//General draw loop
	public void draw(){
		cam.startFrame();
		
		GL.ClearColor(backgroundColor);
		GL.Clear(ClearBufferMask.ColorBufferBit);
		
		//Render scene or main menu background
		if(dun.sce != null){
			dun.sce.draw(this);
		}else{
			mmren.draw();
		}
		
		//Render ui
		if(dun.sce != null){
			if(dun.sce.doingSmoothing && showClock){
				drawClock();
			}
			
			if(currentScreen != null){
				overlayScreen.draw(this, false);
				
				drawRect(-width/2f, height/2f, width, height, Color3.Black, screenAlpha);
				currentScreen.draw(this, true);
			}else{
				overlayScreen.draw(this, true);
			}
		}else{			
			currentScreen?.draw(this, true);
		}
		
		uipr.draw(this); //Render particles
		
		//Render corner info
		if(corner != null){
			if(sw.Elapsed.TotalSeconds < 4d){
				fr.drawText(corner, -width/2f, height/2f - 40f, textSize, cornerColor);
			}else if(sw.Elapsed.TotalSeconds < 6d){
				fr.drawText(corner, -width/2f, height/2f - 40f, textSize, cornerColor, (float) (6d - sw.Elapsed.TotalSeconds));
			}else{
				corner = null;
				sw.Stop();
			}
		}
		
		//Render advanced info
		if(advancedMode){
			string s = "FPS: " + Dungeon.dh.stableFps.ToString("F0");
			fr.drawText(s, width/2f, height/2f, textSize, Placement.TopRight, textColor);
			
			ramUpdateCounter += (float) Dungeon.dh.deltaTime;
			if(ramUpdateCounter > 1f){
				ramUpdateCounter = 0f;
				ramMB = Process.GetCurrentProcess().PrivateMemorySize64 / (1024f * 1024f);
			}
			
			s = "Memory: " + ramMB.ToString("F2") + " MB";
			fr.drawText(s, width/2f, (height/2f) - textSize.Y, textSize, Placement.TopRight, textColor);
			
			string format = "F1";
			
			s = "Pos: (" + (-cam.position.X).ToString(format) + ", " + (-cam.position.Y).ToString(format) + ")";
			fr.drawText(s, width/2f, (height/2f) - 2f * textSize.Y, textSize, Placement.TopRight, textColor);
			
			s = "Cursor: " + cam.mouseWorldPos.ToString(format);
			fr.drawText(s, width/2f, (height/2f) - 3f * textSize.Y, textSize, Placement.TopRight, textColor);
		}
	}
}