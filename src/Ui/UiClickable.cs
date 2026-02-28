using System;
using OpenTK;
using OpenTK.Mathematics;

abstract class UiClickable : UiElement{
	public Action? action;
	public Action? quickAction;
	
	public UiClickable(Placement p, Vector2 o) : base(p, o){
		
	}
	
	public UiClickable(Placement p, float x, float y) : base(p, x, y){
		
	}
	
	public UiClickable setAction(Action? a){
		action = a;
		return this;
	}
	
	public UiClickable setQuickAction(Action? a){
		quickAction = a;
		return this;
	}
}