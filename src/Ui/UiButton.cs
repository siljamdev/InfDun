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
	
	public override void draw(Renderer ren, Vector2d m){		
		if(box != null && box % m){
			ren.drawRect(pos, size, hoverColor, 1f);
			ren.fr.drawText(text, pos + new Vector2(5f, -5f), Renderer.textSize, hoverTextColor, 1f);
		}else{
			ren.drawRect(pos, size, color, 1f);
			ren.fr.drawText(text, pos + new Vector2(5f, -5f), Renderer.textSize, textColor, 1f);
		}
	}
	
	protected override Vector2 updateSize(Renderer ren){
		if(size != Vector2.Zero){
			return size;
		}
		
		Vector2 s = new Vector2(ren.fr.getXsize(text, Renderer.textSize) + 10f, Renderer.textSize.Y + 10f);
		s.X = Math.Max(s.X, this.minXSize);
		
		return s;
	}
	
	protected override AABB updateBox(Renderer ren){
		return new AABB(pos.Y, pos.Y - size.Y, pos.X, pos.X + size.X);
	}
}