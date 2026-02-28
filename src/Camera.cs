using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Camera{
	
	Renderer ren;
	
	const double smoothMove = 0.32d; //in s
	
	public Vector2d position;
	public Vector2d mouseLastPos{get; private set;}
	
	Vector2d? moveTo = null;
	Vector2d oldPosition;
	double moveToTime; //in s
	
	public Vector2d mouseWorldPos{get{
		Vector2d m = mouseLastPos;
		m.X -= ren.width/2f;
		
		m.Y -= ren.height/2f;
		m.Y = -m.Y;
		
		m /= zoom;
		m -= position;
		return m;
	}}
	
	public float zoom{get; private set;}
	
	Matrix4 scale;
	
	public Matrix4 view{get; private set;}
	
	public event EventHandler onViewChange;
	
	public Camera(Renderer r){
		ren = r;
		position = Vector2d.Zero;
		updateSize(r.width, r.height);
		updateMatrix();
	}
	
	public void reset(){
		position = Vector2d.Zero;
		updateMatrix();
	}
	
	public void moveFast(Vector2d p){
		position = -p;
		updateMatrix();
	}
	
	public void move(Vector2d p){
		oldPosition = position;
		moveTo = -p;
		moveToTime = 0d;
	}
	
	public void updateSize(int w, int h){
		#if DEBUG_ZOOM
		zoom = h / 100f;
		#else
		zoom = h / 18.4f;
		#endif
		
		scale = Matrix4.CreateScale(zoom);
		
		updateMatrix();
	}
	
	void updateMatrix(){
		view = Matrix4.CreateTranslation(new Vector3((float) position.X, (float) position.Y, 0.0f)) * scale;
		
		onViewChange?.Invoke(this, EventArgs.Empty);
	}
	
	public void startFrame(){
		if(moveTo != null){
			moveToTime += Dungeon.dh.deltaTime;
			if(moveToTime > smoothMove){
				position = (Vector2d) moveTo;
				moveTo = null;
				updateMatrix();
			}else{
				position = oldPosition + (moveToTime / smoothMove) * ((Vector2d) moveTo - oldPosition);
				updateMatrix();
			}
		}
	}
	
	public void endFrame(){
		
	}
	
	public void mouse(float x, float y){
		mouseLastPos = new Vector2d(x, y);
	}
}