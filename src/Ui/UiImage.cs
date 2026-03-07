using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiImage : UiElement{
	string textureName;
	public Color3 color;
	
	public override bool canHaveDescription => true;
	
	public UiImage(Placement p, float x, float y, float xs, float ys, string tn) : base(p, x, y){
		textureName = tn;
		
		size = new Vector2(xs, ys);
		
		color = Renderer.textColor;
	}
	
	public UiImage setColor(Color3 c){
		color = c;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2d m){
		ren.drawTexture(textureName, pos, size, color);
	}
	
	protected override Vector2 updateSize(Renderer ren){
		return size;
	}
}