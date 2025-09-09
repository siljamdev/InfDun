using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiImageButton : UiClickable{
	public string textureName;
	
	Vector2 size;
	
	public Color3 color;
	public Color3 hoverColor;
	
	public string? description;
	
	public UiImageButton(Placement p, float x, float y, float xs, float ys, string tn, Color3 c, Color3 hc) : base(p, x, y){
		textureName = tn;
		
		size = new Vector2(xs, ys);
		
		color = c;
		hoverColor = hc;
	}
	
	public UiImageButton(Placement p, float x, float y, float xs, float ys, string tn, Color3 c) : this(p, x, y, xs, ys, tn, c, Renderer.selectedTextColor){
		
	}
	
	public UiImageButton setDescription(string d){
		description = d;
		hasHover = true;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2d m){
		if(box != null && box % m){
			ren.drawTexture(textureName, pos, size, hoverColor);
		}else{
			ren.drawTexture(textureName, pos, size, color);
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