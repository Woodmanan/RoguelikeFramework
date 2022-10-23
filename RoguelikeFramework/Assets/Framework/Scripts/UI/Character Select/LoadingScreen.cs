using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public Image loadingBar;
    float fillAmount = 0.0f;
    float timeToFill = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLoading()
    {
        gameObject.SetActive(true);
        StartCoroutine(RunLoadingAnim());
    }

    IEnumerator RunLoadingAnim()
    {
        float speed = 1.0f;
        float goal = 0.0f;
        while (LevelLoader.singleton.current < LevelLoader.singleton.preloadUpTo)
        {
            yield return null;
            float newGoal = ((float)LevelLoader.singleton.current + 1) / LevelLoader.singleton.preloadUpTo;
            if (newGoal != goal)
            {
                goal = newGoal;
                speed = Mathf.Clamp((newGoal - fillAmount) / timeToFill, 0.0f, 10f);
            }
            if (newGoal == 1.0f)
            {
                speed = (1.0f - fillAmount) / 0.5f;
            }

            if (fillAmount < goal)
            {
                fillAmount += speed * Time.deltaTime;
            }
            loadingBar.fillAmount = fillAmount;
        }
    }
}
