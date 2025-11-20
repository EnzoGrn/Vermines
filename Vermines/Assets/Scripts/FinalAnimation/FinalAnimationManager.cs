using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Vermines;
using Vermines.CardSystem.Enumerations;

public class FinalAnimationManager : MonoBehaviour
{
    #region Exposed Properties
    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI _winText;
    [SerializeField] private UnityEngine.UI.Image _godImg;
    [Header("Configuration")]
    [SerializeField] private float skipTime; // Time to skip to when skipping the cinematic
    [Header("Objects to handle")]
    [SerializeField] private NarrationDialog _narrationDialog;
    [SerializeField] private PlayableDirector _playableDirector;
    [SerializeField] private Button _skipButton;
    #endregion

    #region Private Properties
    private bool _isWinner = false;
    #endregion

    private void Start()
    {
        GameEvents.OnPlayerWin.AddListener(OnPlayerWin);
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerWin.RemoveListener(OnPlayerWin);
    }

    #region Private Methods
    private void StartFinalAnimation(bool isWinner, CardFamily family)
    {
        Debug.Log($"[FinalAnimationManager] Starting final animation. Is winner: {isWinner}, Family: {family}");

        _isWinner = isWinner;
        _winText.text = "";

        string familiyName = family.ToString();
        _narrationDialog.SetKeyName(familiyName);

        _playableDirector.Play();
    }
    #endregion 

    #region Public Methods

    public void OnPlayerWin(PlayerRef winnerRef, PlayerRef localPlayerRef)
    {
        Debug.LogError($"Refactor TODO: OnPlayerWin()");

        /*if (GameManager.Instance == null)
            return;

        // Get the family of the winner
        CardFamily winnerFamily = GameDataStorage.Instance.PlayerData[winnerRef].Family;

        // TODO: Set the image of the god from playerData
        // _godImg.texture = GameDataStorage.Instance.PlayerData[winnerRef].GodImage;

        if (winnerRef == localPlayerRef)
        {
            // If the family is mine, I win
            StartFinalAnimation(true, winnerFamily);
        }
        else
        {
            // If the family is not mine, I lose
            StartFinalAnimation(false, winnerFamily);
        }*/
    }

    public void OnPlayableDirectorStopped()
    {
        if (_isWinner)
            _winText.text = "You win!";
        else
            _winText.text = "You lose!";
    }

    public async void ReturnToMenu()
    {
        Debug.LogError($"Refactor TODO: ReturnToMenu()");
        // await GameManager.Instance.ReturnToMenu();
    }

    public void SkipCinematic()
    {
        _playableDirector.time = skipTime;
        _playableDirector.Evaluate();
        _narrationDialog.SkipEntireDialog();
    }

    public void UnloadSceneForCinematic()
    {
        Debug.LogError($"TODO: UnloadSceneForCinematic");
        /*if (GameManager.Instance == null)
            return;

        List<string> sceneToUnload = new() {
            "UI",
            "GameplayCameraTravelling"
        };

        GameManager.Instance.UnloadSceneForCinematic(sceneToUnload);*/
    }

    public void OnPauseRequest()
    {
        _narrationDialog.OnDialogComplete.AddListener(ResumeTimeline);
        _playableDirector.Pause();
    }

    public void ResumeTimeline()
    {
        _narrationDialog.OnDialogComplete.RemoveAllListeners();
        _playableDirector.Resume();
        _skipButton.interactable = false;
    }

    #endregion
}
