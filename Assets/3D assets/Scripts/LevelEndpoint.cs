using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelEndpoint : MonoBehaviour
{
    [Header("Where to go")]
    public string nextSceneName = "";
    public string menuSceneName = "MainMenu";

    [Header("Feedback")]
    public float delayBeforeLoad = 1.5f;
    public string levelCompleteMessage = "LEVEL COMPLETE!";
    public string gameCompleteMessage = "YOU WIN!\nThanks for playing";

    private SpawnAtCheckpoint player;
    private Collider col;
    private bool triggered;

    void Start()
    {
        player = FindFirstObjectByType<SpawnAtCheckpoint>();
        col = GetComponent<Collider>();

        if (col == null)
            Debug.LogWarning("LevelEndpoint: the Endpoint needs a Collider so it knows where its area is.");
        if (SceneManager.GetActiveScene().buildIndex < 0)
            Debug.LogWarning("LevelEndpoint: this scene is not in File > Build Profiles > Scene List, so the next level can't be found.");
    }

    void Update()
    {
        if (triggered || player == null || col == null) return;

        Bounds zone = col.bounds;
        zone.Expand(new Vector3(0.5f, 2f, 0.5f));

        if (zone.Intersects(player.GetBounds()))
        {
            triggered = true;
            StartCoroutine(FinishLevel());
        }
    }

    IEnumerator FinishLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        bool hasCustomScene = !string.IsNullOrEmpty(nextSceneName);
        bool isFinalLevel = !hasCustomScene && next >= SceneManager.sceneCountInBuildSettings;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        ShowMessage(isFinalLevel ? gameCompleteMessage : levelCompleteMessage);

        yield return new WaitForSeconds(delayBeforeLoad);

        if (hasCustomScene)
            SceneManager.LoadScene(nextSceneName);
        else if (isFinalLevel)
            SceneManager.LoadScene(menuSceneName);
        else
            SceneManager.LoadScene(next);
    }


    void ShowMessage(string text)
    {
        GameObject canvasGO = new GameObject("EndpointCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Dark overlay
        GameObject dim = new GameObject("Dim", typeof(RectTransform));
        dim.transform.SetParent(canvasGO.transform, false);
        Image img = dim.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.6f);
        img.raycastTarget = false;
        Stretch(dim.GetComponent<RectTransform>());

        // Message text
        GameObject textGO = new GameObject("Message", typeof(RectTransform));
        textGO.transform.SetParent(canvasGO.transform, false);
        Stretch(textGO.GetComponent<RectTransform>());
        Text t = textGO.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = text;
        t.fontSize = 90;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}