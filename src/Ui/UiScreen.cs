using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using AshLib;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

class UiScreen{
	#region static
	static SoundPool clickSound;
	//static SoundPool closeSound;
	
	static Random rand = new();
	
	public static bool uiSounds;
	
	public static void initialize(){
		clickSound = new SoundPool(Sound.monoFromAssembly("res.sounds.ui.click0.ogg"), Sound.monoFromAssembly("res.sounds.ui.click1.ogg"));
		//closeSound = new SoundPool(Sound.monoFromAssembly("res.sounds.ui.close.ogg"));
	}
	#endregion
	
	public List<UiElement> elements{get; private set;}
	
	public UiSelectable selected;
	
	public Action? closeAction;
	
	UiText? errorText;
	UiLog? scrollLog;
	
	public UiScreen(params UiElement[] b){
		elements = new List<UiElement>(b.Length);
		
		elements.AddRange(b);
	}
	
	public UiScreen setCloseAction(Action a){
		closeAction = a;
		return this;
	}
	
	public UiScreen addScrollingLog(UiLog g){
		elements.Add(g);
		scrollLog = g;
		return this;
	}
	
	public bool scroll(float f){
		return scrollLog?.scroll(f) ?? false;
	}
	
	public UiScreen addErrorText(UiText t){
		elements.Add(t);
		errorText = t;
		return this;
	}
	
	public void showError(string s){
		errorText?.setText(s);
	}
	
	public void draw(UIRenderer uren, bool doHover){
		Vector2 mouse = uren.mousePosition;
		
		foreach(UiElement b in elements){
			if(b.needsToUpdate){
				b.update(uren);
			}
			if(b.isActive){
				b.drawScissored(uren, mouse);
			}
		}
		
		if(doHover){
			foreach(UiElement b in elements){
				if(b.isActive && b.box != null && b.box.collide(mouse)){
					b.drawHover(uren, mouse);
					break;
				}
			}
		}
	}
	
	public bool click(UIRenderer uren, bool shiftPressed){
		Vector2 mouse = uren.mousePosition;
		
		//Last ones are rendered on top
		for(int i = elements.Count - 1; i >= 0; i--){
			UiElement b = elements[i];
			if(b.isActive && b.box != null && b.box.collide(mouse)){
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
				
				uren.uipd.add(new MouseParticle(mouse));
				uren.uipd.add(new MouseParticle(mouse));
				uren.uipd.add(new MouseParticle(mouse));
				uren.uipd.add(new MouseParticle(mouse));
				if(uiSounds){
					Engine.instance.sm.playNonPositional(clickSound.get(rand), 0.5f, 0.8f + (float)rand.NextDouble() * 0.1f);
				}
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
			selected.selected = false;
			selected = null;
			
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
	
	public void close(){
		if(selected != null){
			selected.selected = false;
			selected = null;
		}
		
		if(uiSounds){
			//ren.dun.sm.playNonPositional(closeSound.get(rand), 0.5f, 0.8f + (float)rand.NextDouble() * 0.1f);
		}
		
		closeAction?.Invoke();
	}
	
	public void updateProj(UIRenderer uren){
		foreach(UiElement b in elements){
			b.update(uren);
		}
	}
}