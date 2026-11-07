using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Camera2D{
	#if DEBUG_LEVELS
		public static float zoomD = 18.4f;
		public static bool free = false;
	#endif
	
	const double smoothMove = 0.32d; //in s
	
	public Vector2 position;
	public float zoom{get; private set;}
	public Vector2 mouseWorldPosition {get; private set;}
	
	Matrix4 scale;
	
	public event Action<Matrix4, float> onViewChange;
	
	Vector2? moveTo = null;
	Vector2 oldPosition;
	double moveToTime; //in s
	
	public Camera2D(int w, int h){
		position = Vector2.Zero;
		
		Engine.instance.onMouseMove += mouse;
		Engine.instance.onFrameStart += startFrame;
		Engine.instance.onResize += updateSize;
		
		mouse(Engine.instance.mousePosition.X, Engine.instance.mousePosition.Y); //First update
		updateSize(w, h); //Updates matrix
	}
	
	public void reset(){
		position = Vector2.Zero;
		updateMatrix();
	}
	
	void updateMatrix(){
		Matrix4 view = Matrix4.CreateTranslation(new Vector3(position.X, position.Y, 0.0f)) * scale;
		
		onViewChange?.Invoke(view, zoom);
	}
	
	public void updateForce(){
		updateMatrix();
	}
	
	public void moveFast(Vector2 p){
		mouseWorldPosition += p + position;
		position = -p;
		updateMatrix();
	}
	
	public void move(Vector2 p){
		oldPosition = position;
		moveTo = -p;
		moveToTime = 0d;
	}
	
	public void updateSize(int w, int h){
		#if DEBUG_LEVELS
			zoom = h / zoomD;
		#else
			zoom = h / 18.4f;
		#endif
		
		scale = Matrix4.CreateScale(zoom);
		
		updateMatrix();
	}
	
	public void startFrame(){
		if(moveTo != null){
			moveToTime += Engine.instance.dh.deltaTime;
			if(moveToTime > smoothMove){
				position = (Vector2) moveTo;
				updateMouse();
				moveTo = null;
				updateMatrix();
			}else{
				Vector2 inc = (float) (moveToTime / smoothMove) * ((Vector2) moveTo - oldPosition);
				position = oldPosition + inc;
				updateMouse();
				updateMatrix();
			}
		}
	}
	
	void updateMouse(){
		mouse(Engine.instance.mousePosition.X, Engine.instance.mousePosition.Y);
	}
	
	public void mouse(float x, float y){
		mouseWorldPosition = new Vector2(x - Engine.instance.width / 2f, Engine.instance.height / 2f - y) / zoom - position;
	}
	
	public void Dispose(){
		onViewChange = null; //Clear all
		
		Engine.instance.onMouseMove -= mouse;
		Engine.instance.onFrameStart -= startFrame;
		Engine.instance.onResize -= updateSize;
	}
}