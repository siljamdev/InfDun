using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiImageBackButton : UiImageButton{
	public Color3 backColor;
	public Color3 hoverBackColor;
	
	public UiImageBackButton(Placement p, float x, float y, float xs, float ys, string tn) : base(p, x, y, xs, ys, tn){
		backColor = Renderer.buttonColor;
		hoverBackColor = getHoverColor(backColor);
	}
	
	public UiImageBackButton setBackColor(Color3 b){
		backColor = b;
		hoverBackColor = getHoverColor(backColor);
		return this;
	}
	
	public override void draw(Renderer ren, Vector2 m){
		if(box != null && box.collide(m)){
			rd.draw(ren, pos, size, hoverBackColor);
			td.draw(ren, textureName, pos + new Vector2(5f, -5f), size - new Vector2(10f), hoverColor);
		}else{
			rd.draw(ren, pos, size, backColor);
			td.draw(ren, textureName, pos + new Vector2(5f, -5f), size - new Vector2(10f), color);
		}
	}
}