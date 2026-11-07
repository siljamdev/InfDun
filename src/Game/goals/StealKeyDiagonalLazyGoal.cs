using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class StealKeyDiagonalLazyGoal : RouteDiagonalGoal{
	Key target;
	
	protected override Vector2i posTarget => target.position;
	
	const int distSq = 48*48;
	
	public StealKeyDiagonalLazyGoal(Frog e, Key t, Scene sce) : base(e){
		target = t;
		buildRoute(sce);
	}
	
	public override bool isFinished(Scene sce){
		if(!sce.entitiesItems.Contains(target)){
			return true;
		}
		return false;
	}
	
	public override bool act(Scene sce){
		if(target.onFrogSteal(sce, (Frog) entity)){
			((Frog) entity).hasKey = true;
			return true;
		}
		
		if(sce.randSerious.Next(2) == 0){
			return false;
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
	
	public static StealKeyDiagonalLazyGoal randomKey(Scene sce, Frog entity){
		Entity[] possible = sce.entitiesItems.Where(n => n is Key)
		.Where(n => n.distanceSquared(entity.position) < distSq).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Key x2 = possible[sce.randSerious.Next(possible.Length)] as Key;
		
		return new StealKeyDiagonalLazyGoal(entity, x2, sce);
	}
	
	#if DEBUG_ENTITIES
		public override void drawDebug(Scene sce){
			base.drawDebug(sce);
			target.getBox().draw(sce);
		}
	#endif
}