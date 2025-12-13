using UnityEngine;

public class TextBoxManager : MonoBehaviour
{
    public GameObject[] textBoxes;

    private int currentIndex = -1;

    public void ShowTextBox(int index)
    {
        // Deactivate currently active textbox
        if (currentIndex >= 0 && currentIndex < textBoxes.Length)
        {
            textBoxes[currentIndex].SetActive(false);
        }

        // Activate new textbox
        textBoxes[index].SetActive(true);
        currentIndex = index;
    }

    // 🔴 RESET METHOD
    public void ResetAll()
    {
        foreach (var box in textBoxes)
        {
            box.SetActive(false);
        }

        currentIndex = -1;
    }
}
