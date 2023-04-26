using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPlace : MonoBehaviour
{
    public Camera cam;
    public GameObject spawnedObjVisual;
    public GameObject[] spawnedObj;
    public LayerMask placableLayer;
    public bool isHolding = false;

    GameObject placedObj = null;
    GameObject heldObj;
    int objSpawn;

    private void Update()
    {
        if (isHolding)
        {
            GetPos();
        }
    }

    public void ObjClicked(int i)
    {
        isHolding = true;
        objSpawn = i;
        CreateVisual();
    }

    public Vector2 GetMousePos()
    {
        return cam.ScreenToWorldPoint(Input.mousePosition);
    }

    public void GetPos()
    {
        Vector2 mousePos = GetMousePos();

        if (heldObj != null) heldObj.transform.position = mousePos;

        bool r = Physics2D.OverlapCircle(GetMousePos(), .6f, placableLayer);

        if (r)
        {
            heldObj.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else heldObj.GetComponent<SpriteRenderer>().color = Color.white;

        if (Input.GetMouseButtonDown(0) && !r)
        {
            Destroy(heldObj);

            spawn(mousePos);

            isHolding = false;
        }
        else if (Input.GetMouseButtonDown(0) && r)
        {
            Destroy(heldObj);
            isHolding = false;
        }
    }

    void CreateVisual()
    {
        heldObj = Instantiate(spawnedObjVisual, transform.position, Quaternion.identity);
    }
    
    void spawn(Vector2 mousePos)
    {
        placedObj = Instantiate(spawnedObj[objSpawn], mousePos, Quaternion.identity);
    }
}
