using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCreater : MonoBehaviour
{
    public int count = 0;
    public GameObject prefab;
    [ContextMenu("Create")]
    public void Create()
    {
        for (int i = 1; i < count; i++)
        {
            Vector3 pos = new Vector3(0, 0, i * 50);
            Instantiate(prefab, pos, Quaternion.identity);
        }
    }
}
