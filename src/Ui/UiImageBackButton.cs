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
	
	public override void draw(Renderer ren, Vector2d m){
		if(box != null && box % m){
			ren.drawRect(pos, size, hoverBackColor, 1f);
			ren.drawTexture(textureName, pos + new Vector2(5f, -5f), size - new Vector2(10f), hoverColor);
		}else{
			ren.drawRect(pos, size, backColor, 1f);
			ren.drawTexture(textureName, pos + new Vector2(5f, -5f), size - new Vector2(10f), color);
		}
	}
}