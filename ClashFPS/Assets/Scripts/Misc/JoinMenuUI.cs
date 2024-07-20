using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JoinMenuUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private TMP_InputField joinField;
    [SerializeField] private RelayManager relayManager;

    private void Start()
    {
        hostBtn.onClick.AddListener(() => relayManager.CreateRelay());
        joinBtn.onClick.AddListener(() => relayManager.JoinRelay(joinField.text));
    }
}
