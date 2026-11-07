using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiImageButton : UiClickable{
	protected string textureName{get; private set;}
	public Color3 color;
	public Color3 hoverColor;
	
	public UiImageButton(Placement p, float x, float y, float xs, float ys, string tn) : base(p, x, y){
		textureName = tn;
		
		size = new Vector2(xs, ys);
		
		color = Renderer.textColor;
		hoverColor = Renderer.selectedTextColor;
	}
	
	public UiImageButton setColor(Color3 c, Color3 hc){
		color = c;
		hoverColor = hc;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2 m){
		if(box != null && box.collide(m)){
			td.draw(ren, textureName, pos, size, hoverColor);
		}else{
			td.draw(ren, textureName, pos, size, color);
		}
	}
	
	protected override Vector2 updateSize(){
		return size;
	}
}