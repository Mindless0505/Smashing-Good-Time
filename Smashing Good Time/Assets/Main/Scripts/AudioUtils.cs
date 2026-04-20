using UnityEngine;

public static class AudioUtils
{
    public static void PlayAtPoint(AudioClip clip, Vector3 position, float minPitch, float maxPitch, bool isOwner = false)
    {
        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = Random.Range(minPitch, maxPitch);
        source.spatialBlend = isOwner ? 0f : 1f;
        source.Play();
        Object.Destroy(tempAudio, clip.length);
    }

    public static void PlaySoundFollowing(AudioClip clip, Transform follow, float minPitch, float maxPitch, bool isOwner = false)
    {
        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.SetParent(follow); // attach to player
        tempAudio.transform.localPosition = Vector3.zero;
        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = Random.Range(minPitch, maxPitch);
        source.spatialBlend = isOwner ? 0f : 1f;
        source.minDistance = 2f;
        source.maxDistance = 30f;
        source.Play();
        Object.Destroy(tempAudio, clip.length);
    }
}
