using UnityEngine;
using System.Collections;

public class PPFXMeteor : MonoBehaviour {
	
	Vector3 groundPos = new Vector3(0,0,0);
	public Vector3 spawnPosOffset = new Vector3(0,0,0);
	
	public float speed = 10f;
	public GameObject detonationPrefab;
	
	public bool destroyOnHit;
	public bool setRateToNull;
	
	float dist = 0f;
	float radius = 2f;
	
	ParticleSystem[] psystems;
	
	public void Init () {
		
		//ground pos is spawn position
		groundPos = this.transform.position;
		//apply position offset
		this.transform.position = this.transform.position + spawnPosOffset;
		 
		//store current distance to ground
		dist = Vector3.Distance(transform.position, groundPos);	
		
		StartCoroutine(Move());
	} 
	
	
	IEnumerator Move () {
		//quarter up emission rate during flight
		psystems = GetComponentsInChildren<ParticleSystem>(); 
		foreach(ParticleSystem _system in psystems)
		{
            if (_system != null && _system.isPlaying)
            {
                var _emission = _system.emission;
                var _rate = _emission.rateOverTime;
                _rate.constantMax = 0f;
                _emission.rateOverTime = _rate;
            }
		}
		
		
		while(dist > radius)
        { 
			
        	float step = speed * Time.deltaTime;
    		transform.position = Vector3.MoveTowards(transform.position,groundPos, step);//ref velocity,
        	
        	dist = Vector3.Distance(transform.position, groundPos);
        	yield return null;
        }
		if(setRateToNull)
        {
        	//set emission rate to zero
			foreach(ParticleSystem _system in psystems)
			{
                if (_system != null)
                {
                    var _emission = _system.emission;
                    var _rate = _emission.rateOverTime;
                    _rate.constantMax = 0f;
                    _emission.rateOverTime = _rate;
                }
            }
            if (detonationPrefab != null)
            {
                GameObject go = Instantiate(detonationPrefab, this.transform.position, detonationPrefab.transform.rotation);
                Destroy(go, 1.5f);
            }
           
            Destroy(gameObject, 1.5f);
        }
	}
}
