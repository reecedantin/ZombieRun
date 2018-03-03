using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour {

	public GameObject zombo;
	public float waittime = 0;
    public ArrayList zombies = new ArrayList();
	public GameObject arrowPrefab;

	// Use this for initialization
	void Start () {
        Debug.Log("Game started");
		//Vector3 zombieLoc = new Vector3(transform.position.x + Random.Range(-5,5), transform.position.y, transform.position.z + Random.Range(-5,5));
		//Instantiate(zombo, zombieLoc, zombo.transform.rotation);
	}

	// Update is called once per frame
	void Update () {
		if (waittime > 360) {
			waittime = 0;
			Vector3 zombieLoc = new Vector3 (transform.position.x + Random.Range (-2, 2), transform.position.y - 1, transform.position.z + 4);
			zombies.Add(Instantiate (zombo, zombieLoc, zombo.transform.rotation));
            Debug.Log("camera location is " + zombieLoc);
		}
		waittime++;


        if (Input.touchCount > 0) {
            foreach(GameObject zombie in zombies) {
                GameObject.Destroy(zombie);
            }

			Instantiate (arrowPrefab, transform.position, transform.rotation);

        }
	}
}
