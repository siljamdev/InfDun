using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

abstract class RouteDiagonalGoal : EntityGoal{
	protected abstract Vector2i posTarget {get;}
	Queue<Vector2i> route = new(); //Route to follow
	
	#if DEBUG_ENTITIES
		LineStrip path;
	#endif
	
	public RouteDiagonalGoal(Living e) : base(e){}
	
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
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(7);
			#endif
			return null; //Fallback
		}
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(7);
		#endif
		return route.Dequeue();
	}
	
	#if DEBUG_ENTITIES
		public override void drawDebug(Scene sce){
			path.draw(sce);
		}
	#endif
	
	static readonly Vector2i[] directions = new Vector2i[]{new Vector2i(-1, -1), new Vector2i(0, -1), new Vector2i(1, -1), new Vector2i(-1, 0), new Vector2i(1, 0), new Vector2i(-1, 1), new Vector2i(0, 1), new Vector2i(1, 1)};
	
	protected void buildRoute(Scene sce){		
		Vector2i posTar = (Vector2i) posTarget;
		Vector2i position = entity.position;
		
		// BFS frontier
		var frontier = new Queue<Vector2i>();
		frontier.Enqueue(position);
		
		// Track where we came from
		var cameFrom = new Dictionary<Vector2i, Vector2i>();
		bool[,] visited = new bool[sce.mapXsize, sce.mapYsize];
		visited[position.X, position.Y] = true;
		
		Vector2i? closest = position;
		float closestDist = Vector2.DistanceSquared(position, posTar); // Use squared distance to avoid sqrt
		
		while (frontier.Count > 0){
			var current = frontier.Dequeue();
			
			float dist = Vector2.DistanceSquared(current, posTar);
			if(dist < closestDist){
				closestDist = dist;
				closest = current;
			}
			
			if (current == posTar){
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
		
		#if DEBUG_ENTITIES
			buildPath();
		#endif
	}
	
	#if DEBUG_ENTITIES
		void buildPath(){
			path = new LineStrip(route.Select(n => n + new Vector2(0f, 0.5f)).ToArray());
		}
	#endif
}