using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    // GameObject 전용 FindChild. 아래의 버전과는 다르게 오브젝트에 있는 컴포넌트를 찾아주는 것이 아니라 오브젝트 그 자체를 찾아서 반환함.
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;
        return transform.gameObject;
    }

    /* recursive 매개 불타입 변수는 재귀적으로 탐색할 것인지의 여부.
       만약 재귀적으로 탐색을 진행하지 않는다면 go 밑의 직속 자녀 오브젝트들만 탐색.
       재귀적으로 탐색을 진행한다면 go 밑의 직속 자녀 오브젝트 뿐만 아니라 그 오브젝트들의 자식 오브젝트까지 탐색. */
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);

                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }
}
