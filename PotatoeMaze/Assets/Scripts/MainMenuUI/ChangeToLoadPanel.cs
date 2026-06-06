using UnityEngine;
using UnityEngine.UI;

public class ChangeToLoadPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject mainPanel;
    
    [SerializeField]
    private GameObject loadingPanel;

    [SerializeField]
    private Button playButton;

    void Start()
    {
        playButton.onClick.AddListener(LoadLoagingPanel);
    }

    void LoadLoagingPanel()
    {
        mainPanel.SetActive(false);
        loadingPanel.SetActive(true);
    } 

}
