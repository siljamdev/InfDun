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
	
	public SplashParticle(Vector2d p){
		position = (Vector2) p;
		_ml = (float) getRandom(0.2, 0.4);
		_p = (Vector2) new Vector2d(getRandom(-0.3, 0.3), getRandom(0.05, 0.3));
	}
	
	public override bool draw(Renderer ren){
		base.drawWorld(ren);
		
		return base.draw(ren);
	}
}