using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Goblin : Living{
	const int maxDistSq = 16*16;
	const int wanderDistSq = 48*48;
	
	int _c;
	protected override int atlasRow => _c;
	
	EntityGoal goal;
	
	public int stolenCoins = 0;
	
	#if DEBUG_TIME
		public override int cat => 0;
	#endif
	
	public Goblin(Vector2i p, Random rand) : base(p){
		_c = 1 + rand.Next(2);
		
		facingLeft = rand.Next(2) == 0;
	}
	
	public override void newLevel(Scene sce){
		setGoal(sce);
	}
	
	public override bool doTurn(Scene sce){
		if(goal != null && goal.isFinished(sce)){
			goal = null;
		}
		
		setGoal(sce);
		
		bool f = goal?.act(sce) ?? false;
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(cat);
		#endif
		return f;
	}
	
	void setGoal(Scene sce){
		if(goal != null){
			return;
		}
		
		if(sce.randSerious.Next(10) == 0){
			goal = AttackLazyGoal.anyRandom(sce, this, maxDistSq, 1);
		}else if(sce.randSerious.Next(3) == 0){
			goal = StealCoinLazyGoal.randomCoin(sce, this);
		}else{
			goal = WanderLazyGoal.aroundEntity(sce, this, wanderDistSq);
		}
	}
	
	public void befriend(Scene sce){
		isDying = true; //If someone has it as target, delete it
		sce.entitiesToRemove.Add(this);
		sce.entitiesToAdd.Add(new GoblinFriend(position, _c, facingLeft, health, sce));
		
		int c = sce.randCasual.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pd.add(new FriendParticle(position + new Vector2(0f, 0.5f)));
		}
	}
	
	protected override void onDeath(Scene sce, Living s){
		base.onDeath(sce, s);
		if(s == sce.p){
			sce.p.score += 10;
		}
		
		if(s is Necromancer n){
			n.addSkeleton(sce, position, facingLeft);
		}
		
		if(stolenCoins > 0){
			for(int i = 0; i < stolenCoins; i++){
				Coin c2 = new Coin(position);
				sce.entitiesToAdd.Add(c2);
			}
			
			int c = sce.randCasual.Next(2) + 1;
			for(int i = 0; i < c; i++){
				sce.sm.play(Coin.coinSound, new Vector3(position.X, position.Y, 0f), 0.5f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
			}
		}
	}
	
	protected override void onAttack(Scene sce, Living s){
		base.onAttack(sce, s);
		goal = new AttackLazyGoal(this, s, maxDistSq, 1);
	}
	
	public override int getAnimationFrame(Scene sce){
		if(isDying){
			return dyingAnimation?.frame ?? 4;
		}
		
		if(state == EntityState.idle){
			return sce.idleAnimation.frame;
		}else if(state == EntityState.moving){
			return sce.runningAnimation.frame;
		}
		
		return 0;
	}
	
	#if DEBUG_ENTITIES
		protected override void drawDebug(Scene sce){
			if(goal != null){
				Engine.instance.fd.draw(sce, goal.ToString(), position.X - Renderer.sceneTextSize.X / 2f, position.Y + 0.5f, Renderer.sceneTextSize, Renderer.textColor, 0.65f);
			}
			goal?.drawDebug(sce);
		}
	#endif
}