using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour {
    public StatCanvasController statCanvasPrefab;
    private Stats stats;
    private StatCanvasController statsCanvas;

    private void Awake()
    {
        stats = GetComponent<Stats>();
        statsCanvas = GetComponentInChildren<StatCanvasController>();
        if (statCanvasPrefab && statsCanvas == null)
        {
            statsCanvas = Instantiate(statCanvasPrefab.gameObject, this.transform).GetComponent<StatCanvasController>();
        }

        if (statsCanvas) { 
            stats.OnChangeConsumed += (d) => statsCanvas.ChangeText(d.ToString());
            stats.OnChangeHealth += ((c, m) => statsCanvas.ChangeValue(c/m));
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        ranInto(collision.gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        ranInto(other.gameObject);
    }

    private void ranInto(GameObject other) {
        foreach (string tag in stats.Menu)
        {
            if (other.CompareTag(tag))
            {
                // It's on the menu
                stats.AddHealth(other.GetComponent<Stats>().Nutrition);
                Destroy(other);
                break;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("ground"))
        {
            // Leaving ground is not good for anyone
            Debug.Log("Exit ground " + this.name);
            Destroy(gameObject);
        }
    }

    public void OnDestroy()
    {
        FarmerController.Instance.OnDie(this.gameObject);
    }
}
