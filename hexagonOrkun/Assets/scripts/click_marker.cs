using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class click_marker : MonoBehaviour
{
    bool have_children;

    private void OnEnable()
    {
        have_children = false;
        StartCoroutine(wait_children());
    }

    private void OnDisable()
    {
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    IEnumerator wait_children()
    {
        yield return new WaitUntil(() => have_children);
        Vector3 middle_point = new Vector3(0,0,0);
        for (int i = 1; i < 4; i++)
        {
            middle_point += transform.GetChild(i).transform.position;
        }
        Debug.Log(middle_point / 3);
        transform.position = middle_point / 3;
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    void Update()
    {
        if (transform.childCount == 4)
        {
            have_children = true;
        }
    }

    public IEnumerator explode()
    {
        yield return new WaitForFixedUpdate();
        int count = transform.childCount;
        for (int i = 1; i < count; i++)
        {
            if(transform.GetChild(i).GetComponent<tile_assentials>().colliding_objects.Count != 0)
            {
                transform.GetChild(i).GetComponent<tile_assentials>().check_neighbors(true);
            }
        }
        yield return new WaitForFixedUpdate();
        if (count == transform.childCount)
        {
            for (int i = 1; i < count; i++)
            {
                if (transform.GetChild(i).GetComponent<tile_assentials>().colliding_objects.Count != 0)
                {
                    transform.GetChild(i).GetComponent<tile_assentials>().colliding_objects.Clear();
                    transform.GetChild(i).GetComponent<tile_assentials>().destroy_tile();
                    manager._manager.destroyed_tiles.Add(transform.GetChild(i).gameObject);
                }
            }
            yield return new WaitForSeconds(0.05f);
            UI_manager._uı_manager.update_score(manager._manager.destroyed_tiles.Count);
            manager._manager.update_map_2();
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            StartCoroutine(explode());
        }
    }
}
