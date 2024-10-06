using UnityEngine;

public class TimeSettings : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] private float _scale = 1f;

    private void Update()
    {
        if (Time.timeScale != _scale)
            Time.timeScale = _scale;
    }
}
