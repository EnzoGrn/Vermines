using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BurnController : MonoBehaviour
{
    [Header("Durée de l'effet")]
    [SerializeField] float burnDuration = 2.5f;

    [Header("Courbe d'animation (optionnel)")]
    [SerializeField] AnimationCurve burnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Détruire l'objet après la brûlure ?")]
    [SerializeField] bool destroyOnComplete = true;

    // Référence au matériau instancié (évite de modifier l'asset partagé)
    Material mat;
    static readonly int BurnAmountID = Shader.PropertyToID("_BurnAmount");

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        // On crée une copie du matériau pour ce personnage uniquement
        mat = new Material(sr.sharedMaterial);
        sr.material = mat;
        mat.SetFloat(BurnAmountID, 0f);
    }

    // -------------------------------------------------------
    // Appel public : déclenche la brûlure depuis n'importe où
    // -------------------------------------------------------
    public void StartBurn() => StartCoroutine(BurnRoutine());

    // Pratique pour un bouton UI ou un Input system
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            StartBurn();
    }

    IEnumerator BurnRoutine()
    {
        float t = 0f;

        while (t < burnDuration)
        {
            t += Time.deltaTime;
            float progress = burnCurve.Evaluate(t / burnDuration);
            mat.SetFloat(BurnAmountID, progress);
            yield return null;
        }

        mat.SetFloat(BurnAmountID, 1f);

        if (destroyOnComplete)
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Nettoyage propre du matériau instancié
        if (mat) Destroy(mat);
    }
}