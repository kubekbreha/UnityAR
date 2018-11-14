using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    public GameObject cube;

	void Update () {
        foreach(var touch in Input.touches){
            //Shoot(touch.position);

            if( touch.phase == TouchPhase.Began){
                Debug.Log("addCubbe");
                AddCube();
            }
        }
	}
	
    private void Shoot(Vector2 screenPoint) {
        var ray = Camera.main.ScreenPointToRay(screenPoint);
        var hitInfo = new RaycastHit();
            
        if(Physics.Raycast(ray, out hitInfo)){
            hitInfo.rigidbody.AddForceAtPosition(ray.direction, hitInfo.point);
        }
    }

    private void AddCube(){
        GameObject.Instantiate(cube, transform.position + (transform.forward * 0.3f), Random.rotation);
    }
}
