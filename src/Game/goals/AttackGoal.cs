using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class AttackGoal : EntityGoal{
	protected Living target;
	protected float distSq;
	protected int damage;
	
	public AttackGoal(Living e, Living at, float dq, int dmg) : base(e){
		target = at;
		distSq = dq;
		damage = dmg;
	}
	
	public override bool isFinished(Scene sce){
		if(target == null || target.isDying || target.distanceSquared(entity.position) > distSq || !sce.entitiesLiving.Contains(target)){
			return true;
		}
		return false;
	}
	
	public override bool act(Scene sce){
		if(entity.attack(damage, target, sce)){
			return true;
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
	
	public static AttackGoal closestTarget(Scene sce, Living entity, float distSq, int dmg, params Type[] types){
		Entity[] possible = sce.entitiesLiving.Where(n => (n != entity && types.Any(t => t.IsInstanceOfType(n))))
		.Where(n => n.distanceSquared(entity) < distSq).OrderBy(n => n.distanceSquared(entity)).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Living x2 = possible[0] as Living;
		
		return new AttackGoal(entity, x2, distSq, dmg);
	}
	
	public static AttackGoal randomTarget(Scene sce, Living entity, float distSq, int dmg, params Type[] types){
		Entity[] possible = sce.entitiesLiving.Where(n => (n != entity && types.Any(t => t.IsInstanceOfType(n))))
		.Where(n => n.distanceSquared(entity) < distSq).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Living x2 = possible[sce.randSerious.Next(possible.Length)] as Living;
		
		return new AttackGoal(entity, x2, distSq, dmg);
	}
	
	public static AttackGoal anyRandom(Scene sce, Living entity, float distSq, int dmg){
		Entity[] possible = sce.entitiesLiving.Where(n => n != entity)
		.Where(n => n.distanceSquared(entity) < distSq).ToArray();
		
		if(possible.Length == 0){
			return null;
		}
		
		Living x2 = possible[sce.randSerious.Next(possible.Length)] as Living;
		
		return new AttackGoal(entity, x2, distSq, dmg);
	}
	
	#if DEBUG_ENTITIES
		public override void drawDebug(Scene sce){
			target.getBox().draw(sce);
		}
	#endif
	
	static readonly Vector2i[] directions = new Vector2i[]{new Vector2i(0, 1), new Vector2i(0, -1), new Vector2i(-1, 0), new Vector2i(1, 0)};
	
	//Helper struct
	struct Node{
		public Vector2i pos;
		public Vector2i firstMove;
		public Node(Vector2i p, Vector2i f) { pos = p; firstMove = f; }
	}
	
	//Independent: old pathfinding
	protected static Vector2i? findFirstMove(Vector2i start, Vector2i target, Scene sce){
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
			if(current.pos == target){
				#if DEBUG_TIME
					Scene.tt.CategoryEnd(6);
				#endif
				return current.firstMove;
			}
	
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
	
			if(bestMove != start){
				#if DEBUG_TIME
					Scene.tt.CategoryEnd(6);
				#endif
				return bestMove;
			}
		}
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(6);
		#endif
		return null;
	}
}