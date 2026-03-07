using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using AshLib;
using AshLib.AshFiles;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

class KeyBind{
	#region static
	public static readonly List<KeyBind> configurables = new();
	
	public static AshFileModel getModel(){
		return new AshFileModel(configurables.Select(k => new ModelInstance(ModelInstanceOperation.Type, k.configKey, (int) k.key)).ToArray());
	}
	
	public static AshFileModel getSaveModel(){
		return new AshFileModel(configurables.Select(k => new ModelInstance(ModelInstanceOperation.Value, k.configKey, (int) k.key)).ToArray());
	}
	
	public static AshFileModel getResetModel(){
		return new AshFileModel(configurables.Select(k => new ModelInstance(ModelInstanceOperation.Value, k.configKey, (int) k.ogKey)).ToArray());
	}
	#endregion
	
	bool sticky;
	bool usesModifier;
	
	public Keys key{get; set;}
	Keys modifier;
	
	//Fields for config
	Keys ogKey;
	public string configKey{get; private set;}
	public string configDescription{get; private set;}
	
	public KeyBind(Keys k, bool s = false){
		key = k;
		sticky = s;
	}
	
	public KeyBind(Keys k, Keys m, bool s){
		key = k;
		modifier = m;
		sticky = s;
		usesModifier = true;
	}
	
	public KeyBind addToConfigurables(string key, string description){
		configurables.Add(this);
		ogKey = this.key;
		configKey = key;
		configDescription = description;
		return this;
	}
	
	public bool isActive(KeyboardState kbd){
		if(sticky){
			if(kbd.IsKeyDown(key)){
				return true;
			}else{
				return false;
			}
		}else{
			if(kbd.IsKeyPressed(key)){
				return true;
			}else{
				return false;
			}
		}
	}
	
	public byte isActiveMod(KeyboardState kbd){
		if(!usesModifier){
			return 0;
		}
		if(sticky){
			if(kbd.IsKeyDown(key)){
				if(kbd.IsKeyDown(modifier)){
					return 2;
				}
				return 1;
			}else{
				return 0;
			}
		}else{
			if(kbd.IsKeyPressed(key)){
				if(kbd.IsKeyDown(modifier)){
					return 2;
				}
				return 1;
			}else{
				return 0;
			}
		}
	}
}