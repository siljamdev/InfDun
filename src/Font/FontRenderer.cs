using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

abstract class FontRenderer{
	protected Texture2D font;
	
	protected Mesh fontMesh;
	
	protected Shader fontShader;
	
	public FontRenderer(Mesh m, Texture2D t){
		font = t;
		fontMesh = m;
	}
	
	public void setProjection(Matrix4 m){
		fontShader.setMatrix4("projection", m);
	}
	
	public abstract float getXadvance(char c, Vector2 sca);
	
	public abstract float getXsize(string text, Vector2 sca);
	
	//Placement is anchor point of text, not in the screen
	public abstract void drawText(string text, Vector2 pos, Vector2 sca, Placement p, Color3 col, float alpha = 1f);
	
	public void drawText(string text, float xpos, float ypos, float xsca, float ysca, Placement p, Color3 col, float alpha = 1f){
		drawText(text, new Vector2(xpos, ypos), new Vector2(xsca, ysca), p, col, alpha);
	}
	
	public void drawText(string text, float xpos, float ypos, float sca, Placement p, Color3 col, float alpha = 1f){
		drawText(text, new Vector2(xpos, ypos), new Vector2(sca, sca), p, col, alpha);
	}
	
	public void drawText(string text, float xpos, float ypos, Vector2 sca, Placement p, Color3 col, float alpha = 1f){
		drawText(text, new Vector2(xpos, ypos), sca, p, col, alpha);
	}
	
	public void drawText(string text, Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		drawText(text, pos, sca, Placement.TopLeft, col, alpha);
	}
	
	public void drawText(string text, float xpos, float ypos, float xsca, float ysca, Color3 col, float alpha = 1f){
		drawText(text, new Vector2(xpos, ypos), new Vector2(xsca, ysca), Placement.TopLeft, col, alpha);
	}
	
	public void drawText(string text, float xpos, float ypos, float sca, Color3 col, float alpha = 1f){
		drawText(text, new Vector2(xpos, ypos), new Vector2(sca, sca), Placement.TopLeft, col, alpha);
	}
	
	public void drawText(string text, float xpos, float ypos, Vector2 sca, Color3 col, float alpha = 1f){
		drawText(text, new Vector2(xpos, ypos), sca, Placement.TopLeft, col, alpha);
	}
	
	protected Vector2 getPos(Vector2 pos, float ysize, float totalSize, Placement p){
		switch(p){
			default:
			case Placement.TopLeft:
				return pos;
			
			case Placement.TopRight:
				return new Vector2(pos.X - totalSize, pos.Y);
			
			case Placement.BottomLeft:
				return new Vector2(pos.X, pos.Y + ysize);
			
			case Placement.BottomRight:
				return new Vector2(pos.X - totalSize, pos.Y + ysize);
			
			case Placement.TopCenter: //0, 1
				return new Vector2(pos.X - totalSize / 2f, pos.Y);
			
			case Placement.BottomCenter: //0, -1
				return new Vector2(pos.X - totalSize / 2f, pos.Y + ysize);
			
			case Placement.CenterLeft: //-1, 0
				return new Vector2(pos.X, pos.Y + ysize / 2f);
			
			case Placement.CenterRight: //1, 0
				return new Vector2(pos.X - totalSize, pos.Y + ysize / 2f);
			
			case Placement.Center: //0, 0
				return new Vector2(pos.X - totalSize / 2f, pos.Y + ysize / 2f);
		}
	}
}