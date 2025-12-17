using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class FloatingHealthBar : MonoBehaviour
{
    public Transform target;        
    public Vector3 worldOffset = new Vector3(0, 3f, 0); 
    public RectTransform canvasRect;
    public Camera uiCamera;         
    public Image fillImage;
    public TMP_Text hpText;

    void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf) gameObject.SetActive(true);

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

 
        if (screenPos.z < 0f)
        {
            transform.gameObject.SetActive(false);
            return;
        }
        else transform.gameObject.SetActive(true);


        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out anchoredPos);
        (transform as RectTransform).anchoredPosition = anchoredPos;


        PlayerHealth ph = target.GetComponent<PlayerHealth>();
        BossAI bi = target.GetComponent<BossAI>();
        float cur = 0, max = 1;
        if (ph != null) { cur = ph.GetHealth(); max = ph.GetMaxHealth(); }
        else if (bi != null) { cur = bi.GetHealth(); max = bi.GetMaxHealth(); }

        float fill = (max > 0) ? Mathf.Clamp01(cur / max) : 0f;
        if (fillImage != null) fillImage.fillAmount = fill;
        if (hpText != null) hpText.text = Mathf.CeilToInt(cur) + " / " + Mathf.CeilToInt(max);
    }
}
