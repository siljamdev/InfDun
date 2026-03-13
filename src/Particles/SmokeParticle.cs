using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class SmokeParticle : Particle{
	
	protected override Vector2 positionOffset => new Vector2(0f, 0.2f * lifeFactor);
	
	float _s;
	protected override Vector2 scale => new Vector2(0.5f, _s);
	
	Color3 _c;
	protected override Color3 color => _c;
	protected override float alpha => 0.6f - 0.3f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public SmokeParticle(Vector2d p){
		position = (Vector2) (p + new Vector2d(-0.25f + getRandom(-0.5f, 0.5f), getRandom(-0.5f, 0.5f)));
		_ml = (float) getRandom(1.6, 4.2);
		
		byte gray = (byte) (90 + rand.Next(50));
		_c = new Color3(gray, gray, gray);
		
		_s = (float) getRandom(0.1f, 0.25f);
	}
	
	public override bool draw(Renderer ren){
		base.drawWorld(ren);
		
		return base.draw(ren);
	}
}