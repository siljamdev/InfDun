using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiText : UiElement{
	public string text{get; private set;}
	
	public Color3 color;
	
	Vector2 textSize;
	
	protected override bool doScissor => false; //TT fonts might extend more than they say they do
	
	public UiText(Placement p, float x, float y, string t) : base(p, x, y){
		text = t;
		color = Renderer.textColor;
		textSize = Renderer.textSize;
	}
	
	public UiText setText(string t){
		text = t;
		needsToUpdate = true;
		return this;
	}
	
	public UiText setColor(Color3 c){
		color = c;
		return this;
	}
	
	public UiText setCharSize(Vector2 s){
		textSize = s;
		needsToUpdate = true;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2 mousePos){
		fd.draw(ren, text, pos, textSize, color);
	}
	
	protected override Vector2 updateSize(){
		return new Vector2(fd.getXsize(text, textSize), textSize.Y);
	}
}