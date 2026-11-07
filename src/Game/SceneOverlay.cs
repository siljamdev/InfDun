using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class SceneOverlay : Renderer{
	public static bool showClock;
	
	Matrix4 clockLeft = Matrix4.CreateScale(new Vector3(45f, 45f, 0f)) * Matrix4.CreateTranslation(new Vector3(-22.5f, 22.5f, 0f));
	Matrix4 clockRight;
	
	Scene sce;
	
	//Used to animate death
	public float screenAlpha = 0.6f;
	
	public SceneOverlay(Scene s) : base(){
		sce = s;
		
		view = Matrix4.Identity; //Never changes
		zoom = 1f; //Never changes
		
		clockRight = Matrix4.CreateTranslation(new Vector3(22.5f, -22.5f, 0f)) * Matrix4.CreateTranslation(new Vector3(-width/2f, -height/2f + 45f, 0f));
	}
	
	public override void draw(){
		//Draw descent
		if(sce.doingDescent){
			float factor = (float) (sce.descentTime/Scene.descentTimeMax);
			float alpha = -4f * factor * factor + 4f * factor;
			rd.draw(this, -width/2f, height/2f, width, height, Color3.Black, alpha);
			
			string temp3 = "Level " + sce.levelNum;
			fd.draw(this, temp3, -temp3.Length / 2 * Renderer.bigTextSize.X, 0f, Renderer.bigTextSize, Renderer.textColor, alpha);
		}
		
		//Draw text
		fd.draw(this, sce.p.health.ToString() + "/" + sce.p.maxHealth.ToString() , -width/2f + 45f, height/2f, Renderer.bigTextSize, new Color3("AA0000"));
		string temp2 = sce.p.score.ToString();
		fd.draw(this, temp2, -temp2.Length / 2 * Renderer.middleTextSize.X, height/2f, Renderer.middleTextSize, Renderer.textColor);
		
		//Draw overlay
		td.draw(this, "heart", -width/2f, height/2f, 40f, 40f, Color3.White);
		if(sce.p.hasKey){
			if(sce.doingDescent){
				float factor = (float) (sce.descentTime/Scene.descentTimeMax);
				td.draw(this, "key", -width/2f, height/2f - 45f, 40f, 40f, Color3.White, Math.Min(1f, 1f - 2f*factor));
			}else{
				td.draw(this, "key", -width/2f, height/2f - 45f, 40f, 40f, Color3.White);
			}
		}
		
		//Draw clock
		if(sce.doingSmoothing && showClock){
			drawClock();
		}
		
		//Draw death black
		if(Engine.instance.uren.currentScreen != null){				
			rd.draw(this, -width/2f, height/2f, width, height, Color3.Black, screenAlpha);
		}
	}
	
	void drawClock(){
		Matrix4 rot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float) (360d * sce.smoothTime / sce.smoothTimeMax)));
		
		Matrix4 model = clockLeft * rot * clockRight;
		
		td.draw(this, "clock", model, Color3.White, 0.6f);
	}
	
	protected override void onResize(int width, int height){
		projection = Matrix4.CreateOrthographic((float) width, (float) height, -1.0f, 1.0f);
		
		clockRight = Matrix4.CreateTranslation(new Vector3(22.5f, -22.5f, 0f)) * Matrix4.CreateTranslation(new Vector3(-width/2f, -height/2f + 45f, 0f));
	}
}