using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class FleeDiagonalGoal : EntityGoal{
	Living threat;
	float distSq;
	
	public FleeDiagonalGoal(Living e, Living at, float dq) : base(e){
		threat = at;
		distSq = dq;
	}
	
	public override bool isFinished(Scene sce){
		if(threat == null || threat.isDying || threat.distanceSquared(entity.position) > distSq || !sce.entitiesLiving.Contains(threat)){
			return true;
		}
		return false;
	}
	
	public override bool act(Scene sce){
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(entity.cat);
		#endif
		Vector2i? x2 = findFirstFleeMove(entity.position, threat.position, sce);
		
		if(entity.tryMove(x2, sce)){
			return true;
		}
		return false;
	}
	
	#if DEBUG_ENTITIES
		public override void drawDebug(Scene sce){
			threat.getBox().draw(sce);
		}
	#endif
	
	static readonly Vector2i[] directions = new Vector2i[]{new Vector2i(-1, -1), new Vector2i(0, -1), new Vector2i(1, -1), new Vector2i(-1, 0), new Vector2i(1, 0), new Vector2i(-1, 1), new Vector2i(0, 1), new Vector2i(1, 1)};
	
	//Additional data for better results
	struct Node{
		public Vector2i pos;
		public Vector2i firstMove;
		public int depth;
		public float penalty;
	
		public Node(Vector2i p, Vector2i f, int d, float pen){
			pos = p;
			firstMove = f;
			depth = d;
			penalty = pen;
		}
	}
	
	static Vector2i? findFirstFleeMove(Vector2i start, Vector2i threat, Scene sce){
		const int searchRadius = 8;
		const int localRadius = 3;
		
		//Cache
		bool[,] visited = new bool[sce.mapXsize, sce.mapYsize];
		int[,] openSpaceCache = new int[sce.mapXsize, sce.mapYsize];
		bool[,] openSpaceCalculated = new bool[sce.mapXsize, sce.mapYsize];
	
		Queue<Node> queue = new Queue<Node>();
	
		Vector2i bestMove = start;
		float bestScore = float.MinValue;
	
		int startDist = Math.Abs(start.X - threat.X) + Math.Abs(start.Y - threat.Y);
	
		visited[start.X, start.Y] = true;
	
		foreach(Vector2i dir in directions){
			Vector2i next = start + dir;
	
			if(!sce.isAvailableToMoveFast(next))
				continue;
	
			visited[next.X, next.Y] = true;
	
			int nextDist = Math.Abs(next.X - threat.X) + Math.Abs(next.Y - threat.Y);
			float penalty = nextDist < startDist ? (startDist - nextDist) * 10f : 0f;
	
			queue.Enqueue(new Node(next, next, 1, penalty));
		}
	
		while(queue.Count > 0){
			Node current = queue.Dequeue();
	
			int distance = Math.Abs(current.pos.X - threat.X) + Math.Abs(current.pos.Y - threat.Y);
	
			int escapeDirections = 0;
	
			foreach(Vector2i dir in directions){
				if(sce.isAvailableToMoveFast(current.pos + dir))
					escapeDirections++;
			}
			
			//How open a place is in localRadius
			int openSpace;
	
			if(openSpaceCalculated[current.pos.X, current.pos.Y]){
				openSpace = openSpaceCache[current.pos.X, current.pos.Y];
			}else{
				openSpace = 0;
	
				for(int x = -localRadius; x <= localRadius; x++){
					for(int y = -localRadius; y <= localRadius; y++){
						Vector2i p = current.pos + new Vector2i(x, y);
	
						if(p.X >= 0 && p.Y >= 0 && p.X < sce.mapXsize && p.Y < sce.mapYsize && sce.isAvailableToMoveFast(p))
							openSpace++;
					}
				}
	
				openSpaceCache[current.pos.X, current.pos.Y] = openSpace;
				openSpaceCalculated[current.pos.X, current.pos.Y] = true;
			}
	
			int progress = distance - startDist;
	
			float score =
				distance * 5f +
				progress * 3f +
				openSpace * 2f +
				escapeDirections * 5f -
				current.penalty -
				current.depth * 0.5f;
	
			if(score > bestScore){
				bestScore = score;
				bestMove = current.firstMove;
			}
	
			if(current.depth >= searchRadius) //Prevent too far
				continue;
	
			foreach(Vector2i dir in directions){
				Vector2i next = current.pos + dir;
	
				if(!sce.isAvailableToMoveFast(next) || visited[next.X, next.Y])
					continue;
	
				visited[next.X, next.Y] = true;
	
				int nextDist = Math.Abs(next.X - threat.X) + Math.Abs(next.Y - threat.Y);
	
				float penalty = current.penalty;
	
				if(nextDist < distance)
					penalty += (distance - nextDist) * 10f;
	
				queue.Enqueue(new Node(next, current.firstMove, current.depth + 1, penalty));
			}
		}
	
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(10);
		#endif
	
		return bestMove == start ? null : bestMove;
	}
}