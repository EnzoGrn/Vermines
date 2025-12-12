using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class LODSetupTool : EditorWindow
{
    private GameObject _ParentReference = null;
    private int _NumberofLOD = 3;
    private float[] _LodScreenPercentages = new float[3] { 0.10f, 0.05f, 0f };

    [MenuItem("Window/LOD Setup Tools")]
    public static void ShowWindow()
    {
        LODSetupTool window = GetWindow<LODSetupTool>("LOD Setup Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("LOD Setup for Children", EditorStyles.boldLabel);

        _ParentReference = (GameObject)EditorGUILayout.ObjectField("Parent Reference", _ParentReference, typeof(GameObject), true);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate LODs"))
        {
            if (_ParentReference == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a parent GameObject.", "OK");
                return;
            }
            GeneratesLODFromParent();
        }
    }

    private void GeneratesLODFromParent()
    {

        Debug.Log($"Parent ${_ParentReference.name} has ${_ParentReference.transform.childCount} child !");


        Transform[] childrens = new Transform[_ParentReference.transform.childCount];

        for (int i = 0; i < _ParentReference.transform.childCount; i++)
        {
            childrens[i] = _ParentReference.transform.GetChild(i);
        }

        foreach (Transform child in childrens)
        {
            if (child == null)
                continue;

            Debug.Log("child -> " + child.name);

            GameObject gameObject = child.gameObject;

            if (gameObject == null || gameObject.GetComponent<MeshRenderer>() == null ||
                gameObject.GetComponent<MeshFilter>() == null || gameObject.transform.childCount > 0)
            {
                Debug.LogWarning($"Gameobject ${gameObject.name} doesn't have a Mesh associated or has child ${gameObject.transform.childCount}");

                continue;
            }

            // check if the gameobject has childrens, in that case don't apply the LOD
            Transform[] checkChildren = gameObject.GetComponentsInChildren<Transform>();

            // Creating a new parent and add it as a child of the main parent
            GameObject lodParent = new (gameObject.name + "_LODParent");
            
            if (lodParent == null)
                continue;

            GameObjectUtility.SetStaticEditorFlags(lodParent, StaticEditorFlags.ContributeGI);

            // Collect renderers for LODs
            Renderer[] lodRenderers = new Renderer[_NumberofLOD];
            lodParent.transform.SetParent(_ParentReference.transform);
            LODGroup lodGroup = lodParent.AddComponent<LODGroup>();
            gameObject.transform.SetParent(lodParent.transform);

            Renderer gameObjectRenderer = gameObject.GetComponent<Renderer>();

            if (gameObjectRenderer != null)
                gameObjectRenderer.forceMeshLod = 0;

            lodRenderers[0] = gameObjectRenderer;

            for (int j = 1; j < _NumberofLOD; j++)
            {
                GameObject sibling = PrefabUtility.InstantiatePrefab(gameObject.GetPrefabDefinition(), gameObject.transform) as GameObject;

                Debug.Log(gameObject.name);

                if (sibling == null)
                {
                    Debug.LogError("Sibling isn't created correctly");
                    return;
                }

                sibling.layer = gameObject.layer;

                GameObjectUtility.SetStaticEditorFlags(sibling, GameObjectUtility.GetStaticEditorFlags(gameObject));

                Renderer renderer = sibling.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.forceMeshLod = (short)j;
                    renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                }

                lodRenderers[j] = renderer;

                sibling.name = gameObject.name + "_LOD0" + j;
                sibling.transform.SetParent(lodParent.transform);
            }

            LOD[] lods = new LOD[_NumberofLOD];

            for (int j = 0; j < _NumberofLOD; j++)
            {
                lods[j] = new LOD(_LodScreenPercentages[j], new Renderer[] { lodRenderers[j] });
            }

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
        }
    }
}
