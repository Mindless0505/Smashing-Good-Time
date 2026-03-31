using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] songs;
    private AudioSource audioSource;
    private int[] shuffledOrder;
    private int currentIndex = 0;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ShuffleOrder();
    }

    private void Update()
    {
        // When current song finishes, play the next one
        if (!audioSource.isPlaying)
        {
            currentIndex++;
            if (currentIndex >= songs.Length)
            {
                // Reached the end, reshuffle and loop
                currentIndex = 0;
                ShuffleOrder();
            }
            PlayCurrent();
        }
    }

    private void ShuffleOrder()
    {
        
        shuffledOrder = new int[songs.Length];// Fill array with indices
        for (int i = 0; i < songs.Length; i++)
            shuffledOrder[i] = i;


        for (int i = songs.Length - 1; i > 0; i--)// FISHER-YATES! he shuffled this
        {
            int randomIndex = Random.Range(0, i + 1);
            (shuffledOrder[i], shuffledOrder[randomIndex]) = (shuffledOrder[randomIndex], shuffledOrder[i]);
        }
    }

    private void PlayCurrent()
    {
        audioSource.clip = songs[shuffledOrder[currentIndex]];
        audioSource.Play();
    }
}