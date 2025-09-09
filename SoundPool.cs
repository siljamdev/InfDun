using System;

class SoundPool{
	Sound[] sounds;
	
	public SoundPool(params Sound[] s){
		sounds = s;
	}
	
	public Sound get(Random rand){
		return sounds[rand.Next(sounds.Length)];
	}
}