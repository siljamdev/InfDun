using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class AttackLazyGoal : AttackGoal{
	
	public AttackLazyGoal(Living e, Living at, float dq, int dmg) : base(e, at, dq, dmg){
		
	}
	
	public override bool act(Scene sce){
		if(entity.attack(damage, target, sce)){
			return true;
		}
		
		if(sce.randSerious.Next(2) == 0){
			return false;
		}
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(entity.cat);
		#endif
		Vector2i? x2 = findFirstMove(entity.position, target.position, sce);
		
		if(entity.tryMove(x2, sce)){
			return true;
		}
		return false;
	}
	
	public static new AttackLazyGoal closestTarget(Scene sce, Living entity, float distSq, int dmg, params Type[] types){
		Entity[] possible = sce.entitiesLiving.Where(n => (n != entity && types.Any(t => t.IsInstanceOfType(n))))
		.Where(n => n.distanceSquared(entity) < distSq).OrderBy(n => n.distanceSquared(entity)).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Living x2 = possible[0] as Living;
		
		return new AttackLazyGoal(entity, x2, distSq, dmg);
	}
	
	public static new AttackLazyGoal randomTarget(Scene sce, Living entity, float distSq, int dmg, params Type[] types){
		Entity[] possible = sce.entitiesLiving.Where(n => (n != entity && types.Any(t => t.IsInstanceOfType(n))))
		.Where(n => n.distanceSquared(entity) < distSq).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Living x2 = possible[sce.randSerious.Next(possible.Length)] as Living;
		
		return new AttackLazyGoal(entity, x2, distSq, dmg);
	}
	
	public static new AttackLazyGoal anyRandom(Scene sce, Living entity, float distSq, int dmg){
		Entity[] possible = sce.entitiesLiving.Where(n => n != entity)
		.Where(n => n.distanceSquared(entity) < distSq).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Living x2 = possible[sce.randSerious.Next(possible.Length)] as Living;
		
		return new AttackLazyGoal(entity, x2, distSq, dmg);
	}
}