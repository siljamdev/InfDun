using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using AshLib;

abstract class UiElement{
	public Placement placement{get; private init;}
	public Vector2 offset{get; private init;}
	public bool isActive = true; //If it shows up
	public string description{get; private set;} = null;
	float? descriptionXsize = null;
	
	public bool needsToUpdate{get; protected set;} = true;
	
	//To not have to update it always
	protected Vector2 pos{get; private set;}
	protected Vector2 size{get; set;} //Graphical size. DONT SET DIRECTLY!!! ONLY SET DIRECTLY ON INIT!!!
	public AABB2D box{get; private set;}
	
	protected virtual bool canHaveDescription => false;
	protected virtual bool doScissor => true;
	
	//Useful to avoid writing Engine.instance...
	protected static RectDrawer2D rd => Engine.instance.rd;
	protected static TextureDrawer2D td => Engine.instance.td;
	protected static FontDrawer2D fd => Engine.instance.fd;
	protected static int width => Engine.instance.width;
	protected static int height => Engine.instance.height;
	
	public UiElement(Placement p, Vector2 o){
		placement = p;
		offset = placement switch{
			//-1, 1
			Placement.TopLeft => new Vector2(o.X, -o.Y),
			
			//1, 1
			Placement.TopRight => new Vector2(-o.X, -o.Y),
			
			//-1, -1
			Placement.BottomLeft => o,
			
			//1, -1
			Placement.BottomRight => new Vector2(-o.X, o.Y),
			
			//0, 1
			Placement.TopCenter => new Vector2(o.X, -o.Y),
			
			//0, -1
			Placement.BottomCenter => o,
			
			//-1, 0
			Placement.CenterLeft => new Vector2(o.X, -o.Y),
			
			//1, 0
			Placement.CenterRight => new Vector2(-o.X, -o.Y),
			
			//0, 0
			Placement.Center => new Vector2(o.X, -o.Y),
			_ => new Vector2(o.X, -o.Y)
		};
	}
	
	public UiElement(Placement p, float x, float y) : this(p, new Vector2(x, y)){}
	
	public void update(Renderer ren){
		size = updateSize();
		pos = updatePos();
		box = updateBox();
		
		if(description != null && descriptionXsize == null){
			descriptionXsize = fd.getXsize(description, Renderer.textSize);
		}
		
		needsToUpdate = false;
	}
	
	public UiElement setDescription(string d){
		if(canHaveDescription){
			description = d;
			descriptionXsize = null;
			
			needsToUpdate = true;
		}
		return this;
	}
	
	protected Vector2 getDefaultPos(){
		Vector2 dim = new Vector2(width / 2f, height / 2f);
		
		switch(placement){
			case Placement.TopLeft: //-1, 1
				return new Vector2(-dim.X, dim.Y) + offset;
			
			case Placement.TopRight: //1, 1
				return new Vector2(dim.X, dim.Y) + offset + new Vector2(-size.X, 0);
			
			case Placement.BottomLeft: //-1, -1
				return new Vector2(-dim.X, -dim.Y) + offset + new Vector2(0, size.Y);
			
			case Placement.BottomRight: //1, -1
				return new Vector2(dim.X, -dim.Y) + offset + new Vector2(-size.X, size.Y);
			
			case Placement.TopCenter: //0, 1
				return new Vector2(0, dim.Y) + offset + new Vector2(-size.X / 2f, 0);
			
			case Placement.BottomCenter: //0, -1
				return new Vector2(0, -dim.Y) + offset + new Vector2(-size.X / 2f, size.Y);
			
			case Placement.CenterLeft: //-1, 0
				return new Vector2(-dim.X, 0) + offset + new Vector2(0, size.Y / 2f);
			
			case Placement.CenterRight: //1, 0
				return new Vector2(dim.X, 0) + offset + new Vector2(-size.X, size.Y / 2f);
			
			default:
			case Placement.Center: //0, 0
				return new Vector2(0, 0) + offset + new Vector2(-size.X / 2, size.Y / 2f);
		}
	}
	
	public void drawScissored(Renderer ren, Vector2 mousePos){
		if(doScissor){
			Vector2 dim = new Vector2(width / 2f, height / 2f);
			
			Vector2 correctedPos = pos + dim - new Vector2(0f, size.Y);
			
			GL.Enable(EnableCap.ScissorTest);
			
			GL.Scissor((int) correctedPos.X, (int) correctedPos.Y, (int) size.X, (int) size.Y);
			
			draw(ren, mousePos);
			
			GL.Disable(EnableCap.ScissorTest);
		}else{
			draw(ren, mousePos);
		}
		
		//new AABB2D(pos, pos + new Vector2(size.X, -size.Y)).draw(ren); //Scissor box
		//box?.draw(ren);
	}
	
	public abstract void draw(Renderer ren, Vector2 mousePos);
	
	protected abstract Vector2 updateSize();
	
	protected virtual Vector2 updatePos(){
		return getDefaultPos();
	}
	
	protected virtual AABB2D updateBox(){
		return canHaveDescription ? new AABB2D(pos.Y, pos.Y - size.Y, pos.X, pos.X + size.X) : null;
	}
	
	//Draws description
	public void drawHover(Renderer ren, Vector2 mouse){
		if(description == null){
			return;
		}
		
		Vector2 dSize = new Vector2((float) descriptionXsize + 10f, Renderer.textSize.Y + 10f);
		
		if(mouse.X + dSize.X <= width / 2f){
			rd.draw(ren, mouse.X, mouse.Y + Renderer.textSize.Y + 10f, dSize.X, dSize.Y, Renderer.black, 0.5f);
			fd.draw(ren, description, mouse.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
		}else{
			if((mouse.X + dSize.X) - (width / 2f) <= (-width / 2f) - (mouse.X - dSize.X)){
				rd.draw(ren, mouse.X, mouse.Y + Renderer.textSize.Y + 10f, dSize.X, dSize.Y, Renderer.black, 0.5f);
				fd.draw(ren, description, mouse.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
			}else{
				rd.draw(ren, mouse.X - dSize.X, mouse.Y + Renderer.textSize.Y + 10f, dSize.X, dSize.Y, Renderer.black, 0.5f);
				fd.draw(ren, description, mouse.X - dSize.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
			}
		}
	}
	
	protected static Color3 getHoverColor(Color3 c){
		return new Color3((byte) (c.R * 1.2f), (byte) (c.G * 1.2f), c.B);
	}
}

enum Placement{
	Center, TopLeft, TopRight, BottomLeft, BottomRight, CenterLeft, CenterRight, TopCenter, BottomCenter
}