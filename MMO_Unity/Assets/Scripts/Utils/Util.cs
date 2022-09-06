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

    // GameObject ���� FindChild. �Ʒ��� �������� �ٸ��� ������Ʈ�� �ִ� ������Ʈ�� ã���ִ� ���� �ƴ϶� ������Ʈ �� ��ü�� ã�Ƽ� ��ȯ��.
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;
        return transform.gameObject;
    }

    /* recursive �Ű� ��Ÿ�� ������ ��������� Ž���� �������� ����.
       ���� ��������� Ž���� �������� �ʴ´ٸ� go ���� ���� �ڳ� ������Ʈ�鸸 Ž��.
       ��������� Ž���� �����Ѵٸ� go ���� ���� �ڳ� ������Ʈ �Ӹ� �ƴ϶� �� ������Ʈ���� �ڽ� ������Ʈ���� Ž��. */
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
