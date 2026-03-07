using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class FrogBloodParticle : BloodParticle{
	protected override Vector2 positionOffset => owner.getPos(sce) + new Vector2(0f, lifeFactor * -0.2f - 0.3f);
	
	public FrogBloodParticle(Scene s, Living l) : base(s, l){
		
	}
}