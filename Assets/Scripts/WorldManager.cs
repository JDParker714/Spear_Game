using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldManager : MonoBehaviour
{
    private static WorldManager _instance;
    public static WorldManager Instance { get { return _instance; } }

    public GameObject player_prefab;
    public GameObject player;
    public CameraController player_cam;

    public GameObject scene_transition_effect;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        //handle player
        Init_Player();
        Move_Player(Vector2.zero);
        Move_Camera_To_Player();
        Load_Scene("DemoScene");
    }

    public void Init_Player()
    {
        //handle player
        PlayerController[] existing_players = FindObjectsOfType<PlayerController>();
        if (existing_players.Length > 0)
        {
            for (int i = 0; i < existing_players.Length; i++)
            {
                if (i == 0) player = existing_players[i].gameObject;
                else Destroy(existing_players[i].gameObject);
            }
        }
        else
        {
            player = Instantiate(player_prefab);
        }
    }

    public void Transition_Scenes(string from, string to, Vector2 target_pos)
    {
        Debug.Log("Loading: " + to + " Unloading: " + from);
        if (Application.CanStreamedLevelBeLoaded(to))
        {
            Debug.Log("Can Load " + to + " Scene");
            StartCoroutine(Scene_Transition_Effect(from, to, target_pos));
        }
        else
        {
            Debug.Log("Cant Load " + to + " Scene");
        }
    }

    public void Load_Scene(string scene)
    {
        if (Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.Log("Can Load " + scene + " Scene");
            StartCoroutine(Load_Scene_Async(scene));
        }
        else
        {
            Debug.Log("Cant Load " + scene + " Scene");
        }
    }

    public IEnumerator Load_Scene_Async(string to)
    {
        //load scene
        bool already_loaded = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name == to)
            {
                already_loaded = true;
                break;
            }
        }
        if (!already_loaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(to));
    }

    public IEnumerator Scene_Transition_Effect(string from, string to, Vector2 target_pos)
    {
        GameObject effect = Instantiate(scene_transition_effect, player.transform);
        effect.transform.SetParent(null);
        float timeScale = Time.timeScale;
        //Time.timeScale = 0f;
        player.GetComponent<PlayerController>().Set_Can_Move(false);

        yield return new WaitForSecondsRealtime(1.5f);

        //unload scene if it exists
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name == from)
            {
                SceneManager.UnloadSceneAsync(from);
                break;
            }
        }
        //load scene
        bool already_loaded = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name == to)
            {
                already_loaded = true;
                break;
            }
        }
        if (!already_loaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(to));

        Move_Player(target_pos);
        Move_Camera_To_Player();

        //Time.timeScale = timeScale;

        yield return new WaitForSecondsRealtime(1.5f);

        Destroy(effect);
        player.GetComponent<PlayerController>().Set_Can_Move(true);
    }

    public void Move_Player(Vector2 target_pos)
    {
        if (player == null) Init_Player();
        player.transform.position = target_pos;
    }

    public void Move_Camera_To_Player()
    {
        if (player_cam != null) player_cam.Move_To_Position(player.transform.position);
    }

    public Transform Get_Player_T()
    {
        if (player == null) Init_Player();
        return player.transform;
    }
}
