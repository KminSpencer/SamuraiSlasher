using UnityEngine;
using System.Collections;

public class scr_Rotate : MonoBehaviour {

    private Transform m_Transform;

	// Use this for initialization
	void Start () {
        m_Transform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward * Time.deltaTime);

    }
}
