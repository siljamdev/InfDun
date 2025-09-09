using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiField : UiSelectable{
	public string text;
	string question;
	
	float xSize;
	
	int maxChars;
	public WritingType type;
	
	public string? description;
	
	public UiField(Placement p, float x, float y, float xs, string q, string t, int mc, WritingType wt) : base(p, x, y){
		text = t;
		question = q;
		
		maxChars = mc;
		type = wt;
		
		xSize = xs;
	}
	
	public UiField setDescription(string d){
		description = d;
		hasHover = true;
		return this;
	}
	
	public bool addStr(string s){
		switch(type){
			case WritingType.Hex:
				if(!KeyBind.getHexTyping(s)){
					return false;
				}
				break;
				
				case WritingType.Int:
				if(!KeyBind.getIntTyping(s)){
					return false;
				}
				break;
				
				case WritingType.Float:
				if(!KeyBind.getFloatTyping(s)){
					return false;
				}
				break;
				
				case WritingType.FloatPositive:
				if(!KeyBind.getFloatPositiveTyping(s)){
					return false;
				}
				break;
				
				default:
				case WritingType.String:
				break;
		}
		
		if(text.Length + s.Length > maxChars){
			return false;
		}
		text += s;
		return true;
	}
	
	public bool delChar(){
		if(text.Length == 0){
			return false;
		}
		text = text.Substring(0, text.Length - 1);
		return true;
	}
	
	public override void draw(Renderer ren, Vector2d m){
		Vector2 fsize = new Vector2(text.Length * Renderer.textSize.X + 10f, Renderer.textSize.Y + 10f);
		
		fsize.X = Math.Max(fsize.X, this.xSize);
		
		if(selected){
			ren.drawRect(pos + new Vector2(question.Length * Renderer.textSize.X + 10f, 0f), fsize, Renderer.fieldSelectedColor, 0.8f);
			ren.fr.drawText(text, pos + new Vector2(question.Length * Renderer.textSize.X + 15f, -5f), Renderer.textSize, Renderer.textColor, 1f);
		}else{
			ren.drawRect(pos + new Vector2(question.Length * Renderer.textSize.X + 10f, 0f), fsize, Renderer.fieldColor, 0.7f);
			ren.fr.drawText(text, pos + new Vector2(question.Length * Renderer.textSize.X + 15f, -5f), Renderer.textSize, Renderer.fieldTextColor, 1f);
		}
		ren.fr.drawText(question, pos + new Vector2(0f, -5f), Renderer.textSize, Renderer.textColor, 1f);
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
		Vector2 size = new Vector2(Math.Max(text.Length * Renderer.textSize.X + 10f, this.xSize) + question.Length * Renderer.textSize.X + 10f, Renderer.textSize.Y + 10f);
		
		return base.getPos(ren, size);
	}
	
	protected override AABB updateBox(Renderer ren){
		Vector2 fsize = new Vector2(text.Length * Renderer.textSize.X + 10f, Renderer.textSize.Y + 10f);
		
		fsize.X = Math.Max(fsize.X, this.xSize);
		
		Vector2 fpos = new Vector2(pos.X + question.Length * Renderer.textSize.X + 10f, pos.Y);
		
		return new AABB(fpos.Y, fpos.Y - fsize.Y, fpos.X, fpos.X + fsize.X);
	}
}

enum WritingType{
	Hex, Int, Float, FloatPositive, String
}