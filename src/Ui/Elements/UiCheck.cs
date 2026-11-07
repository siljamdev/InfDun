using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

//Checkbox
class UiCheck : UiClickable{
	public string question{get; private init;}
	
	public Color3 tickColor;
	public Color3 hoverTickColor;
	public Color3 color;
	public Color3 hoverColor;
	public Color3 questionColor;
	
	public bool isChecked;
	
	float xSize;
	float ySize;
	float qSize;
	
	public UiCheck(Placement p, float x, float y, float xs, float ys, string q, bool o) : base(p, x, y){				
		question = q;
		isChecked = o;
		
		xSize = xs;
		ySize = ys;
		
		color = Renderer.buttonColor;
		hoverColor = getHoverColor(color);
		tickColor = Renderer.textColor;
		hoverTickColor = Renderer.selectedTextColor;
		questionColor = Renderer.textColor;
		
		setAction(toggle);
	}
	
	public UiCheck setColor(Color3 c){
		color = c;
		hoverColor = getHoverColor(color);
		return this;
	}
	
	public UiCheck setQuestionColor(Color3 q){
		questionColor = q;
		return this;
	}
	
	public UiCheck setTickColor(Color3 t, Color3 ht){
		tickColor = t;
		hoverTickColor = ht;
		return this;
	}
	
	public void toggle(){
		isChecked = !isChecked;
	}
	
	public override void draw(Renderer ren, Vector2 mousePos){
		Vector2 fsize = size - new Vector2(qSize + 10f, 0f);
		Vector2 fpos = pos + new Vector2(qSize + 10f, 0f);
		
		if(box != null && box.collide(mousePos)){
			rd.draw(ren, fpos, fsize, hoverColor);
			if(isChecked){
				td.draw(ren, "tick", fpos, fsize, hoverTickColor);
			}
		}else{
			rd.draw(ren, fpos, fsize, color);
			if(isChecked){
				td.draw(ren, "tick", fpos, fsize, tickColor);
			}
		}
		fd.draw(ren, question, pos + new Vector2(0f, -((size.Y - Renderer.textSize.Y)/2f)), Renderer.textSize, questionColor, 1f);
	}
	
	protected override Vector2 updateSize(){
		if(size != Vector2.Zero){
			return size;
		}
		
		qSize = fd.getXsize(question, Renderer.textSize);
		return new Vector2(qSize + 10f + xSize, Math.Max(ySize, Renderer.textSize.Y));
	}
	
	protected override AABB2D updateBox(){
		Vector2 fsize = size - new Vector2(qSize + 10f, 0f);
		Vector2 fpos = pos + new Vector2(qSize + 10f, 0f);
		
		return new AABB2D(fpos.Y, fpos.Y - fsize.Y, fpos.X, fpos.X + fsize.X);
	}
}