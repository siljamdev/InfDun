using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class StealOrbGoal : RouteGoal{
	Orb target;
	
	protected override Vector2i posTarget => target.position;
	
	const int distSq = 32*32;
	
	public StealOrbGoal(Necromancer e, Orb t, Scene sce) : base(e){
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
		if(target.onNecromancerSteal(sce, (Necromancer) entity)){
			((Necromancer) entity).maxHealth += 1;
			return true;
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
	
	public static StealOrbGoal randomOrb(Scene sce, Necromancer entity){
		Entity[] possible = sce.entitiesItems.Where(n => n is Orb)
		.Where(n => n.distanceSquared(entity.position) < distSq).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Orb x2 = possible[sce.randSerious.Next(possible.Length)] as Orb;
		
		return new StealOrbGoal(entity, x2, sce);
	}
	
	#if DEBUG_ENTITIES
		public override void drawDebug(Scene sce){
			base.drawDebug(sce);
			target.getBox().draw(sce);
		}
	#endif
}