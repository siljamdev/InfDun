using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

//Checkbox
class UiCheck : UiClickable{
	string question;
	
	public string? description;
	
	Vector2 size;
	
	public Color3 tickColor;
	public Color3 hoverTickColor;
	public Color3 color;
	public Color3 hoverColor;
	
	public bool on;
	
	public UiCheck(Placement p, float x, float y, float xs, float ys, string q, bool o, Color3 c, Color3 tc, Color3 hc) : base(p, x, y){				
		question = q;
		on = o;
		
		size = new Vector2(question.Length * Renderer.textSize.X + 10f + xs, Math.Max(ys, Renderer.textSize.Y));
		
		color = c;
		hoverColor = new Color3((byte) (color.R * 1.2f), (byte) (color.G * 1.2f), color.B);
		tickColor = tc;
		hoverTickColor = hc;
		
		setAction(toggle);
	}
	
	public UiCheck(Placement p, float x, float y, float xs, float ys, string q, bool o, Color3 c) : this(p, x, y, xs, ys, q, o, c, Renderer.textColor, Renderer.selectedTextColor){
		
	}
	
	public UiCheck setDescription(string d){
		description = d;
		hasHover = true;
		return this;
	}
	
	public void toggle(){
		on = !on;
	}
	
	public override void draw(Renderer ren, Vector2d mousePos){
		Vector2 fsize = size - new Vector2(question.Length * Renderer.textSize.X + 10f, 0f);
		
		Vector2 fpos = pos + new Vector2(question.Length * Renderer.textSize.X + 10f, 0f);
		
		if(box != null && box % mousePos){
			ren.drawRect(fpos, fsize, hoverColor);
			if(on){
				ren.drawTexture("tick", fpos, fsize, hoverTickColor);
			}
		}else{
			ren.drawRect(fpos, fsize, color);
			if(on){
				ren.drawTexture("tick", fpos, fsize, tickColor);
			}
		}
		ren.fr.drawText(question, pos + new Vector2(0f, -((size.Y - Renderer.textSize.Y)/2f)), Renderer.textSize, Renderer.textColor, 1f);
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
		Vector2 fsize = size - new Vector2(question.Length * Renderer.textSize.X + 10f, 0f);
		
		Vector2 fpos = pos + new Vector2(question.Length * Renderer.textSize.X + 10f, 0f);		
		
		return new AABB(fpos.Y, fpos.Y - fsize.Y, fpos.X, fpos.X + fsize.X);
	}
}