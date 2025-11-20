using UnityEngine;

public static class JsonHelper
{
    // Wrap array JSON into object with "items" then use JsonUtility.
    public static T[] FromJsonArray<T>(string jsonArray)
    {
        if (string.IsNullOrEmpty(jsonArray)) return new T[0];
        string wrapped = "{\"items\":" + jsonArray + "}";
        Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return w.items ?? new T[0];
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}
