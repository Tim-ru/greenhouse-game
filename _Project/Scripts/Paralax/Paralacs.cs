using UnityEngine;

public class Paralacs : MonoBehaviour
{
    private Transform[] paralacsChild;

    public float speed;

    private float[] pointsMove;

    private void Start()
    {
        // Проверяем, что у объекта есть достаточно дочерних элементов
        if (transform.childCount < 2)
        {
            Debug.LogError($"Paralacs script requires at least 2 child objects, but {gameObject.name} has only {transform.childCount} children.");
            enabled = false; // Отключаем скрипт, если недостаточно дочерних объектов
            return;
        }

        paralacsChild = new Transform[transform.childCount];
        pointsMove = new float[transform.childCount];

        for (int i = 0; i < paralacsChild.Length; i++)
        {
            paralacsChild[i] = transform.GetChild(i);
        }

        pointsMove[0] = -transform.GetChild(1).localPosition.x;
        pointsMove[1] = transform.GetChild(1).localPosition.x;
    }

    private void FixedUpdate()
    {
        // Дополнительная проверка безопасности
        if (paralacsChild == null || paralacsChild.Length < 2 || pointsMove == null || pointsMove.Length < 2)
        {
            return;
        }

        paralacsChild[0].localPosition += new Vector3(1, 0, 0) * speed * Time.deltaTime;

        if (paralacsChild[0].localPosition.x >= pointsMove[1])
        {
            paralacsChild[0].localPosition = new Vector3(pointsMove[0], paralacsChild[0].localPosition.y, paralacsChild[0].localPosition.z);
            paralacsChild[1].localPosition = new Vector3(0, paralacsChild[1].localPosition.y, paralacsChild[1].localPosition.z);
        }

        paralacsChild[1].localPosition += new Vector3(1, 0, 0) * speed * Time.deltaTime;

        if (paralacsChild[1].localPosition.x >= pointsMove[1])
        {
            paralacsChild[1].localPosition = new Vector3(pointsMove[0], paralacsChild[1].localPosition.y, paralacsChild[1].localPosition.z);
            paralacsChild[0].localPosition = new Vector3(0, paralacsChild[0].localPosition.y, paralacsChild[0].localPosition.z);
        }
    }
}
