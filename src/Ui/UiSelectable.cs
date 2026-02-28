using System;
using OpenTK;
using OpenTK.Mathematics;

abstract class UiSelectable : UiElement{
	
	public bool selected;
	
	public UiSelectable(Placement p, Vector2 o) : base(p, o){
		
	}
	
	public UiSelectable(Placement p, float x, float y) : base(p, x, y){
		
	}
}