using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectController : MonoBehaviour
{
    public static CharacterSelectController singleton
    {
        get
        {
            if (Singleton == null)
            {
                Singleton = GameObject.FindObjectOfType<CharacterSelectController> ();
            }
            return Singleton;
        }
        set
        {
            Singleton = value;
        }
    }

    private static CharacterSelectController Singleton;

    public Monster chosenSpecies;
    public ClassGenerator classGenerator;
    public WorldGenerator chosenGenerator;
    public List<string> generationOptions;
    public LoadingScreen loadingScreen;
    public string startAt;

    // Start is called before the first frame update
    void Start()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        if (Singleton != this)
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LaunchGame()
    {
        StartCoroutine(LaunchGameRoutine());
        
    }

    public void SetChosenGenerator(WorldGenerator generator)
    {
        chosenGenerator = generator;
    }

    IEnumerator LaunchGameRoutine()
    {
        yield return SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainScene"));

        loadingScreen.StartLoading();
        Debug.Assert(LevelLoader.singleton.generators.Count == 0, "Level loader was not set up!");
        chosenGenerator = Instantiate(chosenGenerator);

        Player.player = Instantiate(chosenSpecies);

        if (classGenerator.IsChoicePendingUnlock())
        {
            //TODO: Yield wait for unlock screen!
        }
        Class chosenClass = classGenerator.GenerateClass();
        chosenClass.Apply(Player.player);
        if (Player.player is Player player)
        {
            player.chosenClass = chosenClass;
        }

        Player.player.AddEffectInstantiate(chosenGenerator.playerPassives.ToArray());
        Player castPlayer = Player.player as Player;

        foreach (string s in generationOptions)
        {
            chosenGenerator.generationOptions.Add(s);
        }

        LevelLoader.singleton.worldGen = chosenGenerator;
        LevelLoader.singleton.startAt = startAt;
        LevelLoader.singleton.BeginGeneration();

        GameController.singleton.StartGame();

        while (LevelLoader.singleton.current < LevelLoader.singleton.preloadUpTo)
        {
            yield return null;
        }

        yield return new WaitForSeconds(.5f);

        SceneManager.UnloadSceneAsync("CharacterSelect");
    }
}
