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
	
	public DoorParticle(Vector2 p){
		position = p + new Vector2(getRandom(-0.5f, 0.5f), getRandom(-0.5f, 0.5f));
		_ml = getRandom(0.4f, 0.65f);
		_r = getRandom(-25f, 25f);
	}
}