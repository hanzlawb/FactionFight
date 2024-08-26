using UnityEngine;

public class NameTagManager : MonoBehaviour
{
    public float nameTagHeight = 2f;
    public int fontSize = 100; // Larger font size for better clarity
    public float characterSize = 0.1f; // Smaller character size to scale down the text
    public Color fontColor = Color.white;

    public void CreateNameTag(GameObject bot, string name)
    {
        GameObject nameTag = new GameObject("NameTag");
        nameTag.transform.SetParent(bot.transform);
        nameTag.transform.localPosition = new Vector3(0, nameTagHeight, 0); // Adjust height using inspector

        TextMesh textMesh = nameTag.AddComponent<TextMesh>();
        textMesh.text = name; // Set the bot's name here
        textMesh.fontSize = fontSize; // Set to a larger font size
        textMesh.characterSize = characterSize; // Set character size to scale down the text
        textMesh.color = fontColor; // Set font color using inspector
        textMesh.anchor = TextAnchor.MiddleCenter;

        // Attach the NameTag script to make it face the camera
        nameTag.AddComponent<NameTag>();
    }


}
