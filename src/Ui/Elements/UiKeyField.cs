using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiKeyField : UiSelectable{
	const float borderWidth = 3f;
	
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
	
	public override void draw(Renderer ren, Vector2 mousePos){
		string text = key.key.ToString();
		Vector2 fsize = new Vector2(fd.getXsize(text, Renderer.textSize) + 10f, Renderer.textSize.Y + 10f);
		
		if(box != null && box.collide(mousePos)){
			if(selected){
				rd.draw(ren, pos + new Vector2(qXsize + 10f, -borderWidth), fsize, selectedFieldColor, 0.8f);
				fd.draw(ren, text, pos + new Vector2(qXsize + 15f, -5f -borderWidth), Renderer.textSize, hoverTextColor, 1f);
				
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
	}
	
	protected override Vector2 updatePos(){
		return getDefaultPos() - new Vector2(0f, borderWidth);
	}
	
	protected override Vector2 updateSize(){
		if(qXsize == -1f){
			qXsize = fd.getXsize(question, Renderer.textSize);
		}
		
		string text = key.key.ToString();
		
		return new Vector2(qXsize + 10f + fd.getXsize(text, Renderer.textSize) + 10f + borderWidth, Renderer.textSize.Y + 10f + 2f * borderWidth);
	}
	
	protected override AABB2D updateBox(){
		string text = key.key.ToString();
		
		Vector2 fsize = new Vector2(fd.getXsize(text, Renderer.textSize) + 10f + 2f * borderWidth, Renderer.textSize.Y + 10f + 2f * borderWidth); //Field + border
		
		Vector2 fpos = new Vector2(pos.X + qXsize + 10f - borderWidth, pos.Y);
		
		return new AABB2D(fpos.Y, fpos.Y - fsize.Y, fpos.X, fpos.X + fsize.X);
	}
}