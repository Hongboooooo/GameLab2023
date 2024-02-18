using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarControl : MonoBehaviour
{
    [SerializeField] private Image PBF;
    [SerializeField] private Image PBB;
    private Camera _cam;
    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
        UpdateProgressBar(0,100);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = Quaternion.LookRotation(this.transform.position - _cam.transform.position);
    }

    public void UpdateProgressBar(float current, float max)
    {
        PBF.fillAmount = current / max;
        // _HealthSprite.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(max, 100);
        // _HealthSpriteBack.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(max, 100);
    }
}
