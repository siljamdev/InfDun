using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class FollowGoal : RouteGoal{
	protected Living following;
	protected float distanceSq;
	Vector2i _pt;
	protected override Vector2i posTarget => _pt;
	
	int updateCounter = 0; //Update position and route 
	
	public FollowGoal(Living e, Living f, float oq, Scene sce) : base(e){
		following = f;
		distanceSq = oq;
		_pt = following.position;
		buildRoute(sce);
	}
	
	public override bool isFinished(Scene sce){
		if(following.isDying || !sce.entitiesLiving.Contains(following)){
			return true;
		}
		return false;
	}
	
	public override bool act(Scene sce){
		if(following.distanceSquared(entity) < distanceSq){
			return false;
		}
		
		if(following.position != posTarget){
			updateCounter++;
		}
		
		if(updateCounter > 4){
			_pt = following.position;
			buildRoute(sce);
		}
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(entity.cat);
		#endif
		Vector2i? x2 = getNextMove(sce);
		
		if(entity.tryMove(x2, sce)){
			return true;
		}
		return false;
	}
	
	#if DEBUG_ENTITIES
		public override void drawDebug(Scene sce){
			base.drawDebug(sce);
			following.getBox().draw(sce);
		}
	#endif
}