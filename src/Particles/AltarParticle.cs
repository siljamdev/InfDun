using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class AltarParticle : Particle{
	
	float _f;
	protected override Vector2 positionOffset => new Vector2(lifeFactor * _f, lifeFactor * 0.5f);
	protected override Vector2 scale => new Vector2(0.12f);
	
	Color3 _c;
	protected override Color3 color => _c;
	protected override float alpha => 0.8f - 0.4f * lifeFactor;
	
	//protected override float alpha => 1f - 0.5f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public AltarParticle(Vector2 p){
		position = p + new Vector2(getRandom(-0.5f, 0.5f), getRandom(-0.5f, 0.5f));
		_ml = getRandom(0.6f, 1.2f);
		_c = new Color3((byte) rand.Next(256), (byte) rand.Next(256), (byte) rand.Next(256));
		_f = getRandom(-0.5f, 0.5f);
	}
}