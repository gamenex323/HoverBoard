using UnityEngine;

[System.Serializable]
public class PIDController {

	public float pCoeff = .8f;		// .8f;
	public float iCoeff = .0002f;   // .0002f;
	public float dCoeff = .1f;      // .2f;
	public float minimum = -1;
	public float maximum = 2;

	//Variables to store values between calculations
	float integral;
	float lastProportional;


	public float Seek (float seekValue, float currentValue) {
		float deltaTime = Time.fixedDeltaTime;
		float proportional = seekValue - currentValue;

		float derivative = (proportional - lastProportional) / deltaTime;
		integral += proportional * deltaTime;
		lastProportional = proportional;

		float value = pCoeff * proportional + iCoeff * integral + dCoeff * derivative;
		value = Mathf.Clamp (value, minimum, maximum);

		return value;
	}
}