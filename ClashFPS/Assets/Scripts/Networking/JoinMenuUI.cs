using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class JoinMenuUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private TMP_InputField joinField;
    string joinText;

    private void Start()
    {
        DontDestroyOnLoad(this);

        hostBtn.onClick.AddListener(() => StartCoroutine(LoadArena(true)));

        joinBtn.onClick.AddListener(() => StartCoroutine(LoadArena(false)));
    }

    private IEnumerator LoadArena(bool host){
        joinText = joinField.text;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Arena");
        while (!asyncLoad.isDone)
            yield return null;

        if(host)
            Host();
        else
            Join();
    }

    private async void Host(){
        RelayManager relayManager = FindFirstObjectByType<RelayManager>();
        await relayManager.StartManager();

        relayManager.CreateRelay();
        Destroy(gameObject);
    }

    private async void Join(){
        RelayManager relayManager = FindFirstObjectByType<RelayManager>();
        await relayManager.StartManager();
        
        relayManager.JoinRelay(joinText);
        Destroy(gameObject);
    }
}
