using UnityEngine;
using System.Collections;

public class scr_Hitbox : MonoBehaviour {

    //Collisions and shit
    [SerializeField]
    private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character

    public Collider[] attackHitboxes;

    private void Awake()
    {

    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.K))
        {
            LaunchAttack(attackHitboxes[0]);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LaunchAttack(attackHitboxes[1]);
        }
    }

    public void LaunchAttack(Collider col)
    {
        Collider[] cols = Physics.OverlapBox(col.bounds.center, col.bounds.extents, col.transform.rotation, m_WhatIsGround);
        foreach (Collider c in cols)
        {
            if (c.transform.parent.parent == transform)
                continue;

        }
    }

    void OnCollisionEnter()
    {
        //Debug.Log("Hit");
    }
}
