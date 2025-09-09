using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class MagicParticle : Particle{
	
	static readonly Color3 col = new Color3("59E5D9");
	
	Vector2 _f;
	protected override Vector2 positionOffset => lifeFactor * _f;
	float _r;
	protected override float rotation => _r * lifeFactor;
	protected override Vector2 scale => new Vector2(0.1f);
	
	protected override Color3 color => col;
	protected override float alpha => 0.8f - 0.4f * lifeFactor;
	
	//protected override float alpha => 1f - 0.5f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public MagicParticle(Vector2d p){
		position = (Vector2) (p + new Vector2d(getRandom(-0.2f, 0.2f), getRandom(-0.2f, 0.2f)));
		_ml = (float) getRandom(0.4, 1.1);
		_f = new Vector2((float) getRandom(-0.1f, 0.1f), (float) getRandom(0.1f, 0.5f));
		_r = (float) getRandom(-60, 60);
	}
	
	public override bool draw(Renderer ren){
		base.drawWorld(ren);
		
		return base.draw(ren);
	}
}