static class Tile{
	public readonly static int[] transitables = new int[]{Floor, Puddle, Exit, Start, Skull, Fungi, SmallPuddle};
	public readonly static int[] withRandomRotation = new int[]{Floor, Puddle, Fungi, SmallPuddle};
	
	public const int Air = 0;
	public const int Floor = 1;
	public const int Puddle = 2;
	
	public const int Exit = 17;
	public const int Start = 18;
	
	public const int Skull = 19;
	public const int Fungi = 20;
	public const int SmallPuddle = 21;
}