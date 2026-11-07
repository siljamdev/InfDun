using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class VoidParticle : Particle{
	
	Vector2 _d;
	protected override Vector2 positionOffset => lifeFactor * _d;
	
	float _s;
	protected override Vector2 scale => new Vector2(_s);
	
	Color3 _c;
	protected override Color3 color => _c;
	protected override float alpha => 0.8f - 0.6f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public VoidParticle(Vector2 p, Vector2 dir){
		position = p;
		_ml = getRandom(1.2f, 4.2f);
		byte gray = (byte) rand.Next(180);
		_c = new Color3(gray, gray, gray);
		_d = dir + new Vector2(getRandom(-1.5f, 1.5f), getRandom(-1.5f, 1.5f));
		_s = getRandom(0.03f, 0.12f);
	}
}