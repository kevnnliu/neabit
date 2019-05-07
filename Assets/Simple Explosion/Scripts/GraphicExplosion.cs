// script to render explosion
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GraphicExplosion : NetworkBehaviour {
	public float loopduration;
	private float ramptime=0;
	private float alphatime=1;
	private float awakeTime=0;

	void Start() {
		awakeTime = 0;
		if (isClient) {
			GetComponent<AudioSource>().mute = true;
		}
	}
	void Update () {
		awakeTime += Time.deltaTime;
		if (awakeTime >= 8) {
			CmdDestroy(this.gameObject);
		}
		ramptime+=Time.deltaTime*2;
		alphatime-=Time.deltaTime;		
		float r = Mathf.Sin((Time.time / loopduration) * (2 * Mathf.PI)) * 0.5f + 0.25f;
		float g = Mathf.Sin((Time.time / loopduration + 0.33333333f) * 2 * Mathf.PI) * 0.5f + 0.25f;
		float b = Mathf.Sin((Time.time / loopduration + 0.66666667f) * 2 * Mathf.PI) * 0.5f + 0.25f;
		float correction = 1 / (r + g + b);
		r *= correction;
		g *= correction;
		b *= correction;
		GetComponent<Renderer>().material.SetVector("_ChannelFactor", new Vector4(r,g,b,0));
		GetComponent<Renderer>().material.SetVector("_Range", new Vector4(ramptime,0,0,0));
		GetComponent<Renderer>().material.SetFloat("_ClipRange", alphatime);
	}

	[Command]
	public void CmdDestroy(GameObject go) {
		NetworkServer.Destroy(go);
	}
}
