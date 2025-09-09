using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class FriendParticle : Particle{
	
	static readonly Color3 col = new Color3("23FF7B");
	static readonly Color3 col2 = new Color3("00FFA5");
	
	protected override Vector2 positionOffset => new Vector2(0f, lifeFactor * 0.3f);
	protected override Vector2 scale => new Vector2(0.15f);
	
	Color3 _c;
	protected override Color3 color => _c;
	protected override float alpha => 0.8f - 0.4f * lifeFactor;
	
	//protected override float alpha => 1f - 0.5f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public FriendParticle(Vector2d p){
		position = (Vector2) (p + new Vector2d(getRandom(-0.3f, 0.3f), getRandom(-0.3f, 0.3f)));
		_ml = (float) getRandom(0.4, 0.9);
		_c = rand.Next(2) == 0 ? col : col2;
	}
	
	public override bool draw(Renderer ren){
		base.drawWorld(ren);
		
		return base.draw(ren);
	}
}