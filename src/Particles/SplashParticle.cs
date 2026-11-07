using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class SplashParticle : Particle{
	
	static readonly Color3 col = new Color3("6384DD");
	
	Vector2 _p;
	protected override Vector2 positionOffset => lifeFactor * _p;
	protected override Vector2 scale => new Vector2(0.05f);
	
	protected override Color3 color => col;
	protected override float alpha => 0.5f - 0.4f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public SplashParticle(Vector2 p){
		position = p;
		_ml = getRandom(0.2f, 0.4f);
		_p = new Vector2(getRandom(-0.3f, 0.3f), getRandom(0.05f, 0.3f));
	}
}