using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

abstract class Particle{
	public const int dataSize = 9;
	public const string dataFormat = "2214";
	static protected Random rand = new Random();
	
	public Vector2 position;
	protected virtual Vector2 positionOffset => Vector2.Zero;
	protected virtual Vector2 scale => new Vector2(10f);
	protected virtual float rotation => 0f; //Degrees
	
	protected virtual Color3 color => Color3.White;
	protected virtual float alpha => 1f;
	
	protected virtual float maxLife => 1f;
	protected float life{get; private set;}
	
	protected float lifeFactor => life/maxLife;
	
	//Returns false if it has to be deleted
	public bool update(){
		life += (float) Engine.instance.dh.deltaTime;
		if(life > maxLife){
			return false;
		}
		
		return true;
	}
	
	public void writeData(float[] data, int index){
		Vector2 pos = position + positionOffset;
		
		data[index + 0] = pos.X;
		data[index + 1] = pos.Y;
		
		Vector2 sca = scale;
		
		data[index + 2] = sca.X;
		data[index + 3] = sca.Y;
		
		data[index + 4] = MathHelper.DegreesToRadians(rotation);
		
		Color3 col = color;
		
		data[index + 5] = color.R / 255f;
		data[index + 6] = color.G / 255f;
		data[index + 7] = color.B / 255f;
		
		data[index + 8] = alpha;
	}
	
	protected static float getRandom(float min, float max){
		return rand.NextSingle() * (max - min) + min;
	}
	
	protected static Vector2 getRandomVector(float min, float max){
		return new Vector2(getRandom(min, max), getRandom(min, max));
	}
}