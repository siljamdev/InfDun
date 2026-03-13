using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class DoorParticle : Particle{
	
	static readonly Color3 col = new Color3("B28D7A");
	
	protected override Vector2 positionOffset => new Vector2(lifeFactor * -0.07f, lifeFactor * -0.4f);
	float _r;
	protected override float rotation => _r * lifeFactor;
	protected override Vector2 scale => new Vector2(0.1f);
	
	protected override Color3 color => col;
	protected override float alpha => 0.8f - 0.3f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public DoorParticle(Vector2d p){
		position = (Vector2) (p + new Vector2d(getRandom(-0.5f, 0.5f), getRandom(-0.5f, 0.5f)));
		_ml = (float) getRandom(0.4, 0.65);
		_r = (float) getRandom(-25, 25);
	}
	
	public override bool draw(Renderer ren){
		base.drawWorld(ren);
		
		return base.draw(ren);
	}
}