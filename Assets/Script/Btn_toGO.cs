using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Btn_toGO : MonoBehaviour
{


    public Button buttonToClick;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {

                // Invoke the button click after a delay (e.g., 2 seconds)
                float delayInSeconds = 1f;
                Invoke("AutoClick", delayInSeconds);
        }
    }

    private void AutoClick()
    {
        // Check if the buttonToClick reference is still valid
        if (buttonToClick != null)
        {
            // Programmatically click the button
            buttonToClick.onClick.Invoke();
        }
        else
        {
            Debug.LogError("Button reference is null!");
        }
    }

}
