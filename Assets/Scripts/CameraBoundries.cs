using UnityEngine;
using UnityEngine.UI;

public class CameraBoundries : MonoBehaviour
{
    [SerializeField] Image removeImage;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            removeImage.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            removeImage.gameObject.SetActive(false);
    }
}
