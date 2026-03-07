using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiField : UiSelectable{
	public string text{get; private set;}
	public string question{get; private init;}
	public WritingType type;
	float minXsize;
	int maxChars;
	
	public Color3 fieldColor;
	public Color3 hoverFieldColor;
	public Color3 selectedFieldColor;
	public Color3 textColor;
	public Color3 hoverTextColor;
	public Color3 selectedTextColor;
	public Color3 questionColor;
	
	float qXsize = -1f;
	float fXsize;
	
	public UiField(Placement p, float x, float y, float xs, string q, string t, int mc, WritingType wt) : base(p, x, y){
		text = t;
		question = q;
		
		maxChars = mc;
		type = wt;
		
		minXsize = xs;
		
		fieldColor = Renderer.fieldColor;
		hoverFieldColor = getHoverColor(fieldColor);
		selectedFieldColor = Renderer.fieldSelectedColor;
		textColor = Renderer.fieldTextColor;
		hoverTextColor = Renderer.selectedTextColor;
		selectedTextColor = Renderer.textColor;
		questionColor = Renderer.textColor;
	}
	
	public UiField setColor(Color3 f, Color3 sf){
		fieldColor = f;
		hoverFieldColor = getHoverColor(fieldColor);
		selectedFieldColor = sf;
		return this;
	}
	
	public UiField setTextColor(Color3 t, Color3 ht, Color3 st){
		textColor = t;
		hoverTextColor = ht;
		selectedTextColor = st;
		return this;
	}
	
	public UiField setQuestionColor(Color3 q){
		questionColor = q;
		return this;
	}
	
	public UiField setText(string t){
		text = t.Length > maxChars ? t.Substring(0, maxChars) : t;
		needsToUpdate = true;
		return this;
	}
	
	public bool addStr(string s){
		if(text.Length + s.Length > maxChars){
			return false;
		}
		
		switch(type){
			case WritingType.Hex:
				if(!getHexTyping(s)){
					return false;
				}
				break;
				
				case WritingType.Int:
				if(!getIntTyping(s)){
					return false;
				}
				break;
				
				case WritingType.Float:
				if(!getFloatTyping(s)){
					return false;
				}
				break;
				
				case WritingType.FloatPositive:
				if(!getFloatPositiveTyping(s)){
					return false;
				}
				break;
				
				default:
				case WritingType.String:
				break;
		}
		
		text += s;
		needsToUpdate = true;
		return true;
	}
	
	public bool delChar(){
		if(text.Length < 1){
			return false;
		}
		text = text.Substring(0, text.Length - 1);
		needsToUpdate = true;
		return true;
	}
	
	public override void draw(Renderer ren, Vector2d mousePos){
		Vector2 fsize = new Vector2(fXsize, Renderer.textSize.Y + 10f);
		
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
		
		fXsize = Math.Max(ren.fr.getXsize(text, Renderer.textSize) + 10f, this.minXsize);
		
		return new Vector2(fXsize + qXsize + 10f, Renderer.textSize.Y + 10f);
	}
	
	//Only the writable part
	protected override AABB updateBox(Renderer ren){
		Vector2 fsize = new Vector2(ren.fr.getXsize(text, Renderer.textSize) + 10f, Renderer.textSize.Y + 10f);
		
		fsize.X = Math.Max(fsize.X, this.minXsize);
		
		Vector2 fpos = new Vector2(pos.X + ren.fr.getXsize(question, Renderer.textSize) + 10f, pos.Y);
		
		return new AABB(fpos.Y, fpos.Y - fsize.Y, fpos.X, fpos.X + fsize.X);
	}
	
	static bool getHexTyping(string s){
		for(int i = 0; i < s.Length; i++){
			if(!Uri.IsHexDigit(s[i])){
				return false;
			}
		}
		return true;
	}
	
	static bool getFloatPositiveTyping(string s){
		for(int i = 0; i < s.Length; i++){
			if(!(char.IsDigit(s[i]) || s[i] == '.')){
				return false;
			}
		}
		return true;
	}
	
	static bool getFloatTyping(string s){
		for(int i = 0; i < s.Length; i++){
			if(!(char.IsDigit(s[i]) || s[i] == '.' || s[i] == '-')){
				return false;
			}
		}
		return true;
	}
	
	static bool getIntTyping(string s){
		for(int i = 0; i < s.Length; i++){
			if(!char.IsDigit(s[i])){
				return false;
			}
		}
		return true;
	}
}

enum WritingType{
	Hex, Int, Float, FloatPositive, String
}