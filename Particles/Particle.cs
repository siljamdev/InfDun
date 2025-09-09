using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

abstract class Particle{
	static protected Random rand = new Random();
	
	protected Vector2 position;
	protected virtual Vector2 positionOffset => Vector2.Zero;
	protected virtual Vector2 scale => new Vector2(10f);
	protected virtual float rotation => 0f; //Degrees
	
	protected virtual Color3 color => Color3.Cyan;
	protected virtual float alpha => 1f;
	
	protected virtual float maxLife => 1f;
	protected float life{get; private set;}
	
	protected float lifeFactor => life/maxLife;
	
	//Returns false if it has to be deleted
	public virtual bool draw(Renderer ren){
		life += (float)Dungeon.dh.deltaTime;
		if(life > maxLife){
			return false;
		}
		
		return true;
	}
	
	protected void drawLocal(Renderer ren){
		Vector2 pos = position + positionOffset;
		Matrix4 model = Matrix4.CreateScale(new Vector3(scale.X, scale.Y, 0f)) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		ren.rectShader.use();
		ren.rectShader.setMatrix4("model", model);
		ren.rectShader.setMatrix4("view", Matrix4.Identity); //These are for local rects
		ren.rectShader.setVector4("col", color, alpha);
		
		ren.uiMesh.draw();
	}
	
	protected void drawWorld(Renderer ren){
		Vector2 pos = position + positionOffset;
		Matrix4 model = Matrix4.CreateScale(new Vector3(scale.X, scale.Y, 0f)) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		ren.rectShader.use();
		ren.rectShader.setMatrix4("model", model);
		ren.rectShader.setMatrix4("view", ren.cam.view);
		ren.rectShader.setVector4("col", color, alpha);
		
		ren.uiMesh.draw();
	}
	
	protected static double getRandom(double min, double max){
		return rand.NextDouble() * (max - min) + min;
	}
}