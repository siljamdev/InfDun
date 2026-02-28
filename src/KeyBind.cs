using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

class KeyBind{
	bool sticky;
	
	bool usesModifier;
	
	public Keys key{get; set;}
	Keys modifier;
	
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
	
	public static bool getHexTyping(string s){
		for(int i = 0; i < s.Length; i++){
			if(!Uri.IsHexDigit(s[i])){
				return false;
			}
		}
		return true;
	}
	
	public static bool getFloatPositiveTyping(string s){
		for(int i = 0; i < s.Length; i++){
			if(!(char.IsDigit(s[i]) || s[i] == '.')){
				return false;
			}
		}
		return true;
	}
	
	public static bool getFloatTyping(string s){
		for(int i = 0; i < s.Length; i++){
			if(!(char.IsDigit(s[i]) || s[i] == '.' || s[i] == '-')){
				return false;
			}
		}
		return true;
	}
	
	public static bool getIntTyping(string s){
		for(int i = 0; i < s.Length; i++){
			if(!char.IsDigit(s[i])){
				return false;
			}
		}
		return true;
	}
}