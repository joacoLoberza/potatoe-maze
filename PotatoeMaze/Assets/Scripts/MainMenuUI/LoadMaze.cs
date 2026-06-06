using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadMaze : MonoBehaviour
{   
    [SerializeField]
    private Slider loadSlider;


    [SerializeField]
    private void StartGeneration()
    {
        StartCoroutine(GenerateMaze());
    }
    private IEnumerator GenerateMaze()
    {
        AsyncOperation control = SceneManager.LoadSceneAsync("Level Scene", LoadSceneMode.Additive);
        control.allowSceneActivation = false;
        
        while (control.progress < 0.9f)
        {
            loadSlider.value = control.progress/0.9f * 0.6f;

            yield return null;
        }
        Scene levelScene = SceneManager.GetSceneByName("Level Scene");
        GenerateMaze mazeScript = null;
        
        foreach (GameObject obj in levelScene.GetRootGameObjects())
        {
            mazeScript = obj.GetComponent<GenerateMaze>();
            if (mazeScript != null) break;
        }
        StartCoroutine(mazeScript.StartGeneration());

        while (mazeScript.progress < 1.0f)
        {
            loadSlider.value = 0.6f + mazeScript.progress * 0.4f;
            yield return null;
        }

        loadSlider.value = 1.0f;
        control.allowSceneActivation = true;
        yield return null;

        SceneManager.UnloadSceneAsync("Menu Scene");
    }
}

/*
Tengo que:
-Cragar escena asíncrona.
-Modificar el valor del slider.
-Ajustar el valor repartiendolo en  los 4 checkpoints.
*/