using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class MouseParticle : Particle{
	
	protected override Vector2 positionOffset => new Vector2(0f, lifeFactor * -20f);
	float _r;
	protected override float rotation => _r * lifeFactor;
	
	protected override Color3 color {get{
		byte c = (byte) ((byte) 200 - (byte) (150 * lifeFactor));
		return new Color3(c, c, c);
	}}
	
	protected override float alpha => 0.3f - 0.3f * lifeFactor;
	
	//protected override float alpha => 1f - 0.5f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public MouseParticle(Vector2d p){
		position = (Vector2) (p + new Vector2d(getRandom(-20f, 20f), getRandom(-20f, 20f)));
		_ml = (float) getRandom(0.4, 0.8);
		_r = (float) getRandom(-110, 110);
	}
	
	public override bool draw(Renderer ren){
		base.drawLocal(ren);
		
		return base.draw(ren);
	}
}