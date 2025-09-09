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
	
	public AltarParticle(Vector2d p){
		position = (Vector2) (p + new Vector2d(getRandom(-0.5f, 0.5f), getRandom(-0.5f, 0.5f)));
		_ml = (float) getRandom(0.6, 1.2);
		_c = new Color3((byte) rand.Next(256), (byte) rand.Next(256), (byte) rand.Next(256));
		_f = (float) getRandom(-0.5f, 0.5f);
	}
	
	public override bool draw(Renderer ren){
		base.drawWorld(ren);
		
		return base.draw(ren);
	}
}