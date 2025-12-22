using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Vermines.CardSystem.Enumerations;
using Vermines.Core;
using Vermines.Core.Scene;
using Vermines.Core.Services;
using Vermines.Player;

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

    [SerializeField]
    private List<string> _SceneToUnload;

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
        SceneContext context = PlayerController.Local.Context;

        // Get the family of the winner
        CardFamily winnerFamily = context.NetworkGame.GetPlayer(winnerRef).Statistics.Family;

        // TODO: Set the image of the god from playerData
        // _godImg.texture = GameDataStorage.Instance.PlayerData[winnerRef].GodImage;

        if (winnerRef == localPlayerRef) {
            // If the family is mine, I win
            StartFinalAnimation(true, winnerFamily);
        } else {
            // If the family is not mine, I lose
            StartFinalAnimation(false, winnerFamily);
        }
    }

    public void OnPlayableDirectorStopped()
    {
        if (_isWinner)
            _winText.text = "You win!";
        else
            _winText.text = "You lose!";
    }

    public void ReturnToMenu()
    {
        SceneContext context = PlayerController.Local.Context;

        if (context != null && context.GameplayMode != null)
            context.GameplayMode.StopGame();
        else
            Global.Networking.StopGame();
    }

    public void SkipCinematic()
    {
        _playableDirector.time = skipTime;
        _playableDirector.Evaluate();
        _narrationDialog.SkipEntireDialog();
    }

    public void UnloadSceneForCinematic()
    {
        foreach (string scene in _SceneToUnload)
            StartCoroutine(PersistentSceneService.Instance.UnloadScene(scene));
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
