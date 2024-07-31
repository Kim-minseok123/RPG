using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    Dictionary<string, AudioClip> _audioBgms = new Dictionary<string, AudioClip>();

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Define.Sound.Bgm].loop = true;
        }
    }

    public void Clear()
    {
        for(int i = 1; i < (int)Define.Sound.MaxCount; i++)
        {
            _audioSources[i].clip = null;
            _audioSources[i].Stop();
        }
        _audioClips.Clear();
    }

    public void Play(string path, Define.Sound type = Define.Sound.Effect, AudioSource objectAudio = null ,float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, objectAudio, pitch);
    }

	public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effect, AudioSource objectAudio = null, float pitch = 1.0f)
	{
        if (audioClip == null)
            return;

		if (type == Define.Sound.Bgm)
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
            if (audioSource.clip != null && audioSource.clip.name.Equals(audioClip.name) == true)
                return;
            Managers.Instance.BgmSoundChange(audioSource, audioClip, pitch, 1);
		}
		else
		{
            AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
            if(objectAudio == null)
            {
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(audioClip);
            }
			else
            {
                objectAudio.pitch = pitch;
                objectAudio.volume = audioSource.volume;
                objectAudio.PlayOneShot(audioClip);
            }
		}
	}

	AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {
		if (path.Contains("Sounds/") == false)
			path = $"Sounds/{path}";

		AudioClip audioClip = null;

		if (type == Define.Sound.Bgm)
		{
            if (_audioBgms.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
                _audioBgms.Add(path, audioClip);
            }
        }
		else
		{
			if (_audioClips.TryGetValue(path, out audioClip) == false)
			{
				audioClip = Managers.Resource.Load<AudioClip>(path);
				_audioClips.Add(path, audioClip);
			}
		}

		if (audioClip == null)
			Debug.Log($"AudioClip Missing ! {path}");

		return audioClip;
    }
    public void SetAudioVolume(Define.Sound type, float volume) 
    {
        AudioSource audioSource = null;
        if (type == Define.Sound.Bgm)
        {
            audioSource = _audioSources[(int)Define.Sound.Bgm];
            Managers.Instance.data.bgmVolume = volume;
        }
        else if(type == Define.Sound.Effect)
        {
            audioSource = _audioSources[(int)Define.Sound.Effect];
            Managers.Instance.data.eftVolume = volume;
        }
        if (audioSource != null) 
            audioSource.volume = volume;
    }
    public void SetBgmLoading(string path, AudioClip audioClip)
    {
        if(_audioBgms.ContainsKey(path) == false)
            _audioBgms.Add(path, audioClip);
    }
}
