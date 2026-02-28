using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class DripParticle : Particle{
	
	static readonly Color3 col = new Color3("6384DD");
	
	protected override Vector2 positionOffset => new Vector2(0f, -4.9f * life * life);
	protected override Vector2 scale => new Vector2(0.1f);
	
	protected override Color3 color => col;
	protected override float alpha => 0.7f;
	
	protected override float maxLife => 0.5f;
	
	public DripParticle(Vector2d p){
		position = (Vector2) (p + new Vector2d(getRandom(-0.15f, 0.15f), 1.2f));
	}
	
	public override bool draw(Renderer ren){
		base.drawWorld(ren);
		
		return base.draw(ren);
	}
}