using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioElement
{

    private GameObject element;
    private Dictionary<AudioClip, GameObject> soundObjects = new Dictionary<AudioClip, GameObject>();

    public AudioElement(List<AudioClip> sounds, List<float> volumes, string id, Transform parentTransform)
    {
        if (sounds == null || sounds.Count == 0 || volumes == null || volumes.Count == 0 || sounds.Count != volumes.Count) return;
        element = new GameObject("AudioElement_" + id);
        if (parentTransform) element.transform.parent = parentTransform;
        else
        {
            //attach it to the game object list (since we know there should be one present)
            //do so to keep the inspector cleaner - this saves making a sounds object
            GameObjectList list = MonoBehaviour.FindObjectOfType(typeof(GameObjectList)) as GameObjectList;
            if (list) element.transform.parent = list.transform;
        }
        Add(sounds, volumes);
    }

    public void Add(List<AudioClip> sounds, List<float> volumes)
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            AudioClip sound = sounds[i];
            if (!sound) continue;
            GameObject temp = new GameObject(sound.name);
            temp.AddComponent(typeof(AudioSource));
            temp.GetComponent<AudioSource>().clip = sound;
            temp.GetComponent<AudioSource>().volume = volumes[i];
            temp.transform.parent = element.transform;
            soundObjects.Add(sound, temp);
        }
    }
    public void Play(AudioClip sound)
    {
        GameObject temp;
        if (soundObjects.TryGetValue(sound, out temp))
        {
            if (!temp.GetComponent<AudioSource>().isPlaying) temp.GetComponent<AudioSource>().Play();
        }
    }
    public void Pause(AudioClip sound)
    {
        GameObject temp;
        if (soundObjects.TryGetValue(sound, out temp))
        {
            temp.GetComponent<AudioSource>().Pause();
        }
    }
    public void Stop(AudioClip sound)
    {
        GameObject temp;
        if (soundObjects.TryGetValue(sound, out temp))
        {
            temp.GetComponent<AudioSource>().Stop();
        }
    }

    public bool IsPlaying(AudioClip sound)
    {
        GameObject temp;
        if (soundObjects.TryGetValue(sound, out temp))
        {
            return temp.GetComponent<AudioSource>().isPlaying;
        }
        return false;
    }
}
