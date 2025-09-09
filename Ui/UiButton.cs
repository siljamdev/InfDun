using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiButton : UiClickable{
	public string text{get; private set;}
	
	Vector2 size;
	
	float xSize;
	
	public Color3 textColor;
	public Color3 hoverTextColor;
	public Color3 color;
	public Color3 hoverColor;
	
	public string? description{get; private set;}
	
	public UiButton(Placement p, float x, float y, float xs, string t, Color3 c, Color3 tc, Color3 hc) : base(p, x, y){
		text = t;
		
		xSize = xs;
		
		color = c;
		hoverColor = new Color3((byte) (color.R * 1.2f), (byte) (color.G * 1.2f), color.B);
		textColor = tc;
		hoverTextColor = hc;
		
		size = new Vector2(text.Length * Renderer.textSize.X + 10f, Renderer.textSize.Y + 10f);
		size.X = Math.Max(size.X, this.xSize);
	}
	
	public UiButton(Placement p, float x, float y, float xs, string t, Color3 c) : this(p, x, y, xs, t, c, Renderer.textColor, Renderer.selectedTextColor){
		
	}
	
	public UiButton setDescription(string d){
		description = d;
		hasHover = true;
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
	
	public override void drawHover(Renderer ren, Vector2d mousePos){
		Vector2 mouse = (Vector2) mousePos;
		
		Vector2 size = new Vector2(description.Length * Renderer.textSize.X + 10f, Renderer.textSize.Y + 10f);
		
		if(mouse.X + size.X <= ren.width / 2f){
			ren.drawRect(mouse.X, mouse.Y + Renderer.textSize.Y + 10f, size.X, size.Y, Renderer.black, 0.5f);
			ren.fr.drawText(description, mouse.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
		}else{
			if((mouse.X + size.X) - (ren.width / 2f) <= (-ren.width / 2f) - (mouse.X - size.X)){
				ren.drawRect(mouse.X, mouse.Y + Renderer.textSize.Y + 10f, size.X, size.Y, Renderer.black, 0.5f);
				ren.fr.drawText(description, mouse.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
			}else{
				ren.drawRect(mouse.X - size.X, mouse.Y + Renderer.textSize.Y + 10f, size.X, size.Y, Renderer.black, 0.5f);
				ren.fr.drawText(description, mouse.X - size.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
			}
		}
	}
	
	protected override Vector2 updatePos(Renderer ren){
		return base.getPos(ren, size);
	}
	
	protected override AABB updateBox(Renderer ren){
		return new AABB(pos.Y, pos.Y - size.Y, pos.X, pos.X + size.X);
	}
}