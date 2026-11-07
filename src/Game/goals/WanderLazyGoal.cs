using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class WanderLazyGoal : WanderGoal{	
	public WanderLazyGoal(Living e, Vector2i t, Scene sce) : base(e, t, sce){
		
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
			turnsWithoutMove = 0;
			return true;
		}
		turnsWithoutMove++;
		return false;
	}
	
	public static new WanderLazyGoal aroundEntity(Scene sce, Living entity, float distSq){
		int[] posibleTiles = sce.transitableTiles.Select((v, i) => (v, i)).Where(n => n.Item1).Select(n => n.Item2).
		Where(n => entity.distanceSquared(sce.coordinatesOfFlatIndex(n)) < distSq).ToArray();
		
		if(posibleTiles.Length < 1){
			return null;
		}
		
		int x2 = posibleTiles[sce.randSerious.Next(posibleTiles.Length)];
		
		return new WanderLazyGoal(entity, (Vector2i) sce.coordinatesOfFlatIndex(x2), sce);
	}
	
	public static new WanderLazyGoal aroundArea(Scene sce, Living entity, Entity area, float distSq){
		int[] posibleTiles = sce.transitableTiles.Select((v, i) => (v, i)).Where(n => n.Item1).Select(n => n.Item2).
		Where(n => area.distanceSquared(sce.coordinatesOfFlatIndex(n)) < distSq).ToArray();
		
		if(posibleTiles.Length < 1){
			return null;
		}
		
		int x2 = posibleTiles[sce.randSerious.Next(posibleTiles.Length)];
		
		return new WanderLazyGoal(entity, (Vector2i) sce.coordinatesOfFlatIndex(x2), sce);
	}
}