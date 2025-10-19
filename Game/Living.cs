using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

abstract class Living : Entity{
	#region static
	protected static Sound deathSound;
	protected static Sound hurtSound;
	protected static SoundPool stepSound;
	
	public static void initialize(){
		deathSound = Sound.monoFromAssembly("res.sounds.death.ogg");
		hurtSound = Sound.monoFromAssembly("res.sounds.hurt.ogg");
		
		stepSound = new SoundPool(Sound.monoFromAssembly("res.sounds.step0.ogg"), Sound.monoFromAssembly("res.sounds.step1.ogg"), Sound.monoFromAssembly("res.sounds.step2.ogg"));
		
		Player.initialize();
		Skeleton.initialize();
		Necromancer.initialize();
	}
	#endregion static
	
	protected bool isHurt;
	
	public int health{get; set;}
	public int maxHealth{get; set;}
	public bool isDying{get; protected set;}
	protected Animation dyingAnimation;
	
	public Living(){
		maxHealth = 3;
		health = maxHealth;
	}
	
	public Living(Vector2i p) : base(p){
		maxHealth = 3;
		health = maxHealth;
	}
	
	public bool tryMove(Vector2i? p, Scene sce){
		if(!sce.isAvailableToMoveFast(p)){ //Null returns false
			return false;
		}
		
		oldPosition = position;
		position = (Vector2i) p;
		state = EntityState.moving;
		
		if(position.X < oldPosition.X){
			facingLeft = true;
		}else if(position.X > oldPosition.X){
			facingLeft = false;
		}
		
		onMove(sce);
		
		return true;
	}
	
	public bool damage(int d, Living s, Scene sce){
		if(isDying){
			return false;
		}
		health -= d;
		isHurt = true;
		onAttack(sce, s);
		if(health <= 0){
			isDying = true;
			onDeath(sce, s);
		}else{
			onHurt(sce, s);
		}
		
		return true;
	}
	
	public bool tryDamageBy(int d, Living s, Scene sce){
		if(isDying){
			return false;
		}
		if(distance(s) > 1.5f){
			return false;
		}
		health -= d;
		isHurt = true;
		onAttack(sce, s);
		if(health <= 0){
			isDying = true;
			onDeath(sce, s);
		}else{
			onHurt(sce, s);
		}
		
		return true;
	}
	
	protected bool attack(int d, Living s, Scene sce){
		if(s != null && s.tryDamageBy(d, this, sce)){
			state = EntityState.attacking;
			if(s.position.X > position.X){
				facingLeft = false;
			}else if(s.position.X < position.X){
				facingLeft = true;
			}
			
			return true;
		}
		
		return false;
	}
	
	protected virtual void onMove(Scene sce){
		sce.sm.play(stepSound.get(sce.rand), new Vector3(position.X, position.Y, 0f), 0.5f, 0.8f + (float)sce.rand.NextDouble() * 0.4f);
	}
	
	protected virtual void onHurt(Scene sce, Living s){
		sce.sm.play(hurtSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.rand.NextDouble() * 0.7f);
	}
	
	protected virtual void onDeath(Scene sce, Living s){
		sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.rand.NextDouble() * 0.7f);
	}
	
	protected virtual void onAttack(Scene sce, Living s){
		int c = sce.rand.Next(5) + 2;
		for(int i = 0; i < c; i++){
			sce.pr.add(new BloodParticle(sce, this));
		}
	}
	
	public virtual bool doTurn(Scene sce){
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(5);
		#endif
		return false;
	}
	
	public override void endSmooth(Scene sce){	
		if(!isDying){
			isHurt = false;
		}
		
		if(state == EntityState.attacking){
			state = EntityState.idle;
		}
		
		base.endSmooth(sce);
	}
	
	public override bool onClick(Scene sce){
		return sce.p.attack(1, this, sce);
	}
	
	public override Vector2 getPos(Scene sce){
		Vector2 pos;
		if(state == EntityState.moving){
			pos = (Vector2) ((Vector2d) oldPosition + (Vector2d) (position - oldPosition) * (sce.smoothTime/sce.smoothTimeMax));
		}else{
			pos = (Vector2) position;
		}
		
		if(isHurt && !isDying){
			float x = (float) (sce.smoothTime * (1d/sce.smoothTimeMax));
			pos.Y += -x*x + x;
		}
		
		return pos;
	}
	
	protected override Matrix4 getModel(Scene sce){
		Vector2 pos = getPos(sce);
		
		Matrix4 r = Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		if(facingLeft){
			r = leftRotation * r;
		}
		
		if(state == EntityState.attacking){
			float x = (float) (sce.smoothTime * (1d/sce.smoothTimeMax));
			float y = -x*x + x;
			y *= -20f;
			r = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(y)) * r;
		}
		
		return r;
	}
	
	public override float getAlpha() => (dyingAnimation != null && dyingAnimation.frame > 4 && dyingAnimation.frame < 6) ? (float) (1d - dyingAnimation.time/dyingAnimation.maxTime) : 1f;
	
	public override bool getHurt() => isHurt;
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		if(isDying && dyingAnimation == null){
			dyingAnimation = new Animation(16, 4, sce.smoothTimeMax); //4 and 5, 16 is as a safeguard
		}
		
		if(isDying){
			doCollision = false;
			dyingAnimation.tick(Dungeon.dh.deltaTime);
			if(dyingAnimation.frame > 5){
				state = EntityState.idle;
				sce.entitiesToRemove.Add(this);
			}
		}
		
		/* AABB b = new AABB(position.Y + 1d, position.Y, position.X - 0.5d, position.X + 0.5d);
		
		//b.drawWorld();
		
		if(sce.cam.mouseWorldPos % b){
			path?.drawWorld();
		} */
	}
	
	//Sorry for fields not being at the top, but this is much clearer
	#region pathfinding
	
	protected Vector2i? posTarget{get; private set;} //Final target
	protected Queue<Vector2i> route = new(); //Route to follow
	
	LineStrip path;
	
	static readonly Vector2i[] directions = new Vector2i[]{new Vector2i(0, 1), new Vector2i(0, -1), new Vector2i(-1, 0), new Vector2i(1, 0)};
	
	protected void setPosTarget(Scene sce, Vector2i? v){
		posTarget = v;
		buildRoute(sce);
	}
	
	protected Vector2i? getNextMove(Scene sce){
		if(route.Count == 0){
			buildRoute(sce);
		}else{
			foreach(Vector2i s in route){
				if(!sce.isAvailableToMoveFast(s)){
					buildRoute(sce);
					break;
				}
			}
		}
		
		if(route.Count == 0){
			return null; //Fallback
		}
		
		return route.Dequeue();
	}
	
	void buildRoute(Scene sce){
		if(posTarget == null){
			route = new Queue<Vector2i>();
			buildPath();
			return;
		}
		
		Vector2i posTar = (Vector2i) posTarget;
		
		// BFS frontier
		var frontier = new Queue<Vector2i>();
		frontier.Enqueue(position);
		
		// Track where we came from
		var cameFrom = new Dictionary<Vector2i, Vector2i>();
		bool[,] visited = new bool[sce.mapXsize, sce.mapYsize];
		visited[position.X, position.Y] = true;
		
		Vector2i? closest = position;
		float closestDist = Vector2.DistanceSquared(position, posTar); // Use squared distance to avoid sqrt
		
		bool found = false;
		
		while (frontier.Count > 0){
			var current = frontier.Dequeue();
			
			float dist = Vector2.DistanceSquared(current, posTar);
			if(dist < closestDist){
				closestDist = dist;
				closest = current;
			}
			
			if (current == posTar){
				found = true;
				break;
			}
			
			foreach (var d in directions){
				var next = current + d;
				
				if (!visited[next.X, next.Y] && sce.isAvailableToMoveFast(next)){
					frontier.Enqueue(next);
					visited[next.X, next.Y] = true;
					cameFrom[next] = current;
				}
			}
		}
		
		// reconstruct route to closest reachable position
		if (closest != null && closest != position){
			var path = new Stack<Vector2i>();
			var step = closest.Value;
			
			while (step != position){
				path.Push(step);
				step = cameFrom[step];
			}
			
			route = new Queue<Vector2i>(path);
		}else{
			route = new Queue<Vector2i>(); // empty if unreachable
		}
		
		buildPath();
	}
	
	void buildPath(){
		path = new LineStrip(route.Select(i => (Vector2d) i).Select(n => n + new Vector2(0f, 0.5f)).ToArray());
	}
	
	//Helper struct
	struct Node{
		public Vector2i pos;
		public Vector2i firstMove;
		public Node(Vector2i p, Vector2i f) { pos = p; firstMove = f; }
	}
	
	//Independent
	protected static Vector2i? findFirstMove(Vector2i start, Vector2i target, Scene sce)
	{
		// Fast visited array
		bool[,] visited = new bool[sce.mapXsize, sce.mapYsize];
		visited[start.X, start.Y] = true;
	
		// Queue
		var queue = new Queue<Node>();
	
		// Enqueue first moves
		foreach (Vector2i dir in directions)
		{
			Vector2i next = start + dir;
			if ((next == target || sce.isAvailableToMoveFast(next)) && !visited[next.X, next.Y])
			{
				queue.Enqueue(new Node(next, next));
				visited[next.X, next.Y] = true;
			}
		}
	
		// Track closest reachable node to target
		Vector2i closest = start;
		int closestDist = Math.Abs(start.X - target.X) + Math.Abs(start.Y - target.Y);
	
		while (queue.Count > 0)
		{
			Node current = queue.Dequeue();
	
			// Update closest node
			int dist = Math.Abs(current.pos.X - target.X) + Math.Abs(current.pos.Y - target.Y);
			if (dist < closestDist)
			{
				closestDist = dist;
				closest = current.pos;
			}
	
			// Return immediately if we reached the target
			if (current.pos == target)
				return current.firstMove;
	
			// Enqueue neighbors
			foreach (Vector2i dir in directions)
			{
				Vector2i next = current.pos + dir;
				if ((next == target || sce.isAvailableToMoveFast(next)) && !visited[next.X, next.Y])
				{
					queue.Enqueue(new Node(next, current.firstMove));
					visited[next.X, next.Y] = true;
				}
			}
		}
	
		// If target unreachable, pick the best first step toward the closest node
		if (closest != start)
		{
			Vector2i bestMove = start;
			int bestDist = Math.Abs(start.X - closest.X) + Math.Abs(start.Y - closest.Y);
	
			foreach (Vector2i dir in directions)
			{
				Vector2i next = start + dir;
				if ((next == target || sce.isAvailableToMoveFast(next)))
				{
					int dist = Math.Abs(next.X - closest.X) + Math.Abs(next.Y - closest.Y);
					if (dist < bestDist)
					{
						bestDist = dist;
						bestMove = next;
					}
				}
			}
	
			if (bestMove != start)
				return bestMove;
		}
	
		return null;
	}
	#endregion pathfinding
}