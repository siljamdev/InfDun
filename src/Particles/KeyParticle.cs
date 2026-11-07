using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class KeyParticle : Particle{
	
	static readonly Color3 col = new Color3("FFD400");
	
	protected override Vector2 positionOffset => new Vector2(0f, lifeFactor * -0.3f);
	protected override Vector2 scale => new Vector2(0.1f);
	
	float _r;
	protected override float rotation => _r * lifeFactor;
	
	protected override Color3 color => col;
	protected override float alpha => 0.8f - 0.8f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public KeyParticle(Vector2 p){
		position = p + new Vector2(getRandom(-0.15f, 0.15f), getRandom(-0.15f, 0.15f));
		_ml = getRandom(0.6f, 0.9f);
		_r = getRandom(-110f, 110f);
	}
}