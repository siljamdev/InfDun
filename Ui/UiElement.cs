using System;
using OpenTK;
using OpenTK.Mathematics;

abstract class UiElement{
	public AABB box{get; private set;}
	
	public Placement placement;
	public Vector2 offset{get; private set;}
	
	//To not have to update it always
	protected Vector2 pos{get; private set;}
	
	public bool needGen{get; protected set;}
	
	//Shows up
	public bool active;
	
	public bool hasHover{get; protected set;}
	
	public UiElement(Placement p, Vector2 o){
		active = true;
		
		placement = p;
		offset = o;
	}
	
	public UiElement(Placement p, float x, float y) : this(p, new Vector2(x, y)){
		
	}
	
	public void update(Renderer ren){
		pos = updatePos(ren);
		box = updateBox(ren);
		
		needGen = false;
	}
	
	protected Vector2 getPos(Renderer ren, Vector2 siz){
		Vector2 dim = new Vector2(ren.width / 2f, ren.height / 2f);
		
		switch(placement){
			case Placement.TopLeft: //-1, 1
				return new Vector2(-dim.X, dim.Y) + new Vector2(offset.X, -offset.Y);
			
			case Placement.TopRight: //1, 1
				return new Vector2(dim.X, dim.Y) + new Vector2(-offset.X, -offset.Y) + new Vector2(-siz.X, 0);
			
			case Placement.BottomLeft: //-1, -1
				return new Vector2(-dim.X, -dim.Y) + new Vector2(offset.X, offset.Y) + new Vector2(0, siz.Y);
			
			case Placement.BottomRight: //1, -1
				return new Vector2(dim.X, -dim.Y) + new Vector2(-offset.X, offset.Y) + new Vector2(-siz.X, siz.Y);
			
			case Placement.TopCenter: //0, 1
				return new Vector2(0, dim.Y) + new Vector2(offset.X, -offset.Y) + new Vector2(-siz.X / 2f, 0);
			
			case Placement.BottomCenter: //0, -1
				return new Vector2(0, -dim.Y) + new Vector2(offset.X, offset.Y) + new Vector2(-siz.X / 2f, siz.Y);
			
			case Placement.CenterLeft: //-1, 0
				return new Vector2(-dim.X, 0) + new Vector2(offset.X, -offset.Y) + new Vector2(0, siz.Y / 2f);
			
			case Placement.CenterRight: //1, 0
				return new Vector2(dim.X, 0) + new Vector2(-offset.X, -offset.Y) + new Vector2(-siz.X, siz.Y / 2f);
			
			default:
			case Placement.Center: //0, 0
				return new Vector2(0, 0) + new Vector2(offset.X, -offset.Y) + new Vector2(-siz.X / 2, siz.Y / 2f);
		}
	}
	
	abstract public void draw(Renderer ren, Vector2d mousePos);
	
	abstract public void drawHover(Renderer ren, Vector2d mousePos);
	
	abstract protected AABB updateBox(Renderer ren);
	
	abstract protected Vector2 updatePos(Renderer ren);
}

enum Placement{
	Center, TopLeft, TopRight, BottomLeft, BottomRight, CenterLeft, CenterRight, TopCenter, BottomCenter
}