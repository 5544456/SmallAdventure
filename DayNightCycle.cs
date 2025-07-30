using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public Light moon;
    public Material skyboxMaterial;
    public Gradient skyboxColor;
    public AnimationCurve sunIntensity;
    public float dayDuration = 120f;
    private float time;

    void Update()
    {
        time += Time.deltaTime;
        float dayProgress = (time % dayDuration) / dayDuration;

        // Смена положения солнца
        sun.transform.rotation = Quaternion.Euler(dayProgress * 360f - 90f, 170f, 0f);

        // Изменение цвета и интенсивности солнца
        if (sunIntensity != null)
            sun.intensity = sunIntensity.Evaluate(dayProgress);
        if (skyboxMaterial != null && skyboxColor != null)
            skyboxMaterial.SetColor("_Tint", skyboxColor.Evaluate(dayProgress));

        // Активация луны ночью
        if (dayProgress > 0.5f)
        {
            moon.enabled = true;
            sun.enabled = false;
        }
        else
        {
            moon.enabled = false;
            sun.enabled = true;
        }
    }
}
