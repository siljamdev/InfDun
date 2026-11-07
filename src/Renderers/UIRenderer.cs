using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

class UIRenderer : Renderer{
	public ParticleDrawer uipd {get; private set;} //UI particle renderer
	
	Stack<UiScreen> screens = new();
	public UiScreen currentScreen {get; private set;}
	
	public Vector2 mousePosition {get; private set;}
	
	public UIRenderer() : base(){
		view = Matrix4.Identity; //Never changes
		zoom = 1f; //Never changes
		
		uipd = new ParticleDrawer();
		
		Engine.instance.onMouseMove += mouse;
		Engine.instance.onParticleActivation += particleActivation;
	}
	
	public void setScreen(UiScreen s){
		if(currentScreen != null){
			currentScreen.close();
			screens.Push(currentScreen);
		}
		
		if(s == null){
			screens.Clear();
		}
		
		currentScreen = s;
		currentScreen?.updateProj(this);
	}
	
	public void closeScreen(){
		if(Engine.instance.sce == null && screens.Count == 0){ //Not close main menu
			return;
		}
		
		if(screens.Count == 0){
			currentScreen?.close();
			currentScreen = null;
			return;
		}
		
		currentScreen?.close();
		currentScreen = screens.Pop();
		currentScreen?.updateProj(this);
	}
	
	public void mouse(float x, float y){
		mousePosition = new Vector2(x - Engine.instance.width / 2f, Engine.instance.height / 2f - y);
	}
	
	public bool handleKeyDown(KeyboardKeyEventArgs e){
		if(currentScreen != null){
			if(!e.IsRepeat && e.Key != Keys.Escape && e.Key != Keys.Backspace){
				return currentScreen.trySetKeybind(e.Key);
			}else if(e.Key == Keys.Backspace){
				return currentScreen.tryDelChar();
			}
		}
		
		return false;
	}
	
	public bool handleTextInput(string text){
		return currentScreen?.tryAddStr(text) ?? false;
	}
	
	public bool handleMouseScroll(float y){
		return currentScreen?.scroll(y) ?? false;
	}
	
	public void particleActivation(bool e){
		uipd.setEnabled(e);
	}
	
	public override void draw(){
		if(currentScreen != null){
			//drawRect(-width/2f, height/2f, width, height, black, 0.6f);
			
			currentScreen.draw(this, true);
		}
		
		uipd.draw(this); //Render particles
	}
	
	protected override void onResize(int width, int height){
		projection = Matrix4.CreateOrthographic((float) width, (float) height, -1.0f, 1.0f);
		
		currentScreen?.updateProj(this);
	}
	
	public override void Dispose(){
		Engine.instance.onMouseMove -= mouse;
		Engine.instance.onParticleActivation -= particleActivation;
		base.Dispose();
	}
}