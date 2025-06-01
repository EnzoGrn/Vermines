using UnityEngine;
using UnityEngine.Playables;
using Vermines.CardSystem.Enumerations;

public class FinalAnimationManager : MonoBehaviour
{
    #region Exposed Properties
    [SerializeField] private TMPro.TextMeshProUGUI _winText;
    [SerializeField] private PlayableDirector _playableDirector;
    [SerializeField] private NarrationDialog _narrationDialog;
    [SerializeField] private float skipTime; // Time to skip to when skipping the cinematic
    #endregion

    #region Private Properties
    private bool _isWinner = false;
    #endregion

    private void Start()
    {
        // TODO: remove
        StartFinalAnimation(true, CardFamily.Cricket);

        // TODO: Register to a win event to start the final animation
        // GameEvents.OnPlayerWin.AddListener(OnPlayerWin);
    }

    private void OnDisable()
    {
    }

    public void OnPlayerWin(CardFamily family)
    {
        // TODO: Check if the family that won is mine
        //CardFamily testFamily = GameDataStorage.Instance.PlayerData[GameManager.Instance.Runner.LocalPlayer].Family;
        //Debug.Log($"[FinalAnimationManager] OnPlayerWin called with family: {family}, my family: {testFamily}");

        //if (family == testFamily)
        //{
        //    // If the family is mine, I win
        //    StartFinalAnimation(true, family);
        //}
        //else
        //{
        //    // If the family is not mine, I lose
        //    StartFinalAnimation(false, family);
        //}

        StartFinalAnimation(true, family);
    }

    private void StartFinalAnimation(bool isWinner, CardFamily family)
    {
        _isWinner = isWinner;
        _winText.text = "";

        string familiyName = family.ToString();
        Debug.Log($"[FinalAnimationManager] Starting final animation. Is winner: {_isWinner}, Family: {familiyName}");  
        _narrationDialog.SetKeyName(familiyName);

        _playableDirector.Play();
    }

    public void OnPlayableDirectorStopped()
    {
        Debug.Log($"[FinalAnimationManager] PlayableDirector stopped. Is winner: {_isWinner}");
        if (_isWinner)
            _winText.text = "You win!";
        else
            _winText.text = "You lose!";

        // TODO: Go back to the main menu or restart the game
    }

    public void SkipCinematic()
    {
        _playableDirector.time = skipTime;
        _playableDirector.Evaluate();
        _narrationDialog.SkipEntireDialog();
        Debug.Log($"[FinalAnimationManager] Skipped to time: {skipTime}");
    }
}
