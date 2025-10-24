using UnityEngine;

public class CauldronOpenDoorOnStart : MonoBehaviour
{
    [Header("Дверь")]
    public GameObject door;           // Перетащи сюда объект "Door"
    public float openHeight = 3f;     // На сколько поднять
    public float openSpeed = 2f;      // Скорость подъёма

    private void Start()
    {
        OpenDoor();
    }

    private void OpenDoor()
    {
        if (door == null)
        {
            door = GameObject.Find("Door"); // Автопоиск
        }

        if (door != null)
        {
            // Мгновенно
            door.transform.position += Vector3.up * openHeight;

            // ИЛИ плавно (раскомментируй, если хочешь анимацию)
            // StartCoroutine(MoveDoorUp());
        }
    }

    // Плавное открытие (опционально)
    private System.Collections.IEnumerator MoveDoorUp()
    {
        Vector3 start = door.transform.position;
        Vector3 target = start + Vector3.up * openHeight;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            door.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
    }
}