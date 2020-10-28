using UnityEngine;

public class CameraFollower : MonoBehaviour
{
	public Transform Target;

	void Update()
	{
		if (Target != null)
		{
			transform.position = Target.transform.position;
		}
	}
}
