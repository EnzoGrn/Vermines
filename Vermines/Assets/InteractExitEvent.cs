using UnityEngine;

public class InteractExitEvent : StateMachineBehaviour
{
    public string functionName;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the parent of the animator
        Transform parent = animator.transform.parent;

        if (parent != null)
        {
            // Try access the NpcController component on the parent
            var script = parent.GetComponent<NpcController>();

            if (script != null)
            {
                script.Invoke(functionName, 0f);
            }
            else
            {
                Debug.LogWarning($"Script 'ScriptDeMonPrefab' not found from {animator.gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"GameObject {animator.gameObject.name} doesn't have parent");
        }
    }
}
