using System.Collections;
using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Pre-warms every pool behind a loading screen, then raises GameReady.</summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Tooltip("Components implementing IPrewarmable (ChunkGenerator, EnemySpawner, ...)")]
        [SerializeField] MonoBehaviour[] prewarmables;
        [SerializeField] VoidEventChannelSO gameReady;
        [SerializeField] GameObject loadingScreen;

        IEnumerator Start()
        {
            if (loadingScreen != null) loadingScreen.SetActive(true);
            yield return null;

            foreach (var m in prewarmables)
            {
                if (m is IPrewarmable p) p.Prewarm();
                yield return null; // spread the hitch across frames
            }

            if (loadingScreen != null) loadingScreen.SetActive(false);
            gameReady.Raise();
        }
    }
}
