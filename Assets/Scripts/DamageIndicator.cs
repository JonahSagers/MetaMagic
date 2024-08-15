using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    public TextMeshPro display;
    public Gradient damageGradient;
    public Gradient healGradient;
    private Gradient gradient;
    // Start is called before the first frame update
    public IEnumerator DisplayDamage(float damage)
    {
        if(damage < 0){
            gradient = healGradient;
            damage = Mathf.Abs(damage);
        } else {
            gradient = damageGradient;
        }
        display.text = damage.ToString();
        display.color = gradient.Evaluate(0);
        while(transform.localScale.x < 2f + Mathf.Max(Mathf.Log(damage/10,5), -1)){
            transform.localScale += Vector3.one * Time.deltaTime * 10 * (2f + Mathf.Max(Mathf.Log(damage/10,5), -1));
            yield return 0;
        }
        float sideVel = Random.Range(-0.6f,0.6f);
        Vector3 velocity = new Vector3(sideVel,Random.Range(4.0f,5.0f),sideVel);
        float elapsed = 0;
        while(elapsed < 1f){
            if(transform.localScale.x > (2f + Mathf.Max(Mathf.Log(damage/10,5), -1)) * 0.9f){
                transform.localScale -= Vector3.one * Time.deltaTime * 15;
            }
            transform.position += velocity * Time.deltaTime;
            velocity.y -= 10f * Time.deltaTime;
            display.color = gradient.Evaluate(elapsed);
            elapsed += Time.deltaTime;
            yield return 0;
        }
        Destroy(gameObject);
    }
}
