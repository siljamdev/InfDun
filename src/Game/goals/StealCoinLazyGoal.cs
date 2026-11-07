using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class StealCoinLazyGoal : RouteGoal{
	Coin target;
	
	protected override Vector2i posTarget => target.position;
	
	const int distSq = 16*16;
	
	public StealCoinLazyGoal(Goblin e, Coin t, Scene sce) : base(e){
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
		if(target.onGoblinSteal(sce, (Goblin) entity)){
			((Goblin) entity).stolenCoins += 1;
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
	
	public static StealCoinLazyGoal randomCoin(Scene sce, Goblin entity){
		Entity[] possible = sce.entitiesItems.Where(n => n is Coin)
		.Where(n => n.distanceSquared(entity.position) < distSq).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Coin x2 = possible[sce.randSerious.Next(possible.Length)] as Coin;
		
		return new StealCoinLazyGoal(entity, x2, sce);
	}
	
	#if DEBUG_ENTITIES
		public override void drawDebug(Scene sce){
			base.drawDebug(sce);
			target.getBox().draw(sce);
		}
	#endif
}