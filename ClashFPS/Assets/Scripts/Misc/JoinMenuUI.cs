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

    private void Start()
    {
        DontDestroyOnLoad(this);

        hostBtn.onClick.AddListener(() => StartCoroutine(Host()));

        joinBtn.onClick.AddListener(() => StartCoroutine(Join()));
    }

    private IEnumerator Host(){
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Arena");
        while (!asyncLoad.isDone)
            yield return null;
        yield return new WaitForSeconds(0.25f);

        FindFirstObjectByType<RelayManager>().CreateRelay();
        Destroy(gameObject);
    }

    private IEnumerator Join(){
        string joinText = joinField.text;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Arena");
        while (!asyncLoad.isDone)
            yield return null;
        yield return new WaitForSeconds(0.25f);

        FindFirstObjectByType<RelayManager>().JoinRelay(joinText);
        Destroy(gameObject);
    }
}
