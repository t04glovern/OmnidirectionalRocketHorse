using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

    public void ChangeScene()
    {
        Application.LoadLevel ("MainGame");
    }

    public void Death()
    {
        Application.LoadLevel("Menu");
    }
}
