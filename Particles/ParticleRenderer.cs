using System;

class ParticleRenderer{
	public static bool isActive = true;
	
	List<Particle> pars;
	
	public ParticleRenderer(){
		pars = new();
	}
	
	public void add(Particle p){
		if(!isActive){
			return;
		}
		
		pars.Add(p);
	}
	
	public void clear(){
		pars.Clear();
	}
	
	public void draw(Renderer ren){
		if(!isActive){
			return;
		}
		
		List<Particle> del = new();
		foreach(Particle p in pars){
			if(!p.draw(ren)){
				del.Add(p);
			}
		}
		
		pars.RemoveAll(n => del.Contains(n));
	}
}