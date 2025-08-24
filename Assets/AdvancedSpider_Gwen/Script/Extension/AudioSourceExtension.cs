using UnityEngine;




public static class AudioSourceExtension
{
    public static void PlayClipAtPoint(AudioClip clip, Vector3 pos, float volume = 1, float pitch = 1)
    {
        AudioSource source = new GameObject("PlayClipAtPoint").AddComponent<AudioSource>();
        source.transform.position = pos;
        source.volume = volume;
        source.pitch = pitch;
        source.clip = clip;
        source.Play();
        Object.Destroy(source.gameObject, clip.length / pitch); 
    }
}
