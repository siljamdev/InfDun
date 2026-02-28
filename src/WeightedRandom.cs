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