using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class LevelGenerator{
	
	readonly WeightedRandom roomPopulator;
	
	Random rand;
	
	List<Room> rooms;
	List<Entity> entities;
	bool[,] tiles;
	
	int levelNum;
	
	public LevelGenerator(Random r){
		rand = r;
		
		roomPopulator = new WeightedRandom();
		roomPopulator.Add(30); //0, Empty
		roomPopulator.Add(20); //1, Goblin
		roomPopulator.Add(10); //2, FastGoblin
		roomPopulator.Add(10); //3, MedKit
		roomPopulator.Add(20); //4, Coin
		roomPopulator.Add(8); //5, Orb
		roomPopulator.Add(3); //6, Altar
		roomPopulator.Add(3); //7, BefriendingAltar
		roomPopulator.Add(2); //8, Necromancer
	}
	
	public (bool[,], List<Entity>, Vector2i) generate(int num){
		levelNum = num;
		
		(int width, int height) = generateSize();
		
		tiles = new bool[height, width];
		
		enableEntities();
		
		entities = new();
		
		rooms = new();
		
		int maxRoomSize = Math.Min(6 + 2 * levelNum, 12);
		
		int min = 4 + (width * height) / 600;
		
		int roomCount = rand.Next(min, min + (width * height) / 400);
		for(int i = 0; i < roomCount; i++){
			int w = rand.Next(4, maxRoomSize);
			int h = rand.Next(4, maxRoomSize);
			int x = rand.Next(1, width - w - 1);
			int y = rand.Next(1, height - h - 1);
			
			Room newRoom = new Room(x, y, w, h);
			if(rooms.Any(r => r.intersects(newRoom))){
				i--;
				continue;
			}
			
			for(int x2 = newRoom.X; x2 < newRoom.X + newRoom.W; x2++){
				for(int y2 = newRoom.Y; y2 < newRoom.Y + newRoom.H; y2++){
					tiles[y2, x2] = true;
				}
			}
			
			if(rooms.Count > 0){
				int connections = Math.Min(rooms.Count, 1 + rand.Next(2));
				for(int k = 0; k < connections; k++){
					Room target = selectRoomForCorridor(newRoom);
					newRoom.connections.Add(target);
					createCorridor(newRoom, target);
				}
			}
			rooms.Add(newRoom);
		}
		
		bool get(int y, int x){
			if(y >= 0 && y < height && x >= 0 && x < width){
				return tiles[y, x];
			}
			return false;
		}
		
		//Ensure there is no weird single wall tile
		for(int i = 0; i < height; i++){
			for(int j = 0; j < width; j++){
				if(!tiles[i, j] && get(i + 1, j) && get(i - 1, j) && get(i, j + 1) && get(i, j - 1)){
					tiles[i, j] = true;
				}
			}
		}
		
		//Place player
		Room player = rooms[rand.Next(rooms.Count)];
		rooms.Remove(player);
		
		Vector2i pos = getAvailablePos(player);
		
		entities.Add(new Player(pos));
		
		//Place exit
		Room exit = selectRandomRoomFar(player);
		rooms.Remove(exit);
		
		Vector2i pos2 = exit.center;
		
		foreach(Room r in rooms){
			populateRoom(r);
		}
		
		return (tiles, entities, pos2);
	}
	
	(int, int) generateSize(){
		int l = Math.Min(levelNum, 10);
		return (30 + l * 12 + rand.Next(5 * l), 30 + l * 12 + rand.Next(5 * l));
	}
	
	void enableEntities(){
		roomPopulator.Enabled[0] = true; //0, Empty
		roomPopulator.Enabled[1] = true; //1, Goblin
		roomPopulator.Enabled[2] = false; //2, FastGoblin
		roomPopulator.Enabled[3] = true; //3, MedKit
		roomPopulator.Enabled[4] = true; //4, Coin
		roomPopulator.Enabled[5] = false; //5, Orb
		roomPopulator.Enabled[6] = false; //6, Altar
		roomPopulator.Enabled[7] = false; //7, BefriendingAltar
		roomPopulator.Enabled[8] = false; //8, Necromancer
		
		if(levelNum > 0){
			roomPopulator.Enabled[2] = true; //Fast goblin
		}
		
		if(levelNum > 1){
			roomPopulator.Enabled[5] = true; //Orb
		}
		
		if(levelNum > 4){
			roomPopulator.Enabled[6] = true; //Altar
		}
		
		if(levelNum > 6){
			roomPopulator.Enabled[8] = true; //Necromancer
		}
		
		if(levelNum > 7){
			roomPopulator.Enabled[7] = true; //BefriendingAltar
		}
		
		roomPopulator.build();
	}
	
	void populateRoom(Room r){
		switch(roomPopulator.get(rand)){
			case 0: //Empty
			break;
			
			case 1: //Goblin
			int c = 1 + rand.Next(4);
			for(int i = 0; i < c; i++){
				entities.Add(new Goblin(getAvailablePos(r), rand));
			}
			break;
			
			case 2: //FastGoblin
			entities.Add(new FastGoblin(getAvailablePos(r), rand));
			break;
			
			case 3: //Medkit
			c = (levelNum > 5 ? 2 : 1) + rand.Next(3);
			for(int i = 0; i < c; i++){
				entities.Add(new Medkit(getAvailablePos(r), rand));
			}
			break;
			
			case 4: //Coin
			c = 2 + rand.Next(10);
			for(int i = 0; i < c; i++){
				entities.Add(new Coin(getAvailablePos(r)));
			}
			break;
			
			case 5: //Orb
			entities.Add(new Orb(getAvailablePos(r)));
			break;
			
			case 6: //Altar
			entities.Add(new Altar(r.center));
			break;
			
			case 7: //BefriendingAltar
			entities.Add(new BefriendingAltar(r.center));
			break;
			
			case 8: //Necromancer
			entities.Add(new Necromancer(getAvailablePos(r), rand));
			break;
		}
		
		if(rand.Next(2) == 0){
			entities.Add(new DripGenerator(getAvailablePos(r)));
		}
	}
	
	bool entityOccupies(Vector2i p){
		foreach(Entity e in entities){
			if(e.position == p){
				return true;
			}
		}
		return false;
	}
	
	Vector2i getAvailablePos(Room r){
		while(true){
			Vector2i pos = r.getRandomLocation(rand);
			
			if(entityOccupies(pos)){
				continue;
			}
			
			return pos;
		}
	}
	
	void createCorridor(Room r1, Room r2){
		int x1 = r1.CenterX;
		int y1 = r1.CenterY;
		
		int x2 = r2.CenterX;
		int y2 = r2.CenterY;
		
		switch(rand.Next(5)){
			case 0:
			case 1:
			carveHor(x1, x2, y1);
			carveVer(y1, y2, x2);
			break;
			case 2:
			case 3:
			carveVer(y1, y2, x1);
			carveHor(x1, x2, y2);
			break;
			case 4:
			carveDiagonal(x1, y1, x2, y2);
			break;
		}
	}
	
	void carveHor(int x1, int x2, int y) {
		for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
			tiles[y, x] = true;
	}
	
	void carveVer(int y1, int y2, int x) {
		for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
			tiles[y, x] = true;
	}
	
	void carveDiagonal(int x1, int y1, int x2, int y2) {
		int dx = Math.Abs(x2 - x1);
		int dy = Math.Abs(y2 - y1);
		int sx = x1 < x2 ? 1 : -1;
		int sy = y1 < y2 ? 1 : -1;
		int err = dx - dy;
		int e2;
		
		while(true){
			tiles[y1, x1] = true; // mark current tile
			
			if(x1 == x2 && y1 == y2){
				break;
			}
			
			e2 = err * 2;
			
			// mark both tiles when moving diagonally across a corner
			if(e2 > -dy){
				err -= dy;
				x1 += sx;
				tiles[y1, x1] = true; 
			}
			
			if(e2 < dx){
				err += dx;
				y1 += sy;
				tiles[y1, x1] = true; 
			}
		}
	}
	
	Room selectRoomForCorridor(Room current){		
		var possible = rooms
			.Where(r => r != current)
			.Select(r => {
				double distance = current.distance(r);
				return (room: r, distance);
			}).OrderBy(r => r.distance)
			.Select(r => r.room)
			.Where(r => !current.connections.Contains(r))
			.ToArray();
		
		return possible[0];
	}
	
	Room selectRandomRoomNear(Room current){
		const double epsilon = 1e-5;
		
		// Use squared distance for performance, still gives relative proximity
		var weightedRooms = rooms
			.Where(r => r != current)
			.Select(r => {
				double distance = current.distanceSquared(r);
				double weight = 1.0 / (distance + epsilon);
				return (room: r, weight);
			}).ToList();
		
		double totalWeight = weightedRooms.Sum(wr => wr.weight);
		double roll = rand.NextDouble() * totalWeight;
		
		double cumulative = 0.0;
		foreach (var (room, weight) in weightedRooms){
			cumulative += weight;
			if (roll <= cumulative)
				return room;
		}
		
		return weightedRooms[weightedRooms.Count - 1].room; // fallback
	}
	
	Room selectRandomRoomFar(Room current){
		var weightedRooms = rooms
			.Where(r => r != current)
			.Select(r => {
				double distance = current.distance(r);
				double weight = distance;
				return (room: r, weight);
			}).ToList();
		
		double totalWeight = weightedRooms.Sum(wr => wr.weight);
		double roll = rand.NextDouble() * totalWeight;
		
		double cumulative = 0.0;
		foreach (var (room, weight) in weightedRooms){
			cumulative += weight;
			if (roll <= cumulative)
				return room;
		}
		
		return weightedRooms[weightedRooms.Count - 1].room; // fallback
	}	
}

class Room{
	public int X, Y, W, H;
	public int CenterX => X + W / 2;
	public int CenterY => Y + H / 2;
	
	public List<Room> connections = new();
	
	public Vector2i center => new Vector2i(CenterX, CenterY);

	public Room(int x, int y, int w, int h){
		X = x; Y = y; W = w; H = h;
	}

	public bool intersects(Room other) =>
		X < other.X + other.W && X + W > other.X &&
		Y < other.Y + other.H && Y + H > other.Y;
	
	public Vector2i getRandomLocation(Random rand){
		return new Vector2i(X + rand.Next(W), Y + rand.Next(H));
	}
	
	public float distance(Room r){
		return Vector2.Distance(center, r.center);
	}
	
	public float distanceSquared(Room r){
		return Vector2.DistanceSquared(center, r.center);
	}
}

class WeightedRandom{
	public int Count {get; private set;}
	public List<int> Weight {get; private set;} = new();
	public List<bool> Enabled {get; private set;} = new();
	
	int max;
	List<(int, int, int)> ranges = new(); //Rmin, rmax, value
	
	public void Add(int w){
		Count++;
		Weight.Add(w);
		Enabled.Add(true);
	}
	
	public void build(){
		ranges.Clear();
		max = 0;
		
		for(int i = 0; i < Count; i++){
			if(Enabled[i]){
				int n = max;
				
				max += Weight[i];
				
				ranges.Add((n, max - 1, i));
			}
		}
	}
	
	public int get(Random rand){
		int g = rand.Next(max);
		foreach((int n, int x, int v) in ranges){
			if(g >= n && g <= x){
				return v;
			}
		}
		
		return -1;
	}
}