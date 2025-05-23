using System.Collections.Generic;
using UnityEngine;
public class Mocks : MonoBehaviour
{
    private List<Mock> mocks = new List<Mock>();
    public static Mocks Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UnitsChanged()
    {
        mocks.Clear();
        // Add all intial ones i.e only players?
        foreach (var mock in GetComponentsInChildren<Mock>())
            mocks.Add(mock);
    }


    public bool IsTileFree(Vector2Int pos)
    {
        UnitsChanged();
        foreach (var mock in mocks) {
            if (mock.pos == pos)
                return false;
        }
        return true;
    }

}
