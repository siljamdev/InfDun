using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class BloodParticle : Particle{
	
	protected Living owner;
	protected Scene sce;
	
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
		
		position = new Vector2(getRandom(-0.2f, 0.2f), 0.6f + getRandom(-0.2f, 0.2f));
		_ml = getRandom(0.4f, 0.8f);
		_r = getRandom(-60f, 60f);
	}
}