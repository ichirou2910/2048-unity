using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class TitleMenu : MonoBehaviour
    {
        const int GAME_SCENE_IDX = 1;

        public void Play()
        {
            SceneManager.LoadScene(GAME_SCENE_IDX);
        }

        public void Quit()
        {
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}