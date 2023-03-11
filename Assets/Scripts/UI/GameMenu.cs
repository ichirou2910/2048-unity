using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class GameMenu : MonoBehaviour
    {
        public void Retry()
        {
            SceneManager.LoadScene(0);
        }
    }
}
