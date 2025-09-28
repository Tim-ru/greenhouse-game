using UnityEngine;

public class CloudMoving : MonoBehaviour
{
    public float speed;

    public float leftBorder;
    public float rightBorder;

    private void FixedUpdate()
    {
        transform.localPosition -= Vector3.left * speed * Time.deltaTime;

        if (transform.localPosition.x <= leftBorder && speed < 0f)
        {
            transform.localPosition = new Vector3(rightBorder, transform.localPosition.y, transform.localPosition.z);
        }
        else if (transform.localPosition.x >= rightBorder && speed > 0f)
        {
            transform.localPosition = new Vector3(leftBorder, transform.localPosition.y, transform.localPosition.z);
        }
    }
}
