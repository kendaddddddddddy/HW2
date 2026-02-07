using UnityEngine;
using UnityEngine.InputSystem;

public class QuitKey : MonoBehaviour
{
    public InputAction quitAction;

    void OnEnable()
    {
        quitAction.Enable();
    }

    void OnDisable()
    {
        quitAction.Disable();
    }

    void Update()
    {
        if (quitAction.triggered)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
