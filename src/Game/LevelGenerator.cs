using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class LevelGenerator{
	
	readonly WeightedRandom roomPopulator;
	
	Random rand;
	Random casual;
	
	List<Room> rooms;
	List<Room> allRooms = new();
	List<Entity> entities = new();
	
	int width;
	int height;
	GridElement[,] grid;
	
	int levelNum;
	
	bool hasNecromancer;
	
	Room lockedMain;
	bool hasLockedRooms => lockedMain != null;
	
	int lockedRoomsContent = 0;
	
	public LevelGenerator(int l, Random serious, Random cas){
		levelNum = l;
		rand = serious;
		casual = cas;
		
		roomPopulator = new WeightedRandom();
		roomPopulator.Add(30); //0, Empty
		roomPopulator.Add(18); //1, Goblin
		roomPopulator.Add(10); //2, FastGoblin
		roomPopulator.Add(8); //3, MedKit
		roomPopulator.Add(20); //4, Coin
		roomPopulator.Add(5); //5, Orb
		roomPopulator.Add(1); //6, Altar
	}
	
	public (int[,], List<Entity>, Vector2i, Vector2i) generate(){
		//Size
		int l = Math.Min(levelNum, 10);
		width = 30 + l * 12 + rand.Next(5 * l);
		height = 30 + l * 12 + rand.Next(5 * l);
		
		grid = new GridElement[height, width];
		
		hasNecromancer = levelNum > 6 && rand.Next(2) == 0;
		
		//Room count
		int min = 4 + (width * height) / 700;
		int roomCount = rand.Next(min, min + (width * height) / 500);
		rooms = new List<Room>(roomCount);
		
		//Room size
		int maxRoomSize = Math.Min(6 + 2 * levelNum, 10);
		
		int wetRoomProbability = 1 + rand.Next(4);
		
		bool trylocked = levelNum > 9 || (levelNum > 3 && rand.Next(2) == 0);
		//trylocked = true;
		
		if(trylocked){
			lockedMain = generateLockedRooms(wetRoomProbability);
			
			if(hasLockedRooms){
				if(rand.Next(3) == 0){
					lockedRoomsContent = 0;
				}else if(levelNum < 6){
					lockedRoomsContent = rand.Next(5); //No BefriendingAltar
				}else{
					lockedRoomsContent = rand.Next(6);
				}
			}
		}
		
		//Rooms
		for(int i = 0; i < roomCount; i++){
			int w = rand.Next(4, maxRoomSize);
			int h = rand.Next(4, maxRoomSize);
			int x = rand.Next(1, width - w - 1);
			int y = rand.Next(1, height - h - 1);
			bool wet = rand.Next(wetRoomProbability) != 0;
			
			Room newRoom = new Room(x, y, w, h, wet);
			if(newRoom.touchesOtherLocked(grid) || newRoom.intersectNumber(grid) > 6){
				i--;
				continue;
			}
			
			//carve
			newRoom.carve(grid);
			
			//connect
			if(rooms.Count > 0){
				List<Room> pool = rooms.ToList();
				int numberOfConnections = Math.Min(rooms.Count, 1 + rand.Next(2));
				for(int k = 0; k < numberOfConnections; k++){
					Room target = selectNearestRoom(newRoom, pool);
					if(target == null){
						break;
					}
					pool.Remove(target);
					
					Corridor c = new Corridor(newRoom, target);
					if(c.tryCarve(grid, rand)){
						newRoom.connections.Add(c);
					}else{
						k--; //Retry
					}
				}
			}
			rooms.Add(newRoom);
		}
		
		//Ensure no unconnected rooms
		foreach(Room r in rooms){
			if(r.connections.Count == 0){
				foreach(Room t in rooms.Where(h => h != r).OrderBy(h => r.distanceSquared(h))){
					Corridor c = new Corridor(r, t);
					if(c.tryCarve(grid, rand)){
						r.connections.Add(c);
						//Console.WriteLine("Connected orphan room");
						break;
					}
				}
			}
		}
		
		//Helper
		bool isFloor(int y, int x){
			if(y >= 0 && y < height && x >= 0 && x < width){
				return grid[y, x] != null;
			}
			return false;
		}
		
		//Ensure there is no weird single wall tile
		for(int i = 0; i < height; i++){
			for(int j = 0; j < width; j++){
				if(!isFloor(i, j) && isFloor(i + 1, j) && isFloor(i - 1, j) && isFloor(i, j + 1) && isFloor(i, j - 1)){
					Door d = grid[i, j - 1].lockedBehind;
					if(d == grid[i - 1, j].lockedBehind && d == grid[i + 1, j].lockedBehind && d == grid[i, j + 1].lockedBehind){
						grid[i, j] = grid[i, j - 1];
					}
				}
			}
		}
		
		allRooms.AddRange(rooms);
		
		//Place player
		Room player = rooms[rand.Next(rooms.Count)];
		rooms.Remove(player);
		
		Vector2i startPos = getAvailablePos(player);
		
		//Place exit + necromancer
		Room exitRoom;
		if(hasLockedRooms && lockedRoomsContent == 0){
			exitRoom = lockedMain;
		}else{
			exitRoom = selectRandomRoomFar(player);
		}
		rooms.Remove(exitRoom);
		
		Vector2i exitPos = exitRoom.center;
		
		if(hasNecromancer){ //Add necromancer
			List<Entity> ents = exitRoom.isLocked ? exitRoom.lockedBehind.entitiesToAdd : entities;
			ents.Add(new Necromancer(getAvailablePos(exitRoom), casual));
			
			//Second necromancer
			if(levelNum > 10 && rand.Next(4) == 0){
				Room secondNecroRoom = rooms[rand.Next(rooms.Count)];
				rooms.Remove(secondNecroRoom);
				
				entities.Add(new Necromancer(getAvailablePos(secondNecroRoom), casual));
			}
		}
		
		//Place key
		if(hasLockedRooms){
			Room keyRoom = selectRandomRoomFar(player);
			rooms.Remove(keyRoom);
			
			entities.Add(new Key(keyRoom.center));
		}
		
		//Place one treasure room
		Room treasure = rooms[rand.Next(rooms.Count)];
		rooms.Remove(treasure);
		
		populateTreasure(treasure);
		
		//Populate
		enableEntities();
		foreach(Room r in rooms){
			populateNormal(r);
		}
		
		//Populate locked rooms
		foreach(Room r in allRooms.Where(h => h.isLocked && !rooms.Contains(h))){
			populateLocked(r);
		}
		
		//Decorate
		foreach(Room r in allRooms){
			decorate(r);
		}
		
		return (generateTiles(startPos, exitPos), entities, startPos, exitPos);
	}
	
	Room generateLockedRooms(int wetRoomProbability){
		int w = rand.Next(6, 10);
		int h = rand.Next(6, 10);
		int x = rand.Next(1, width - w - 1);
		int y = rand.Next(10, height - h - 1);
		bool wet = rand.Next(wetRoomProbability) != 0;
		
		Room locked = new Room(x, y, w, h, wet);
		Door d = new Door(new Vector2i(locked.centerX, locked.centerY - h / 2 - 1));
		
		locked.lockedBehind = d;
		locked.carve(grid);
		
		int tries = 0;
		
		while(tries < 10){
			int w2 = rand.Next(4, 10);
			int h2 = rand.Next(4, 10);
			(int x2, int y2) = locked.getRandomNearRoomPos(rand, width, height, w2, h2);
			bool wet2 = rand.Next(wetRoomProbability) != 0;
			
			Room connect = new Room(x2, y2, w2, h2, wet2);
			
			if(connect.touchesOtherLocked(grid)){ //Avoid connect touching locked
				tries++;
				continue;
			}
			
			Corridor c = new LockedCorridor(locked, connect, d);
			if(c.tryCarve(grid, rand)){
				c.lockedBehind = null;
				
				locked.connections.Add(c);
				entities.Add(d);
				allRooms.Add(locked);
				
				connect.carve(grid);
				rooms.Add(connect);
				
				bool hasSecond = rand.Next(2) == 0;
				bool hasThird = hasSecond && rand.Next(3) == 0;
				
				if(hasSecond){
					Room second = generateSecondaryLocked(locked, wetRoomProbability);
					
					if(hasThird){
						generateSecondaryLocked(rand.Next(2) == 0 ? locked : second, wetRoomProbability);
					}
				}
				
				return locked;
			}else{
				tries++;
			}
		}
		
		locked.lockedBehind = null;
		rooms.Add(locked);
		return null;
	}
	
	Room generateSecondaryLocked(Room locked, int wetRoomProbability){
		int tries = 0;
		
		while(tries < 10){
			int w3 = rand.Next(6, 10);
			int h3 = rand.Next(6, 10);
			(int x3, int y3) = locked.getRandomNearRoomPos(rand, width, height, w3, h3);
			bool wet3 = rand.Next(wetRoomProbability) != 0;
			
			Room locked2 = new Room(x3, y3, w3, h3, wet3);
			locked2.lockedBehind = locked.lockedBehind;
			
			if(locked2.touchesOtherLocked(grid)){
				tries++;
				continue;
			}
			
			Corridor c = new Corridor(locked, locked2);
			c.lockedBehind = locked.lockedBehind;
			if(c.tryCarve(grid, rand)){
				locked.connections.Add(c);
				
				locked2.carve(grid);
				allRooms.Add(locked2);
				
				return locked2;
			}else{
				tries++;
			}
		}
		
		return null;
	}
	
	void enableEntities(){
		roomPopulator.Enabled[0] = true; //0, Empty
		roomPopulator.Enabled[1] = true; //1, Goblin
		roomPopulator.Enabled[2] = false; //2, FastGoblin
		roomPopulator.Enabled[3] = true; //3, MedKit
		roomPopulator.Enabled[4] = true; //4, Coin
		roomPopulator.Enabled[5] = false; //5, Orb
		roomPopulator.Enabled[6] = false; //6, Altar
		
		if(levelNum > 0){
			roomPopulator.Enabled[2] = true; //Fast goblin
		}
		
		if(levelNum > 3){
			roomPopulator.Enabled[5] = true; //Orb
		}
		
		if(levelNum > 8){
			roomPopulator.Enabled[6] = true; //Altar
		}
		
		roomPopulator.build();
	}
	
	void populateNormal(Room r){
		List<Entity> ents = r.isLocked ? r.lockedBehind.entitiesToAdd : entities;
		
		switch(roomPopulator.get(rand)){
			case 0: //Empty + Frogs
			if(r.isWet && rand.Next(12) == 0){
				int c2 = 1 + rand.Next(3);
				for(int i = 0; i < c2; i++){
					ents.Add(new Frog(getAvailablePos(r), casual));
				}
			}
			break;
			
			case 1: //Goblin + Coins
			int c = 1 + rand.Next(3);
			for(int i = 0; i < c; i++){
				ents.Add(new Goblin(getAvailablePos(r), casual));
			}
			
			c = -1 + rand.Next(5);
			for(int i = 0; i < c; i++){
				ents.Add(new Coin(getAvailablePos(r)));
			}
			break;
			
			case 2: //FastGoblin
			ents.Add(new FastGoblin(getAvailablePos(r), casual));
			break;
			
			case 3: //Medkit
			c = (levelNum > 6 ? 2 : 1) + rand.Next(2);
			for(int i = 0; i < c; i++){
				ents.Add(new Medkit(getAvailablePos(r), rand));
			}
			break;
			
			case 4: //Coin
			c = 2 + rand.Next(12);
			for(int i = 0; i < c; i++){
				ents.Add(new Coin(getAvailablePos(r)));
			}
			break;
			
			case 5: //Orb
			ents.Add(new Orb(getAvailablePos(r)));
			break;
			
			case 6: //Altar
			ents.Add(new Altar(r.center));
			break;
		}
	}
	
	void populateTreasure(Room r){
		List<Entity> ents = r.isLocked ? r.lockedBehind.entitiesToAdd : entities;
		
		int c = 10 + rand.Next(6);
		for(int i = 0; i < c; i++){
			ents.Add(new Coin(getAvailablePos(r)));
		}
	}
	
	void populateLocked(Room r){
		List<Entity> ents = r.isLocked ? r.lockedBehind.entitiesToAdd : entities;
		
		if(r == lockedMain){
			switch(lockedRoomsContent){
				case 1: //Medkits
				int c = 4 + rand.Next(3);
				for(int i = 0; i < c; i++){
					ents.Add(new Medkit(getAvailablePos(r), rand));
				}
				break;
				
				case 2: //Coins
				c = 10 + rand.Next(6);
				for(int i = 0; i < c; i++){
					ents.Add(new Coin(getAvailablePos(r)));
				}
				break;
				
				case 3: //Orbs
				c = 1 + rand.Next(3);
				for(int i = 0; i < c; i++){
					ents.Add(new Orb(getAvailablePos(r)));
				}
				break;
				
				case 4: //Altar
				ents.Add(new Altar(r.center));
				break;
				
				case 5: //BefriendingAltar
				ents.Add(new BefriendingAltar(r.center));
				break;
			}
		}else{
			switch(rand.Next(2)){
				case 0: //Medkits
				int c = 3 + rand.Next(3);
				for(int i = 0; i < c; i++){
					ents.Add(new Medkit(getAvailablePos(r), rand));
				}
				break;
				
				case 1: //Coins
				c = 7 + rand.Next(4);
				for(int i = 0; i < c; i++){
					ents.Add(new Coin(getAvailablePos(r)));
				}
				break;
			}
		}
	}
	
	void decorate(Room r){
		List<Entity> ents = r.isLocked ? r.lockedBehind.entitiesToAdd : entities;
		
		if(r.isWet){ //Drip
			int c2 = casual.Next(3) == 0 ? 2 : 1;
			for(int i = 0; i < c2; i++){
				ents.Add(new DripGenerator(r.getRandomLocation(casual), casual));
			}
		}
		
		if(r.isLocked){ //smoke
			Entity[] smks = r.getAllTiles().Select(p => new SmokeGenerator(p, casual)).ToArray();
			r.lockedBehind.entitiesToRemove.AddRange(smks);
			entities.AddRange(smks);
		}
		
		r.connections.ForEach(c => decorateCorridor(c));
	}
	
	void decorateCorridor(Corridor c){
		if(c is LockedCorridor){
			return;
		}
		
		List<Entity> ents = c.isLocked ? c.lockedBehind.entitiesToAdd : entities;
		
		if(c.isLocked){ //smoke			
			for(int y = Math.Min(c.y1, c.y2); y <= Math.Max(c.y1, c.y2); y++){
				for(int x = Math.Min(c.x1, c.x2); x <= Math.Max(c.x1, c.x2); x++){
					if(y >= 0 && y < height && x >= 0 && x < width){
						if(grid[y, x] == c){
							Entity smk = new SmokeGenerator(new Vector2i(x, y), casual);
							c.lockedBehind.entitiesToRemove.Add(smk);
							entities.Add(smk);
						}
					}
				}
			}
		}
	}
	
	bool entityOccupies(Room r, Vector2i p){
		List<Entity> ents = r.isLocked ? r.lockedBehind.entitiesToAdd : entities;
		
		foreach(Entity e in ents){
			if(e.position == p){
				return true;
			}
		}
		return false;
	}
	
	Vector2i getAvailablePos(Room r){
		while(true){
			Vector2i pos = r.getRandomLocation(rand);
			
			if(entityOccupies(r, pos)){
				continue;
			}
			
			return pos;
		}
	}
	
	//Closest unconnected
	Room selectNearestRoom(Room current, List<Room> pool){
		return pool
			.Where(r => r != current)
			.Where(r => !current.connectedRooms.Contains(r))
			.OrderBy(r => current.distanceSquared(r))
			.FirstOrDefault();
	}
	
	Room selectRandomRoomFar(Room origin){
		var weightedRooms = rooms
			.Where(r => r != origin)
			.Select(r => (room: r, weight: origin.distance(r)))
			.ToList();
		
		double totalWeight = weightedRooms.Sum(wr => wr.weight);
		double roll = rand.NextDouble() * totalWeight;
		
		double cumulative = 0.0;
		foreach((Room room, double weight) in weightedRooms){
			cumulative += weight;
			if(roll <= cumulative)
				return room;
		}
		
		return weightedRooms[weightedRooms.Count - 1].room; //fallback
	}
	
	//A bit messy, but it works
	int[,] generateTiles(Vector2i startPos, Vector2i exitPos){
		int[,] w = new int[height, width];
		
		//Helper func
		bool isFloor(int x, int y){
			if(x >= 0 && x < width && y >= 0 && y < height){
				return grid[y, x] != null;
			}
			return false;
		}
		
		int getFloorTile(GridElement r){
			if(casual.Next(8) == 0){
				if(hasNecromancer && casual.Next(100) == 0){
					return Tile.Skull;
				}else if(casual.Next(4) == 0){
					return Tile.Fungi;
				}else if(r.isLocked && casual.Next(6) == 0){
					return Tile.Pots;
				}else if(r.isWet){
					return casual.Next(3) == 0 ? Tile.SmallPuddle : Tile.Puddle;
				}
			}
			return Tile.Floor;
		}
		
		int getWallTile(GridElement r){
			if(casual.Next(24) == 0){
				return Tile.TorchWall;
			}else if(r.isWet && casual.Next(12) == 0){
				return Tile.MossyWall;
			}
			return Tile.Wall;
		}
		
		for(int i = 0; i < width; i++){
			for(int j = 0; j < height; j++){
				if(isFloor(i, j)){
					if(new Vector2i(i, j) == exitPos){
						w[j, i] = Tile.Exit;
					}else if(new Vector2i(i, j) == startPos){
						w[j, i] = Tile.Start;
					}else{
						w[j, i] = getFloorTile(grid[j, i]);
					}
				}else if(isFloor(i, j-1)){
					w[j, i] = getWallTile(grid[j-1, i]);
				}else if(isFloor(i+1, j-1) && isFloor(i-1, j-1)){
					if(isFloor(i, j+1)){
						w[j, i] = 11;
					}else{
						w[j, i] = 12;
					}
				}else if(isFloor(i+1, j-1)){
					if(isFloor(i, j+1)){
						if(isFloor(i-1, j)){
							w[j, i] = 11;
						}else{
							w[j, i] = 9;
						}
					}else if(isFloor(i-1, j)){
						w[j, i] = 12;
					}else if(isFloor(i-1, j+1)){
						w[j, i] = 14;
					}else{
						w[j, i] = 5;
					}
				}else if(isFloor(i-1, j-1)){
					if(isFloor(i, j+1)){
						if(isFloor(i+1, j)){
							w[j, i] = 11;
						}else{
							w[j, i] = 10;
						}
					}else if(isFloor(i+1, j)){
						w[j, i] = 12;
					}else if(isFloor(i+1, j+1)){
						w[j, i] = 15;
					}else{
						w[j, i] = 6;
					}
				}else if(isFloor(i, j+1)){
					if(isFloor(i+1, j) && isFloor(i-1, j)){
						w[j, i] = 11;
					}else if(isFloor(i+1, j)){
						w[j, i] = 9;
					}else if(isFloor(i-1, j)){
						w[j, i] = 10;
					}else{
						w[j, i] = 4;
					}
				}else if(isFloor(i+1, j) && isFloor(i-1, j)){
					w[j, i] = 12;
				}else if(isFloor(i+1, j)){
					if(isFloor(i-1, j+1)){
						w[j, i] = 14;
					}else{
						w[j, i] = 5;
					}
				}else if(isFloor(i-1, j)){
					if(isFloor(i+1, j+1)){
						w[j, i] = 15;
					}else{
						w[j, i] = 6;
					}
				}else if(isFloor(i+1, j+1) && isFloor(i-1, j+1)){
					w[j, i] = 13;
				}else if(isFloor(i+1, j+1)){
					w[j, i] = 7;
				}else if(isFloor(i-1, j+1)){
					w[j, i] = 8;
				}
			}
		}
		
		return w;
	}
}

class GridElement{
	public Door lockedBehind;
	public bool isLocked => lockedBehind != null;
	
	public bool isWet;
	
	protected GridElement get(GridElement[,] grid, int y, int x){
		if(y >= 0 && y < grid.GetLength(0) && x >= 0 && x < grid.GetLength(1)){
			return grid[y, x];
		}
		return null;
	}
}

class Room : GridElement{
	public int x, y, w, h;
	public int centerX => x + w / 2;
	public int centerY => y + h / 2;
	
	public IEnumerable<Room> connectedRooms => connections.Select(c => c.other);
	public List<Corridor> connections = new();
	
	public Vector2i center => new Vector2i(centerX, centerY);

	public Room(int x, int y, int w, int h, bool wet){
		this.x = x;
		this.y = y;
		this.w = w;
		this.h = h;
		
		isWet = wet;
	}

	public bool intersects(Room other) =>
		this.x < other.x + other.w && x + w > other.x &&
		this.y < other.y + other.h && y + h > other.y;
	
	public int intersectNumber(GridElement[,] grid){
		int c = 0;
		
		for(int x2 = x; x2 < x + w; x2++){
			for(int y2 = y; y2 < y + h; y2++){
				if(grid[y2, x2] != null){
					c++;
				}
			}
		}
		
		return c;
	}
	
	public bool touchesOtherLocked(GridElement[,] grid){
		for(int x2 = x - 1; x2 < x + w + 1; x2++){
			for(int y2 = y - 1; y2 < y + h + 1; y2++){
				GridElement e = get(grid, y2, x2);
				if(e != null && e.lockedBehind != lockedBehind){
					return true;
				}
			}
		}
		
		return false;
	}
	
	public void carve(GridElement[,] grid){
		for(int x2 = x; x2 < x + w; x2++){
			for(int y2 = y; y2 < y + h; y2++){
				grid[y2, x2] = this;
			}
		}
	}
	
	public (int x, int y) getRandomNearRoomPos(Random rand, int width, int height, int w2, int h2){
		const int maxDist = 20;
		return (rand.Next(Math.Max(1, x - maxDist), Math.Min(width - w2 - 1, x + w + maxDist)), rand.Next(Math.Max(1, y - maxDist), Math.Min(height - h2 - 1, y + h + maxDist)));
	}
	
	public Vector2i getRandomLocation(Random rand){
		return new Vector2i(x + rand.Next(w), y + rand.Next(h));
	}
	
	public float distance(Room r){
		return Vector2.Distance(center, r.center);
	}
	
	public float distanceSquared(Room r){
		return Vector2.DistanceSquared(center, r.center);
	}
	
	public Vector2i[] getAllTiles(){
		List<Vector2i> all = new(w * h);
		for(int x2 = x; x2 < x + w; x2++){
			for(int y2 = y; y2 < y + h; y2++){
				all.Add(new Vector2i(x2, y2));
			}
		}
		return all.ToArray();
	}
}

class Corridor : GridElement{
	public int x1, x2, y1, y2;
	public Room room;
	public Room other;
	
	public Corridor(int x1, int y1, int x2, int y2, Room r, Room o){
		this.x1 = x1;
		this.y1 = y1;
		this.x2 = x2;
		this.y2 = y2;
		
		room = r;
		other = o;
		
		isWet = room.isWet;
	}
	
	public Corridor(Room m, Room s) : this(m.centerX, m.centerY, s.centerX, s.centerY, m, s){
		
	}
	
	public virtual bool tryCarve(GridElement[,] grid, Random rand){
		List<int> carvers = new(){1,1,2,2,3};
		
		while(carvers.Count > 0){
			int sindex = rand.Next(carvers.Count);
			int selected = carvers[sindex];
			carvers.RemoveAt(sindex);
			
			switch(selected){
				case 1:
					if(!horTouchesOtherLocked(grid, x1, x2, y1) && !verTouchesOtherLocked(grid, y1, y2, x2)){
						carveHor(grid, x1, x2, y1);
						carveVer(grid, y1, y2, x2);
						return true;
					}
					break;
				
				case 2:
					if(!verTouchesOtherLocked(grid, y1, y2, x1) && !horTouchesOtherLocked(grid, x1, x2, y2)){
						carveVer(grid, y1, y2, x1);
						carveHor(grid, x1, x2, y2);
						return true;
					}
					break;
				
				case 3:
					if(!diagonalTouchesOtherLocked(grid, x1, y1, x2, y2)){
						carveDiagonal(grid, x1, y1, x2, y2);
						return true;
					}
					break;
			}
		}
		
		return false;
	}
	
	protected bool horTouchesOtherLocked(GridElement[,] grid, int x1, int x2, int y){
		for(int y2 = y - 1; y2 < y + 2; y2++){
			for(int x = Math.Min(x1, x2) - 1; x <= Math.Max(x1, x2) + 1; x++){
				GridElement e = get(grid, y2, x);
				if(e != null && e.lockedBehind != lockedBehind){
					return true;
				}
			}
		}
		
		return false;
	}
	
	protected void carveHor(GridElement[,] grid, int x1, int x2, int y){
		for(int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
			grid[y, x] ??= this;
	}
	
	protected bool verTouchesOtherLocked(GridElement[,] grid, int y1, int y2, int x){
		for(int x2 = x - 1; x2 < x + 2; x2++){
			for(int y = Math.Min(y1, y2) - 1; y <= Math.Max(y1, y2) + 1; y++){
				GridElement e = get(grid, y, x2);
				if(e != null && e.lockedBehind != lockedBehind){
					return true;
				}
			}
		}
		
		return false;
	}
	
	protected void carveVer(GridElement[,] grid, int y1, int y2, int x){
		for(int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
			grid[y, x] ??= this;
	}
	
	bool touches(GridElement[,] grid, int y, int x){
		GridElement e = get(grid, y, x);
		if(e != null && e.lockedBehind != lockedBehind){
			return true;
		}
		
		e = get(grid, y, x - 1);
		if(e != null && e.lockedBehind != lockedBehind){
			return true;
		}
		
		e = get(grid, y, x + 1);
		if(e != null && e.lockedBehind != lockedBehind){
			return true;
		}
		
		e = get(grid, y - 1, x);
		if(e != null && e.lockedBehind != lockedBehind){
			return true;
		}
		
		e = get(grid, y + 1, x);
		if(e != null && e.lockedBehind != lockedBehind){
			return true;
		}
		
		return false;
	}
	
	protected bool diagonalTouchesOtherLocked(GridElement[,] grid, int x1, int y1, int x2, int y2){
		int dx = Math.Abs(x2 - x1);
		int dy = Math.Abs(y2 - y1);
		int sx = x1 < x2 ? 1 : -1;
		int sy = y1 < y2 ? 1 : -1;
		int err = dx - dy;
		int e2;
		
		while(true){
			if(touches(grid, y1, x1)){
				return true;
			}
			
			if(x1 == x2 && y1 == y2){
				return false;
			}
			
			e2 = err * 2;
			
			//mark both tiles when moving diagonally across a corner
			if(e2 > -dy){
				err -= dy;
				x1 += sx;
				if(touches(grid, y1, x1)){
					return true;
				}
			}
			
			if(e2 < dx){
				err += dx;
				y1 += sy;
				if(touches(grid, y1, x1)){
					return true;
				}
			}
		}
	}
	
	protected void carveDiagonal(GridElement[,] grid, int x1, int y1, int x2, int y2){
		int dx = Math.Abs(x2 - x1);
		int dy = Math.Abs(y2 - y1);
		int sx = x1 < x2 ? 1 : -1;
		int sy = y1 < y2 ? 1 : -1;
		int err = dx - dy;
		int e2;
		
		while(true){
			grid[y1, x1] ??= this; //mark current tile
			
			if(x1 == x2 && y1 == y2){
				break;
			}
			
			e2 = err * 2;
			
			//mark both tiles when moving diagonally across a corner
			if(e2 > -dy){
				err -= dy;
				x1 += sx;
				grid[y1, x1] ??= this; 
			}
			
			if(e2 < dx){
				err += dx;
				y1 += sy;
				grid[y1, x1] ??= this; 
			}
		}
	}
}

class LockedCorridor : Corridor{
	public LockedCorridor(Room m, Room s, Door d) : base(m, s){
		lockedBehind = d;
	}
	
	public override bool tryCarve(GridElement[,] grid, Random rand){
		Door d = lockedBehind;
		
		bool b1 = !verTouchesOtherLocked(grid, y1, y1 - 10, x1);
		lockedBehind = null;
		bool b2 = !horTouchesOtherLocked(grid, x1, x2, y1 - 10) && !verTouchesOtherLocked(grid, y1 - 10, y2, x2);
		lockedBehind = d;
		if(b1 && b2){
			carveVer(grid, y1, y1 - 10, x1);
			carveHor(grid, x1, x2, y1 - 10);
			carveVer(grid, y1 - 10, y2, x2);
			return true;
		}
		
		return false;
	}
}