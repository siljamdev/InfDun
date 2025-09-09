using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using AshLib;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

class UiScreen{	
	public List<UiElement> elements{get; private set;}
	
	public UiSelectable selected;
	
	public Action? closeAction;
	
	UiText? errorText;
	UiLog? scrollLog;
	
	public UiScreen(params UiElement[] b){
		elements = new List<UiElement>();
		
		elements.AddRange(b);
	}
	
	public UiScreen setCloseAction(Action a){
		closeAction = a;
		return this;
	}
	
	public UiScreen setScrollingLog(UiLog g){
		elements.Add(g);
		scrollLog = g;
		return this;
	}
	
	public bool scroll(float f){
		if(scrollLog != null){
			return scrollLog.scroll(f);
		}
		
		return false;
	}
	
	public UiScreen setErrorText(UiText t){
		elements.Add(t);
		errorText = t;
		return this;
	}
	
	public void showError(string s){
		errorText?.setText(s);
	}
	
	public void draw(Renderer ren, bool doHover){
		Vector2d mouse = ren.cam.mouseLastPos - new Vector2d(ren.width / 2f, ren.height / 2f);
		mouse.Y = -mouse.Y;
		
		foreach(UiElement b in elements){
			if(b.active){
				if(b.needGen){
					b.update(ren);
				}
				b.draw(ren, mouse);
			}
		}
		
		if(doHover){
			foreach(UiElement b in elements){
				if(b.active && b.hasHover && b.box != null && b.box % mouse){
					b.drawHover(ren, mouse);
				}
			}
		}
	}
	
	public bool click(Renderer ren, bool shiftPressed){
		Vector2d mouse = ren.cam.mouseLastPos - new Vector2d(ren.width / 2f, ren.height / 2f);
		mouse.Y = -mouse.Y;
		
		//Last ones are rendered on top
		for(int i = elements.Count - 1; i >= 0; i--){
			UiElement b = elements[i];
			if(b.active && b.box != null && b.box % mouse){
				if(b is UiSelectable s){
					if(selected != null){
						selected.selected = false;
					}
					
					selected = s;
					selected.selected = true;
				}else{
					if(selected != null){
						selected.selected = false;
						selected = null;
					}
					
					if(b is UiClickable c){
						if(shiftPressed){
							c.quickAction?.Invoke();
						}else{
							c.action?.Invoke();
						}
					}
				}
				
				ren.uipr.add(new MouseParticle(mouse));
				ren.uipr.add(new MouseParticle(mouse));
				ren.uipr.add(new MouseParticle(mouse));
				ren.uipr.add(new MouseParticle(mouse));
				return true;
			}
		}
		
		//If nothing was clicked, the currently selected one is no longer selected
		if(selected != null){
			selected.selected = false;
			selected = null;
		}
		return false;
	}
	
	public bool trySetKeybind(Keys k){
		if(selected is UiKeyField kf){
			kf.key.key = k;
			return true;
		}
		
		return false;
	}
	
	public bool tryAddStr(string s){
		if(selected is UiField f){
			return f.addStr(s);
		}
		
		return false;
	}
	
	public bool tryDelChar(){
		if(selected is UiField f){
			return f.delChar();
		}
		
		return false;
	}
	
	public void updateProj(Renderer ren){
		foreach(UiElement b in elements){
			b.update(ren);
		}
	}
}