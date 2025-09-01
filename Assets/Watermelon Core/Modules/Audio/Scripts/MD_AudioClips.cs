using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Audio Clips", menuName = "Data/Core/Audio Clips")]
    public class AudioClips : ScriptableObject
    {
        [BoxGroup("UI", "UI")]
        public AudioClip buttonSound;

        [BoxGroup("Gameplay", "Gameplay")]
        public AudioClip objectHit;
        [BoxGroup("Gameplay")]
        public AudioClip shot;
        [BoxGroup("Gameplay")]
        public AudioClip obstcleDestroyed;
        [BoxGroup("Gameplay")]
        public AudioClip buff;
        [BoxGroup("Gameplay")]
        public AudioClip debuff;
        [BoxGroup("Gameplay")]
        public AudioClip win;
        [BoxGroup("Gameplay")]
        public AudioClip lose;
        [BoxGroup("Gameplay")]
        public AudioClip heal;
        [BoxGroup("Gameplay")]
        public AudioClip reward;
        [BoxGroup("Gameplay")]
        public List<AudioClip> screams;

        [BoxGroup("Clip Handlers", "Clip Handlers")]
        public AudioClipHandler shotClipHandler;

    }
}

// -----------------
// Audio Controller v 0.4
// -----------------