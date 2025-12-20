using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Tutorial/Page")]
public class TutorialPage : ScriptableObject
{
    public string title;

    [TextArea(3, 10)]
    public string description;

    public Sprite image;      // optional
}
