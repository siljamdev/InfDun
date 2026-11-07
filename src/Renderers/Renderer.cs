using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

abstract class Renderer : IDisposable{
	
	public Matrix4 projection {get; protected set;}
	public Matrix4 view {get; protected set;}
	public float zoom {get; protected set;}
	
	//Useful to avoid writing Engine.instance...
	protected static RectDrawer2D rd => Engine.instance.rd;
	protected static TextureDrawer2D td => Engine.instance.td;
	protected static FontDrawer2D fd => Engine.instance.fd;
	protected static int width => Engine.instance.width;
	protected static int height => Engine.instance.height;
	
	protected Renderer(){
		Engine.instance.onResize += onResize;
		onResize(Engine.instance.width, Engine.instance.height);
	}
	
	protected virtual void onResize(int width, int height){
		
	}
	
	public abstract void draw();
	
	public virtual void Dispose(){
		Engine.instance.onResize -= onResize;
	}

	#region static
	//UsefulConstants to keep uniformity
	public static readonly Color4 backgroundColor = new Color4(0f, 0f, 0f, 1f);
	
	public static readonly Vector2 textSize = new Vector2(15f, 18f);
	public static readonly Vector2 middleTextSize = new Vector2(22f, 28f);
	public static readonly Vector2 bigTextSize = new Vector2(28f, 35f);
	public static readonly Vector2 sceneTextSize = new Vector2(0.3f, 0.36f);
	
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
	
	public static readonly Color3 highscoreColor = new Color3("FFFF70");
	
	public static readonly Color3 black = Color3.Black;
	#endregion
}