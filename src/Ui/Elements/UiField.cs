using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiField : UiSelectable{
	const float borderWidth = 3f;
	
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
	
	float qXsize = -1f; //Size of question
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
	
	public override void draw(Renderer ren, Vector2 mousePos){
		Vector2 fsize = new Vector2(fXsize, Renderer.textSize.Y + 10f);
		
		if(box != null && box.collide(mousePos)){
			if(selected){
				rd.draw(ren, pos + new Vector2(qXsize + 10f, -borderWidth), fsize, selectedFieldColor, 0.8f);
				fd.draw(ren, text, pos + new Vector2(qXsize + 15f, -5f - borderWidth), Renderer.textSize, hoverTextColor, 1f);
				
				//Border
				rd.draw(ren, pos + new Vector2(qXsize + 10f - borderWidth, 0f), new Vector2(fsize.X + 2f * borderWidth, borderWidth), hoverTextColor, 1f); //Up
				rd.draw(ren, pos + new Vector2(qXsize + 10f - borderWidth, -fsize.Y - borderWidth), new Vector2(fsize.X + 2f * borderWidth, borderWidth), hoverTextColor, 1f); //Down
				rd.draw(ren, pos + new Vector2(qXsize + 10f - borderWidth, 0f), new Vector2(borderWidth, fsize.Y + 2f * borderWidth), hoverTextColor, 1f); //Left
				rd.draw(ren, pos + new Vector2(qXsize + 10f + fsize.X, 0f), new Vector2(borderWidth, fsize.Y + 2f * borderWidth), hoverTextColor, 1f); //Right
			}else{
				rd.draw(ren, pos + new Vector2(qXsize + 10f, -borderWidth), fsize, hoverFieldColor, 0.8f);
				fd.draw(ren, text, pos + new Vector2(qXsize + 15f, -5f - borderWidth), Renderer.textSize, hoverTextColor, 1f);
			}
		}else{
			if(selected){
				rd.draw(ren, pos + new Vector2(qXsize + 10f, -borderWidth), fsize, selectedFieldColor, 0.8f);
				fd.draw(ren, text, pos + new Vector2(qXsize + 15f, -5f - borderWidth), Renderer.textSize, selectedTextColor, 1f);
				
				//Border
				rd.draw(ren, pos + new Vector2(qXsize + 10f - borderWidth, 0f), new Vector2(fsize.X + 2f * borderWidth, borderWidth), selectedTextColor, 1f); //Up
				rd.draw(ren, pos + new Vector2(qXsize + 10f - borderWidth, -fsize.Y - borderWidth), new Vector2(fsize.X + 2f * borderWidth, borderWidth), selectedTextColor, 1f); //Down
				rd.draw(ren, pos + new Vector2(qXsize + 10f - borderWidth, 0f), new Vector2(borderWidth, fsize.Y + 2f * borderWidth), selectedTextColor, 1f); //Left
				rd.draw(ren, pos + new Vector2(qXsize + 10f + fsize.X, 0f), new Vector2(borderWidth, fsize.Y + 2f * borderWidth), selectedTextColor, 1f); //Right
			}else{
				rd.draw(ren, pos + new Vector2(qXsize + 10f, -borderWidth), fsize, fieldColor, 0.7f);
				fd.draw(ren, text, pos + new Vector2(qXsize + 15f, -5f - borderWidth), Renderer.textSize, textColor, 1f);
			}
		}
		
		fd.draw(ren, question, pos + new Vector2(0f, -5f -borderWidth), Renderer.textSize, questionColor, 1f);
		
		//box?.draw(ren);
	}
	
	protected override Vector2 updatePos(){
		return getDefaultPos() - new Vector2(0f, borderWidth);
	}
	
	protected override Vector2 updateSize(){
		if(qXsize == -1f){
			qXsize = fd.getXsize(question, Renderer.textSize);
		}
		
		fXsize = Math.Max(fd.getXsize(text, Renderer.textSize) + 10f, this.minXsize);
		
		return new Vector2(fXsize + qXsize + 10f + borderWidth, Renderer.textSize.Y + 10f + 2f * borderWidth);
	}
	
	//Only the writable part
	protected override AABB2D updateBox(){
		Vector2 fsize = new Vector2(fXsize + 2f * borderWidth, Renderer.textSize.Y + 10f + 2f * borderWidth); //Size of field + border
		
		Vector2 fpos = new Vector2(pos.X + qXsize + 10f - borderWidth, pos.Y);
		
		return new AABB2D(fpos.Y, fpos.Y - fsize.Y, fpos.X, fpos.X + fsize.X);
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