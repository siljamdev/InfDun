using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class SmokeParticle : Particle{
	
	float _a;
	protected override Vector2 positionOffset => new Vector2(0f, _a * lifeFactor);
	
	float _s;
	protected override Vector2 scale => new Vector2(0.5f, _s - 0.08f * lifeFactor);
	
	Color3 _c;
	protected override Color3 color => _c;
	protected override float alpha => 0.6f - 0.4f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public SmokeParticle(Vector2 p){
		position = p + new Vector2(-0.25f + getRandom(-0.5f, 0.5f), getRandom(-0.5f, 0.5f));
		_ml = getRandom(1.6f, 4.2f);
		
		byte gray = (byte) (90 + rand.Next(50));
		_c = new Color3(gray, gray, gray);
		
		_s = getRandom(0.1f, 0.25f);
		_a = getRandom(0.2f, 0.5f);
	}
}