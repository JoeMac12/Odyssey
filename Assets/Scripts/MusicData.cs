using UnityEngine;

[CreateAssetMenu(fileName = "MusicData", menuName = "Audio/Music Data")]
public class MusicData : ScriptableObject
{
	[System.Serializable]
	public class MusicTrack
	{
		public string name;
		public AudioClip clip;
		[Range(0f, 1f)]
		public float volume = 1f;
	}

	[Header("Music Tracks")]
	public MusicTrack gameplayMusic;
	public MusicTrack interfaceMusic;

	[Header("Transition Settings")]
	public float crossFadeDuration = 1.5f;
	public float maxVolume = 1f;
}
