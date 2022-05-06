using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string target_scene;
    public string current_scene;
    public Vector2 target_pos;

    private void Awake()
    {
        current_scene = gameObject.scene.name;
    }

    public void Transition_Scenes()
    {
        WorldManager.Instance.Transition_Scenes(current_scene, target_scene, target_pos);
    }
}
