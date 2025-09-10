using System;
using System.IO;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

class SoundManager : IDisposable{
	
	ALContext context;
	ALDevice device;
	List<int> sources = new List<int>();
	
	const int initialSources = 4;
	
	public bool isActive;
	
	public SoundManager(){
		device = ALC.OpenDevice(null);
		if(device.Handle == 0){
			throw new Exception("Failed to open a OpenAL device. Try changing openal32.dll");
		}
		
		context = ALC.CreateContext(device, new ALContextAttributes());
		if(context.Handle == 0){
			throw new Exception("Failed to create a OpenAL context");
		}
		
		if(!ALC.MakeContextCurrent(context)){
			throw new Exception("Failed to set the OpenAL context");
		}
		
		int[] s = new int[initialSources];
		AL.GenSources(initialSources, s);
		sources = new List<int>(s);
		
		setPos(Vector3.Zero);
		AL.Listener(ALListenerfv.Orientation, new float[]{0f, 0f, -1f, 0f, 1f, 0f});
		//AL.DistanceModel(ALDistanceModel.LinearDistanceClamped);
		AL.DistanceModel(ALDistanceModel.InverseDistanceClamped);
	}
	
	public void setPos(Vector3 p){
		AL.Listener(ALListener3f.Position, ref p);
	}
	
	public void play(Sound sound, Vector3 pos, float gain = 1.0f, float pitch = 1.0f){
		if(pos != Vector3.Zero && !sound.isMono){
			Console.Error.WriteLine("Only Mono sounds support directional audio");
		}
		
		if(!isActive){
			return;
		}
		
		int? s = getFreeSource();
		
		if(s == null){
			return;
		}
		
		int source = (int) s;

		AL.SourceStop(source);
		
		AL.Source(source, ALSourcei.Buffer, sound.id);
		
		AL.Source(source, ALSourcef.Pitch, pitch);
		AL.Source(source, ALSourcef.Gain, gain);
		AL.Source(source, ALSourcef.MaxGain, 1f);
		
		AL.Source(source, ALSourceb.SourceRelative, false);
		AL.Source(source, ALSourcef.RolloffFactor, 1.5f);
		AL.Source(source, ALSourcef.ReferenceDistance, 2f);
		AL.Source(source, ALSourcef.MaxDistance, 32f);
		
		AL.Source(source, ALSource3f.Position, ref pos);
		
		AL.SourcePlay(source);
	}
	
	public void play(Sound sound, float gain = 1.0f, float pitch = 1.0f){
		play(sound, Vector3.Zero, gain, pitch);
	}
	
	//Always max vol
	public void playAbs(Sound sound, float gain = 1.0f, float pitch = 1.0f){
		if(!isActive){
			return;
		}
		
		int? s = getFreeSource();
		
		if(s == null){
			return;
		}
		
		int source = (int) s;

		AL.SourceStop(source);
		
		AL.Source(source, ALSourcei.Buffer, sound.id);
		
		AL.Source(source, ALSourcef.Pitch, pitch);
		AL.Source(source, ALSourcef.Gain, gain);
		
		AL.Source(source, ALSourceb.SourceRelative, true);
		AL.Source(source, ALSource3f.Position, 0f, 0f, 0f);
		
		AL.SourcePlay(source);
	}
	
	int? getFreeSource(){
		for(int i = 0; i < sources.Count; i++){
			ALSourceState state = (ALSourceState) AL.GetSource(sources[i], ALGetSourcei.SourceState);
			if(state != ALSourceState.Playing && state != ALSourceState.Paused){
				return sources[i];
			}
		}
		
		//Soft limit
		if(sources.Count > 255){
			return null;
		}
		
		// Expand source pool
		int s = AL.GenSource();
		sources.Add(s);
		return s;
	}
	
	#region errors
	public void checkErrors(){
		ALError error = AL.GetError();
		while(error != ALError.NoError){
			Console.Error.WriteLine("[OpenAL Error] " + error);
			
			error = AL.GetError();
		}
		
		AlcError error2 = ALC.GetError(device);
		while(error2 != AlcError.NoError){
			Console.Error.WriteLine("[OpenAL Context Error] " + error);
			
			error2 = ALC.GetError(device);
		}
	}
	#endregion
	
	public void Dispose(){
		foreach(int s in sources){
			AL.SourceStop(s);
			AL.DeleteSource(s);
		}
		sources.Clear();
		
		//ALC.MakeContextCurrent((ALContext) null);
		ALC.DestroyContext(context);
		
		ALC.CloseDevice(device);
		GC.SuppressFinalize(this);
	}
	
	~SoundManager(){
		Dispose();
	}
}