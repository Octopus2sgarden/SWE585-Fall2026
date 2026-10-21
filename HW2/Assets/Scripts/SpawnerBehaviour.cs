using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpawnerBehaviour : MonoBehaviour
{

	[SerializeField] Button resetAllButton;


	// Start is called before the first frame update
	void Start()
	{
		resetAllButton.onClick.AddListener(ResetAll);
	}

	public void ResetAll()
	{
		Scene currentScene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(currentScene.buildIndex, LoadSceneMode.Single);
	}

}
