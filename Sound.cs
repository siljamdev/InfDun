using System;
using System.IO;
using OpenTK;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using StbVorbisSharp;

class Sound : IDisposable{
	
	//Buffer id
	public int id{get; private set;}
	
	public bool isMono{get; private set;}
	
	private Sound(byte[] d, ALFormat format, int sampleRate){
		this.id = AL.GenBuffer();
		
		unsafe{
			fixed(byte* ptr = d){
				AL.BufferData(this.id, format, ptr, d.Length, sampleRate);
			}
		}
		
		if(format == ALFormat.Mono8 || format == ALFormat.Mono16){
			isMono = true;
		}
	}
	
	//Vorbis ogg
	public static Sound fromBytes(byte[] d){
		using Vorbis vorbis = Vorbis.FromMemory(d);
		List<short> all = new();
		
		do{
			vorbis.SubmitBuffer();
			all.AddRange(vorbis.SongBuffer.Take(vorbis.Decoded * vorbis.Channels));
		}while(vorbis.Decoded != 0);
		
		
		short[] pcmShorts = all.ToArray();
		int samplesDecoded = pcmShorts.Length;
		
		byte[] pcmBytes = new byte[samplesDecoded * 2];
		for(int i = 0; i < samplesDecoded; i++){			
			pcmBytes[i * 2] = (byte)(pcmShorts[i] & 0xFF);            // LSB
			pcmBytes[i * 2 + 1] = (byte)((pcmShorts[i] >> 8) & 0xFF); // MSB
		}
		
		ALFormat format = vorbis.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
		return new Sound(pcmBytes, format, vorbis.SampleRate);
	}
	
	//Vorbis ogg
	public static Sound fromAssembly(string name){
		return fromBytes(AssemblyFiles.getBytes(name));
	}
	
	//Vorbis ogg
	public static Sound fromFile(string path){
		return fromBytes(File.ReadAllBytes(path));
	}
	
	//Vorbis ogg
	public static Sound monoFromBytes(byte[] d){
		using Vorbis vorbis = Vorbis.FromMemory(d);
		List<short> all = new();
		
		do{
			vorbis.SubmitBuffer();
			ReadOnlySpan<short> buffer = vorbis.SongBuffer;
			int decodedFrames = vorbis.Decoded;
			int channels = vorbis.Channels;
			
			for(int i = 0; i < decodedFrames; i++){
				int frameStart = i * channels;
				int sum = 0;
		
				for(int c = 0; c < channels; c++){
					sum += buffer[frameStart + c];
				}
				
				// Average all channels into mono
				short mono = (short)(sum / channels);
				all.Add(mono);
			}
		} while(vorbis.Decoded != 0);
		
		
		short[] pcmShorts = all.ToArray();
		int samplesDecoded = pcmShorts.Length;
		
		byte[] pcmBytes = new byte[samplesDecoded * 2];
		for(int i = 0; i < samplesDecoded; i++){			
			pcmBytes[i * 2] = (byte)(pcmShorts[i] & 0xFF);            // LSB
			pcmBytes[i * 2 + 1] = (byte)((pcmShorts[i] >> 8) & 0xFF); // MSB
		}
		
		return new Sound(pcmBytes, ALFormat.Mono16, vorbis.SampleRate);
	}
	
	//Vorbis ogg
	public static Sound monoFromAssembly(string name){
		return monoFromBytes(AssemblyFiles.getBytes(name));
	}
	
	//Vorbis ogg
	public static Sound monoFromFile(string path){
		return monoFromBytes(File.ReadAllBytes(path));
	}
	
	public void Dispose(){
		AL.DeleteBuffer(this.id);
		this.id = 0;
		GC.SuppressFinalize(this);
	}
	
	~Sound(){
		Dispose();
	}
}