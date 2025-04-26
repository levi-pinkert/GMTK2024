using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public AudioSource oneShotAudioSource;
	public AudioSource musicAudioSource;

	private bool musicMuted = false;

	public static AudioManager Get()
	{
		return GameManager.instance.audioManager;
	}

	public static void PlaySound(AudioClip clip, float volume = 1.0f)
	{
		Get().InstancePlaySound(clip, volume);
	}

	public static void PlayRandomSound(List<AudioClip> options, float volume = 1.0f)
	{
		if (options.Count > 0)
		{
			AudioClip randomClip = options[UnityEngine.Random.Range(0, options.Count)];
			Get().InstancePlaySound(randomClip, volume);
		}
	}

	public void InstancePlaySound(AudioClip clip, float volume = 1.0f)
	{
		oneShotAudioSource.PlayOneShot(clip, volume);
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.M))
		{
			musicMuted = !musicMuted;
			musicAudioSource.mute = musicMuted;
		}
	}
}
