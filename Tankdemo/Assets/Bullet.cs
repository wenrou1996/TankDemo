using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {



	public float speed = 10000000f;
	public GameObject explode;
    public float maxLiftTime = 2f;
    public float instantiateTime = 0f;

	//碰撞
	private void OnCollisionEnter(Collision collision)
	{
        //爆炸效果
        Instantiate(explode, transform.position, transform.rotation);
        //摧毁自身
        Destroy(gameObject);
	}

	// Use this for initialization
	void Start () {
        instantiateTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        //前进
        transform.position += transform.forward * speed * Time.deltaTime*10f;
        //摧毁
        if(Time.time - instantiateTime > maxLiftTime)
        {
            Destroy(gameObject);
        }
	}
}
