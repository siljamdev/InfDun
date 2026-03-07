using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiKeyField : UiSelectable{
	public KeyBind key{get; private init;}
	public string question{get; private init;}
	
	public Color3 fieldColor;
	public Color3 hoverFieldColor;
	public Color3 selectedFieldColor;
	public Color3 textColor;
	public Color3 hoverTextColor;
	public Color3 selectedTextColor;
	public Color3 questionColor;
	
	float qXsize = -1f;
	
	public UiKeyField(Placement p, float x, float y, string q, KeyBind k) : base(p, x, y){
		key = k;
		question = q;
		
		fieldColor = Renderer.fieldColor;
		hoverFieldColor = getHoverColor(fieldColor);
		selectedFieldColor = Renderer.fieldSelectedColor;
		textColor = Renderer.fieldTextColor;
		hoverTextColor = Renderer.selectedTextColor;
		selectedTextColor = Renderer.textColor;
		questionColor = Renderer.textColor;
	}
	
	public UiKeyField setColor(Color3 f, Color3 sf){
		fieldColor = f;
		hoverFieldColor = getHoverColor(fieldColor);
		selectedFieldColor = sf;
		return this;
	}
	
	public UiKeyField setTextColor(Color3 t, Color3 ht, Color3 st){
		textColor = t;
		hoverTextColor = ht;
		selectedTextColor = st;
		return this;
	}
	
	public UiKeyField setQuestionColor(Color3 q){
		questionColor = q;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2d mousePos){
		string text = key.key.ToString();
		Vector2 fsize = new Vector2(ren.fr.getXsize(text, Renderer.textSize) + 10f, Renderer.textSize.Y + 10f);
		
		if(box != null && box % mousePos){
			if(selected){
				ren.drawRect(pos + new Vector2(qXsize + 10f, 0f), fsize, selectedFieldColor, 0.8f);
				ren.fr.drawText(text, pos + new Vector2(qXsize + 15f, -5f), Renderer.textSize, hoverTextColor, 1f);
			}else{
				ren.drawRect(pos + new Vector2(qXsize + 10f, 0f), fsize, hoverFieldColor, 0.8f);
				ren.fr.drawText(text, pos + new Vector2(qXsize + 15f, -5f), Renderer.textSize, hoverTextColor, 1f);
			}
		}else{
			if(selected){
				ren.drawRect(pos + new Vector2(qXsize + 10f, 0f), fsize, selectedFieldColor, 0.8f);
				ren.fr.drawText(text, pos + new Vector2(qXsize + 15f, -5f), Renderer.textSize, selectedTextColor, 1f);
			}else{
				ren.drawRect(pos + new Vector2(qXsize + 10f, 0f), fsize, fieldColor, 0.7f);
				ren.fr.drawText(text, pos + new Vector2(qXsize + 15f, -5f), Renderer.textSize, textColor, 1f);
			}
		}
		ren.fr.drawText(question, pos + new Vector2(0f, -5f), Renderer.textSize, questionColor, 1f);
	}
	
	protected override Vector2 updateSize(Renderer ren){
		if(qXsize == -1f){
			qXsize = ren.fr.getXsize(question, Renderer.textSize);
		}
		
		string text = key.key.ToString();
		
		return new Vector2(ren.fr.getXsize(text, Renderer.textSize) + 10f + qXsize + 10f, Renderer.textSize.Y + 10f);
	}
	
	protected override AABB updateBox(Renderer ren){
		string text = key.key.ToString();
		
		Vector2 fsize = new Vector2(ren.fr.getXsize(text, Renderer.textSize) + 10f, Renderer.textSize.Y + 10f);
		
		Vector2 fpos = new Vector2(pos.X + qXsize + 10f, pos.Y);
		
		return new AABB(fpos.Y, fpos.Y - fsize.Y, fpos.X, fpos.X + fsize.X);
	}
}