using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiButton : UiClickable{
	public string text{get; private set;}
	
	float minXSize;
	
	public Color3 textColor;
	public Color3 hoverTextColor;
	public Color3 color;
	public Color3 hoverColor;
	
	public UiButton(Placement p, float x, float y, float xs, string t) : base(p, x, y){
		text = t;
		
		minXSize = xs;
		
		color = Renderer.buttonColor;
		hoverColor = getHoverColor(color);
		textColor = Renderer.textColor;
		hoverTextColor = Renderer.selectedTextColor;
	}
	
	public UiButton setColor(Color3 c){
		color = c;
		hoverColor = getHoverColor(color);
		return this;
	}
	
	public UiButton setTextColor(Color3 t, Color3 ht){
		textColor = t;
		hoverTextColor = ht;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2 m){		
		if(box != null && box.collide(m)){
			rd.draw(ren, pos, size, hoverColor, 1f);
			fd.draw(ren, text, pos + new Vector2(5f, -5f), Renderer.textSize, hoverTextColor, 1f);
		}else{
			rd.draw(ren, pos, size, color, 1f);
			fd.draw(ren, text, pos + new Vector2(5f, -5f), Renderer.textSize, textColor, 1f);
		}
	}
	
	protected override Vector2 updateSize(){
		if(size != Vector2.Zero){
			return size;
		}
		
		Vector2 s = new Vector2(fd.getXsize(text, Renderer.textSize) + 10f, Renderer.textSize.Y + 10f);
		s.X = Math.Max(s.X, this.minXSize);
		
		return s;
	}
	
	protected override AABB2D updateBox(){
		return new AABB2D(pos.Y, pos.Y - size.Y, pos.X, pos.X + size.X);
	}
}