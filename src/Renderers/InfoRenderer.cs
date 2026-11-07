using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class InfoRenderer : Renderer{
	Stopwatch sw;
	string corner;
	Color3 cornerColor;
	
	float ramMB = 0f;
	float ramUpdateCounter = 0f;
	
	bool advancedMode;
	
	public InfoRenderer() : base(){
		view = Matrix4.Identity; //Never changes
		zoom = 1f; //Never changes
		
		sw = new Stopwatch();
	}
	
	public void setCornerInfo(string s, Color3 c){
		corner = s;
		cornerColor = c;
		sw.Restart();
	}
	
	public void setCornerInfo(string s){
		corner = s;
		cornerColor = Renderer.textColor;
		sw.Restart();
	}
	
	public void toggleAdvancedMode(){
		advancedMode = !advancedMode;
		
		if(advancedMode){
			setCornerInfo("Advanced mode enabled");
		}else{
			setCornerInfo("Advanced mode diabled");
		}
	}
	
	public override void draw(){
		//Render corner info
		if(corner != null){
			if(sw.Elapsed.TotalSeconds < 4d){
				fd.draw(this, corner, -width/2f, height/2f - 85f, Renderer.textSize, cornerColor);
			}else if(sw.Elapsed.TotalSeconds < 6d){
				fd.draw(this, corner, -width/2f, height/2f - 85f, Renderer.textSize, cornerColor, (float) (6d - sw.Elapsed.TotalSeconds));
			}else{
				corner = null;
				sw.Stop();
			}
		}
		
		//Render advanced info
		if(advancedMode){
			string s = "FPS: " + Engine.instance.dh.stableFps.ToString("F0");
			fd.draw(this, s, width/2f, height/2f, Renderer.textSize, Placement.TopRight, Renderer.textColor);
			
			ramUpdateCounter += (float) Engine.instance.dh.deltaTime;
			if(ramUpdateCounter > 1f){
				ramUpdateCounter = 0f;
				ramMB = Process.GetCurrentProcess().PrivateMemorySize64 / (1024f * 1024f);
			}
			
			s = "Memory: " + ramMB.ToString("F2") + " MB";
			fd.draw(this, s, width/2f, (height/2f) - Renderer.textSize.Y, Renderer.textSize, Placement.TopRight, Renderer.textColor);
			
			if(Engine.instance.sce != null){
				Camera2D cam = Engine.instance.sce.cam;
				string format = "F1";
				
				s = "Pos: (" + (-cam.position.X).ToString(format) + ", " + (-cam.position.Y).ToString(format) + ")";
				fd.draw(this, s, width/2f, (height/2f) - 2f * Renderer.textSize.Y, Renderer.textSize, Placement.TopRight, Renderer.textColor);
				
				s = "Cursor: " + cam.mouseWorldPosition.ToString(format);
				fd.draw(this, s, width/2f, (height/2f) - 3f * Renderer.textSize.Y, Renderer.textSize, Placement.TopRight, Renderer.textColor);
			}
		}
	}
	
	protected override void onResize(int width, int height){
		projection = Matrix4.CreateOrthographic((float) width, (float) height, -1.0f, 1.0f);
	}
}