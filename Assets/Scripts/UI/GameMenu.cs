using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class GameMenu : MonoBehaviour
    {
        const int TITLE_SCENE_IDX = 0;
        const int GAME_SCENE_IDX = 1;
        
        public void Retry()
        {
            SceneManager.LoadScene(GAME_SCENE_IDX);
        }

        public void ReturnToTitle()
        {
            SceneManager.LoadScene(TITLE_SCENE_IDX);
        }
    }
}
