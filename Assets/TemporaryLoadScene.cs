using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TemporaryLoadScene : MonoBehaviour
{

    public void LadScenebyName(string name)
    {
        SceneManager.LoadScene(name);
    }

}
