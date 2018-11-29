using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioButtonPool : MonoBehaviour {

	public GameObject prefab;
	public Stack<GameObject> inactiveInstances = new Stack<GameObject>();

	public GameObject GetObject() {
		GameObject spawned_game_object;

		if (inactiveInstances.Count > 0) {
			spawned_game_object = inactiveInstances.Pop();
		} else {
			/* Adding the this.transform fixed the size of the button by adding
			   the parent...
			 */
			spawned_game_object = Instantiate(prefab, this.transform);

			PooledObject pooled_object = spawned_game_object.AddComponent<PooledObject>();
			pooled_object.pool = this;
		}

		spawned_game_object.transform.SetParent(this.transform);
		spawned_game_object.SetActive(true);

		return spawned_game_object;
	}

	public void ReturnObject(GameObject toReturn) {
		PooledObject pooled_object = toReturn.GetComponent<PooledObject>();

		if(pooled_object != null && pooled_object.pool == this) {
			toReturn.transform.SetParent(transform);
			toReturn.SetActive(false);

			inactiveInstances.Push(toReturn);
		} else {
			Debug.LogWarning(toReturn.name + " was return to pool, wasn't spawned (Destroying");
			Destroy(toReturn);
		}
	}
}

public class PooledObject : MonoBehaviour 
{
	public AudioButtonPool pool;
}
