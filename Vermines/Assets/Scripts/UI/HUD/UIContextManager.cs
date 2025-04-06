using UnityEngine;

public class UIContextManager : MonoBehaviour
{
    public static UIContextManager Instance { get; private set; }

    private IUIContext _current;

    private void Awake()
    {
        Instance = this;
    }

    public bool HasContext() => _current != null;

    public void SetContext(IUIContext context)
    {
        ClearContext();
        _current = context;
        _current.Enter();
    }

    public void ClearContext()
    {
        _current?.Exit();
        _current = null;
    }

    public IUIContext Current => _current;
}
