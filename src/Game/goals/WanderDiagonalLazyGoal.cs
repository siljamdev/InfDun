using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class WanderDiagonalLazyGoal : RouteDiagonalGoal{
	Vector2i target;
	
	protected override Vector2i posTarget => target;
	
	public WanderDiagonalLazyGoal(Living e, Vector2i t, Scene sce) : base(e){
		target = t;
		buildRoute(sce);
	}
	
	public override bool isFinished(Scene sce){
		if(target == entity.position){
			return true;
		}
		return false;
	}
	
	public override bool act(Scene sce){
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
	
	public static WanderDiagonalLazyGoal aroundEntity(Scene sce, Living entity, float distSq){
		int[] posibleTiles = sce.transitableTiles.Select((v, i) => (v, i)).Where(n => n.Item1).Select(n => n.Item2).
		Where(n => entity.distanceSquared(sce.coordinatesOfFlatIndex(n)) < distSq).ToArray();
		
		if(posibleTiles.Length < 1){
			return null;
		}
		
		int x2 = posibleTiles[sce.randSerious.Next(posibleTiles.Length)];
		
		return new WanderDiagonalLazyGoal(entity, (Vector2i) sce.coordinatesOfFlatIndex(x2), sce);
	}
	
	public static WanderDiagonalLazyGoal aroundArea(Scene sce, Living entity, Entity area, float distSq){
		int[] posibleTiles = sce.transitableTiles.Select((v, i) => (v, i)).Where(n => n.Item1).Select(n => n.Item2).
		Where(n => area.distanceSquared(sce.coordinatesOfFlatIndex(n)) < distSq).ToArray();
		
		if(posibleTiles.Length < 1){
			return null;
		}
		
		int x2 = posibleTiles[sce.randSerious.Next(posibleTiles.Length)];
		
		return new WanderDiagonalLazyGoal(entity, (Vector2i) sce.coordinatesOfFlatIndex(x2), sce);
	}
}