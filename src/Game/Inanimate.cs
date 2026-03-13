using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

//Only for grouping entities
abstract class Inanimate : Entity{
	public Inanimate(Vector2i p) : base(p){
		
	}
}