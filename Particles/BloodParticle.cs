using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class BloodParticle : Particle{
	
	Living owner;
	Scene sce;
	
	protected override Vector2 positionOffset => owner.getPos(sce) + new Vector2(0f, lifeFactor * -0.4f);
	float _r;
	protected override float rotation => _r * lifeFactor;
	protected override Vector2 scale => new Vector2(0.1f);
	
	protected override Color3 color {get{
		byte c = (byte) ((byte) 200 - (byte) (80 * lifeFactor));
		return new Color3(c, 0, 0);
	}}
	protected override float alpha => 0.8f - 0.4f * lifeFactor;
	
	//protected override float alpha => 1f - 0.5f * lifeFactor;
	
	float _ml;
	protected override float maxLife => _ml;
	
	public BloodParticle(Scene s, Living l){
		sce = s;
		owner = l;
		
		position = (Vector2) new Vector2d(getRandom(-0.2f, 0.2f), 0.6d + getRandom(-0.2f, 0.2f));
		_ml = (float) getRandom(0.4, 0.8);
		_r = (float) getRandom(-60, 60);
	}
	
	public override bool draw(Renderer ren){
		base.drawWorld(ren);
		
		return base.draw(ren);
	}
}