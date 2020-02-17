using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tile_assentials : MonoBehaviour
{
    SpriteRenderer SR;
    PolygonCollider2D PC;
    ParticleSystem PS;
    ParticleSystem.MainModule PSM;
    public List<GameObject> colliding_objects = new List<GameObject>();
    public bool destroyed;
    public int color_numb;//we will check for the same color by using this variable
    public bool going_down = false;
    public float target_pos;
    private float speed;
    public float time = 0;
    private int bomb_time;
    [SerializeField]
    private int color_count = 5;//can change it from the editor it only goes up to 7 tho
    public bool isbomb;
    [SerializeField]
    private Sprite normal_tile_sprite;

    void Start()
    {
        SR = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        PC = gameObject.transform.GetComponent<PolygonCollider2D>();
        PS = gameObject.GetComponent<ParticleSystem>();
        PSM = PS.main;
        PS.Stop();
        speed = 5f;
        going_down = false;
        isbomb = false;
        change_color(color_count);
    }

    private void change_color(int colornumb)
    {
        color_numb = Random.Range(0, colornumb+1);
        if (color_numb == 0)
        {
            SR.color = Color.red;
            PSM.startColor = Color.red;
        }
        else if (color_numb == 1)
        {
            SR.color = Color.yellow;
            PSM.startColor = Color.yellow;
        }
        else if (color_numb == 2)
        {
            SR.color = Color.green;
            PSM.startColor = Color.green;
        }
        else if (color_numb == 3)
        {
            SR.color = Color.blue;
            PSM.startColor = Color.blue;
        }
        else if (color_numb == 4)
        {
            SR.color = Color.magenta;
            PSM.startColor = Color.magenta;
        }
        else if (color_numb == 5)
        {
            SR.color = new Color32(255, 139, 11, 255);//orange
            Color color = new Color32(255, 139, 11, 255);//orange
            PSM.startColor = color;
        }
        else if (color_numb == 6)
        {
            SR.color = Color.gray;
            PSM.startColor = Color.gray;
        }
        else if (color_numb == 7)
        {
            SR.color = Color.black;
            PSM.startColor = Color.black;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 2 && collision.gameObject.transform.childCount <= 3)
        {
            gameObject.transform.parent = collision.gameObject.transform;
            SR.sortingOrder = 1;
            collision.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            PC.enabled = false;
        }
        else if(collision.gameObject.transform.parent == null && collision.gameObject.tag != "no")
        {
            if (!colliding_objects.Contains(collision.gameObject))
            {
                colliding_objects.Add(collision.gameObject);
            }
        }

    }


    public int check_neighbors(bool recruiting)
    {
        int x = 0;
        for (int i = 0; i < colliding_objects.Count; i++)
        {
            if (colliding_objects[i].GetComponent<tile_assentials>().color_numb == color_numb)
            {
                x++;
                if (recruiting)
                {
                    colliding_objects[i].transform.parent = transform.parent;
                }
            }
        }
        return x;
    }

    public void update_pos()
    {
        float y = 0;
        for (int c = 0; c < 9; c++)
        {
            for (int r = 0; r < 8; r++)
            {
                if (r % 2 == 0)
                {
                    y = 1;
                }
                else
                {
                    y = .62f;
                }
                if (Mathf.Abs(transform.position.y - (c * .75f - y)) < .1f && Mathf.Abs(transform.position.x - (r * .66f)) < .1f)
                {
                    transform.position = new Vector3(r * .66f, c * .75f - y, 0);
                    transform.name = c.ToString() + r.ToString();
                    transform.eulerAngles = new Vector3(0,0,0);
                    manager._manager.map_tiles[c, r] = gameObject;
                }


            }
        }
    }

    public void destroy_tile()//not really destroys it just makes it seem as it is
    {
        PS.Play();
        StartCoroutine(waitforps());
        destroyed = true;
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        manager._manager.map_tiles[int.Parse(gameObject.name[0].ToString()), int.Parse(gameObject.name[1].ToString())] = null;//necesarry but check it again
        if (isbomb)
        {
            manager._manager.have_bomb = false;
            SR.sprite = normal_tile_sprite;
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(1).transform.parent = null;
            isbomb = false;
        }
    }

    IEnumerator waitforps()
    {
        yield return new WaitForSeconds(1);
        PS.Stop();
    }

    public void fall_down(int c,int r)//will teleport the tile to the correct column and way up and will make it fall to its place (outside of screen)
    {
        change_color(color_count);
        transform.position = new Vector3(r * .66f, 2 + 1*c, 0);
        SR.enabled = true;
        float y = 0;
        if (r % 2 == 0)
        {
            y = 1;
        }
        else
        {
            y = .62f;
        }
        target_pos = c * .75f - y;
        manager._manager.map_tiles[c, r] = gameObject;
        gameObject.name = c.ToString() + r.ToString();
        going_down = true;
    }


    public IEnumerator check_down(float wait_time)
    {
        
        Debug.Log(wait_time);
        yield return new WaitForSeconds(wait_time);
        Debug.Log("checking");
        Debug.Log(int.Parse(gameObject.name[0].ToString()) - 1);

        bool should_go_down = false;
        int go_down_count = 0;
        while (int.Parse(gameObject.name[0].ToString()) != 0 && manager._manager.map_tiles[int.Parse(gameObject.name[0].ToString()) - 1, int.Parse(gameObject.name[1].ToString())] == null)
        {
            manager._manager.map_tiles[int.Parse(gameObject.name[0].ToString()), int.Parse(gameObject.name[1].ToString())] = null;
            manager._manager.map_tiles[int.Parse(gameObject.name[0].ToString()) - 1, int.Parse(gameObject.name[1].ToString())] = gameObject;
            gameObject.name = (int.Parse(gameObject.name[0].ToString()) - 1).ToString() + gameObject.name[1].ToString();
            should_go_down = true;
            go_down_count++;
        }
        if (should_go_down)
        {
            target_pos = transform.position.y - go_down_count*.75f;
            going_down = true;
        }
        else
        {
            going_down = false;

        }       
    }

    public void change_to_bomb(Sprite bomb_sprite)
    {
        isbomb = true;
        SR.sprite = bomb_sprite;
        bomb_time = Random.Range(4, 8);
        SR.sortingOrder = 0;
        bomb_timer();
    }


    public void bomb_timer()
    {
        bomb_time--;
        transform.GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = bomb_time.ToString();
        if (bomb_time == 0)
        {
            StartCoroutine(manager._manager.game_over());
            transform.GetChild(1).gameObject.SetActive(false);
        }
    }


    

    private void Update()
    {
        if (going_down)
        {
            transform.position = transform.position + new Vector3(0, -1, 0) * Time.deltaTime * speed;
            if (transform.position.y <= target_pos)
            {
                transform.position = new Vector3(transform.position.x, target_pos, 0);
                going_down = false;
                update_pos();
            }
        }
    }
}
