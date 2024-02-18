using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarControl : MonoBehaviour
{
    [SerializeField] private Image _HealthSprite;
    [SerializeField] private Image _HealthSpriteBack;
    [SerializeField] private Image _FocusSprite;
    [SerializeField] private Image _FocusSpriteBack;
    [SerializeField] private Image _HungerSprite;
    [SerializeField] private Image _HungerSpriteBack;
    private Camera _cam;
    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = Quaternion.LookRotation(this.transform.position - _cam.transform.position);
    }

    public void UpdateHealthBar(float current, float max)
    {
        _HealthSprite.fillAmount = current / max;
        _HealthSprite.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(max, 100);
        _HealthSpriteBack.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(max, 100);
    }

    public void UpdateFocusBar(float current, float max)
    {
        _FocusSprite.fillAmount = current / max;
        _FocusSprite.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(max, 100);
        _FocusSpriteBack.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(max, 100);
    }

    public void UpdateHungerBar(float current, float max)
    {
        _HungerSprite.fillAmount = current / max;
        _HungerSprite.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(max, 100);
        _HungerSpriteBack.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(max, 100);
    }
}
