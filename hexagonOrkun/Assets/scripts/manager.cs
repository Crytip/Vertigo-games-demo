using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour
{

    public static manager _manager;


    [SerializeField]
    private GameObject click_texture;
    private GameObject _used_click_txt;//will get disabled after a succesfull turn
    [SerializeField]
    private GameObject normal_tile;
    public GameObject[,] map_tiles = new GameObject[9,8];
    public List<GameObject> destroyed_tiles = new List<GameObject>();
    [SerializeField]
    private Sprite bomb_sprite;
    [SerializeField]
    private GameObject bomb_canvas;

    private Touch current_touch = new Touch();

    private bool rotate_right;
    private bool rotate_left;
    private bool rotating;
    private List<Vector3> points = new List<Vector3>();
    private bool acceptable;
    private bool sideways;
    private float rotation_max;
    private Vector3 rotation;
    private Vector3 start_angle;
    [SerializeField]
    private int speed = 350;
    public bool have_bomb = false;
    private GameObject current_bomb_tile;
    private int bomb_appear_score;

    private void Awake()//create the singleton for the manager
    {
        if (manager._manager == null)
        {
            manager._manager = this;
        }
    }

    void Start()
    {
        Application.targetFrameRate = 30;
        _used_click_txt = Instantiate(click_texture);
        _used_click_txt.SetActive(false);

        rotate_right = false;
        rotate_left = false;

        bomb_appear_score = 1000;

        Create_tile();
    }

    private void Create_tile()
    {
        float y;
        for (int c = 0; c < 9; c++)
        {
            for (int r = 0; r < 8; r++)
            {
                GameObject obj = Instantiate(normal_tile);
                if (r%2 == 0)
                {
                    y = 1;
                }
                else
                {
                    y = .62f;
                }
                obj.transform.position = new Vector3(r*.66f,c*.75f-y,0);
                obj.name = c.ToString() + r.ToString();
                map_tiles[c, r] = obj;
            }
        }

    }

    public void onClick_choose()
    {
        if (current_touch.fingerId != 500)//checking if the click event does not get called before the down event just to be sure
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.touches[i].phase == TouchPhase.Ended)
                {
                    if (Vector2.Distance(current_touch.position, Input.touches[i].position) < 15)
                    {
                        current_touch = Input.touches[i];

                        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(current_touch.position), Vector2.zero);
                        if (hit.collider != null)
                        {
                            get_clicked_middle(hit.point,hit.transform.gameObject);
                            
                        }
                    }
                    else
                    {
                        if (_used_click_txt.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled == true)
                        {
                            rotate(current_touch.position - Input.touches[i].position);
                        }
                        
                    }
                }
            }  
        }
        current_touch.fingerId = 500;
    }

    public void onClick_down()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.touches[i].phase == TouchPhase.Began)
            {
                current_touch = Input.touches[i];
                
            }
        }
    }

    //this will help us get the clicked middle point, instead of adding colliders to the middle points doing it this way will
    //help us to change the tile rearangment easier since we will not be dealing with rearanging the collider objects
    void get_clicked_middle(Vector3 point,GameObject clicked_tile)
    {
        if (!rotating)
        {
            acceptable = true;
            sideways = false;

            empty_childs();
            _used_click_txt.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
            _used_click_txt.GetComponent<BoxCollider2D>().enabled = true;
            points.Clear();
            for (int i = 0; i < 6; i++)
            {
                
                if (i == 0)
                {      
                    points.Add(clicked_tile.transform.position + new Vector3(.23f, .37f, 0));
                }
                else if (i == 1)
                {
                    points.Add(clicked_tile.transform.position + new Vector3(-.23f, .37f, 0));
                }
                else if (i == 2)
                {
                    points.Add(clicked_tile.transform.position + new Vector3(.23f, -.37f, 0));
                }
                else if (i == 3)
                {
                    points.Add(clicked_tile.transform.position + new Vector3(-.23f, -.37f, 0));
                }
                else if (i == 4)
                {
                    points.Add(clicked_tile.transform.position + new Vector3(.43f, 0, 0));
                }
                else if (i == 5)
                {
                    points.Add(clicked_tile.transform.position + new Vector3(-.43f, 0, 0));
                }
            }

            //get the closest corner to the clicked point of the tile
            float distance = 0;
            int index = 0;
            for (int z = 0; z < points.Count; z++)
            {
                if (z == 0)
                {
                    distance = Vector3.Distance(point, points[z]);
                    index = 0;
                }
                else
                {
                    if (Vector3.Distance(point, points[z]) < distance)
                    {
                        distance = Vector3.Distance(point, points[z]);
                        index = z;
                    }
                }
            }

            //check if the clicked place is between the boundaries
            if (clicked_tile.name[0] == '8')
            {
                if (index == 0 || index == 1)
                {
                    acceptable = false;
                }
                if (int.Parse(clicked_tile.name[1].ToString()) % 2 == 1)
                {
                    if (index == 5 || index == 4)
                    {
                        acceptable = false;

                    }
                }
            }
            if (clicked_tile.name[0] == '0')
            {
                if (index == 2 || index == 3)
                {
                    acceptable = false;
                }
                if (int.Parse(clicked_tile.name[1].ToString()) % 2 == 0)
                {
                    if (index == 5 || index == 4)
                    {
                        acceptable = false;
                    }
                }
            }
            if (clicked_tile.name[1] == '0')
            {
                if (index == 5 || index == 3 || index == 1)
                {
                    acceptable = false;
                }
            }
            if (clicked_tile.name[1] == '7')
            {
                if (index == 2 || index == 4 || index == 0)
                {
                    acceptable = false;
                }
            }

            //rotate the clicked texture before enabling
            if (index == 1 || index == 4 || index == 3)
            {
                sideways = true;
            }



            if (acceptable)//enable the texture
            {
                _used_click_txt.transform.position = points[index];
                if (sideways)
                {
                    rotation = new Vector3(0, 0, -180);
                }
                else
                {
                    rotation = new Vector3(0, 0, 0);
                }
                _used_click_txt.transform.eulerAngles = rotation;
                _used_click_txt.SetActive(true);
            }
        }
        
    }

    public void empty_childs()
    {
        if (_used_click_txt.transform.childCount > 1)//updates the old children every time a succesfull click happens
        {
            Debug.Log(_used_click_txt.transform.childCount);
            int child_count = _used_click_txt.transform.childCount;//cannot be in the for loop syntax since it will decrease inside of it
            for (int i = 1; i < child_count; i++)
            {
                
                _used_click_txt.transform.GetChild(1).GetComponent<PolygonCollider2D>().enabled = true;
                _used_click_txt.transform.GetChild(1).transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 0;
                _used_click_txt.transform.GetChild(1).GetComponent<tile_assentials>().update_pos();
                _used_click_txt.transform.GetChild(1).transform.parent = null;
                
                
            }
        }
    }

    void rotate(Vector2 direction)
    {
        
        rotating = true;
        if (direction.x > direction.y)
        {
            
            if (!sideways)
            {
                rotation_max = -120;
            }
            else
            {
                rotation_max = -300;
            }
            
            rotate_right = true;
        }
        else
        {
            if (!sideways)
            {
                rotation_max = 120;
            }
            else
            {
                rotation_max = -60;
            }
            
            
            rotate_left = true;
        }
       
    }

    IEnumerator check_wait(int dir)//if 1 right if 2 left
    {
        bool found = false;
        for (int i = 1; i < 4; i++)
        {
            _used_click_txt.transform.GetChild(i).gameObject.GetComponent<CircleCollider2D>().enabled = true;
        }
        yield return new WaitForSeconds(1f);
        for (int i = 1; i < 4; i++)
        {
            _used_click_txt.transform.GetChild(i).gameObject.GetComponent<CircleCollider2D>().enabled = false;
            if (_used_click_txt.transform.GetChild(i).gameObject.GetComponent<tile_assentials>().check_neighbors(false) >= 2)
            {
                Debug.Log("found neighboors");
                found = true;
                rotating = false;
                
            }
            else
            {
                _used_click_txt.transform.GetChild(i).gameObject.GetComponent<tile_assentials>().colliding_objects.Clear();
            }
        }
        if (!found)
        {
            if (dir == 1)
            {
                if (!sideways)
                {
                    if (rotation_max == -120)
                    {
                        rotation_max = -240;
                        rotate_right = true;
                    }
                    else if (rotation_max == -240)
                    {
                        rotation_max = -360;
                        rotate_right = true;
                    }
                    else
                    {
                        rotating = false;
                    }
                }
                else
                {
                    if (rotation_max == -300)
                    {
                        rotation_max = -420;
                        rotate_right = true;
                    }
                    else if (rotation_max == -420)
                    {
                        rotation_max = -540;
                        rotate_right = true;
                    }
                    else
                    {
                        rotating = false;
                    }
                }

            }
            else
            {
                if (!sideways)
                {
                    if (rotation_max == 120)
                    {
                        rotation_max = 240;
                        rotate_left = true;
                    }
                    else if (rotation_max == 240)
                    {
                        rotation_max = 360;
                        rotate_left = true;
                    }
                    else
                    {
                        rotating = false;
                    }
                }
                else
                {
                    if (rotation_max == -60)
                    {
                        rotation_max = 60;
                        rotate_left = true;
                    }
                    else if (rotation_max == 60)
                    {
                        rotation_max = 180;
                        rotate_left = true;
                    }
                    else
                    {
                        rotating = false;
                    }
                }
            }
        }
        else
        {
            for (int i = 1; i < 4; i++)
            {
                _used_click_txt.transform.GetChild(i).gameObject.GetComponent<tile_assentials>().update_pos();
            }
            StartCoroutine(_used_click_txt.GetComponent<click_marker>().explode());
        }
    }

    private void Update()
    {
        if (rotate_right)
        {
            rotation += new Vector3(0, 0, -1)*Time.deltaTime*speed;
            _used_click_txt.transform.eulerAngles = rotation;

            if (rotation.z <= rotation_max + 1)
            {
                rotate_right = false;
                rotation = new Vector3(0, 0, rotation_max);
                _used_click_txt.transform.eulerAngles = rotation;
                StartCoroutine(check_wait(1));
                
            }
        }
        else if (rotate_left)
        {
            rotation += new Vector3(0, 0, 1) * Time.deltaTime * speed;
            _used_click_txt.transform.eulerAngles = rotation;

            if (rotation.z >= rotation_max - 1)
            {
                rotate_left = false;
                rotation = new Vector3(0,0,rotation_max);
                _used_click_txt.transform.eulerAngles = rotation;
                StartCoroutine(check_wait(2));

            }
        }
    }

    public void update_map_2()//updates the tiles to fall
    {
        bool yes = false;
        for (int c = 0; c < 8; c++)
        {
            for (int r = 0; r < 9; r++)
            {
                if (map_tiles[r, c] == null)
                {
                    yes = true;
                    continue;
                }
                if (yes)
                {
                    Debug.Log("cslling the coroutine");
                    StartCoroutine(map_tiles[r, c].GetComponent<tile_assentials>().check_down((r / 100f)));
                }
                

            }
            yes = false;
        }
        
        StartCoroutine(refresh_tiles());
    }


    IEnumerator refresh_tiles()//this will realocate the destroyed tiles and will refresh the tiles just in case if necesarry
    {
        if (have_bomb)
        {
            current_bomb_tile.GetComponent<tile_assentials>().bomb_timer();
        }
        yield return new WaitForSeconds(0.4f);//it has to wait to be sure that the highest tile was placed in its place (not physically just in the array)
        int index_countdown = 0;
        for (int c = 0; c < 9; c++)
        {
            for (int r = 0; r < 8; r++)
            {
                if (map_tiles[c, r] == null)
                {
                    if (UI_manager._uı_manager.score >= bomb_appear_score && !have_bomb)//change one of the tiles to a bomb tile when necesarry
                    {
                        have_bomb = true;
                        bomb_appear_score *= 2;
                        current_bomb_tile = destroyed_tiles[index_countdown];
                        bomb_canvas.transform.parent = current_bomb_tile.transform;
                        bomb_canvas.transform.localPosition = new Vector3(0,0,0);
                        bomb_canvas.SetActive(true);
                        current_bomb_tile.GetComponent<tile_assentials>().change_to_bomb(bomb_sprite);
                    }
                    destroyed_tiles[index_countdown].GetComponent<ParticleSystem>().Stop();
                    destroyed_tiles[index_countdown].GetComponent<tile_assentials>().fall_down(c, r);
                    
                    index_countdown++;
                }
            }
        }
        StartCoroutine(wait_for_check());
        destroyed_tiles.Clear();
    }

    public IEnumerator game_over()
    {
        yield return new WaitForSeconds(0.5f);
        for (int c = 0; c < 9; c++)
        {
            for (int r = 0; r < 8; r++)
            {
                map_tiles[c, r].GetComponent<tile_assentials>().destroy_tile();   
            }
        }
    }

    public IEnumerator wait_for_check()
    {
        yield return new WaitForSeconds(.1f);
        empty_childs();
        if (!check_possible_moves())
        {
            StartCoroutine(game_over());
        }
    }

    bool check_possible_moves()//honestly this is just brute force
    {
        bool there_are_moves = false;
        
        for (int r = 0; r < 8; r++)//first check for columns
        {
            for (int c = 0; c < 9; c++)
            {
                if (c < 8)
                {
                    if (r <= 1)
                    {
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r].GetComponent<tile_assentials>().color_numb)
                        {
                            if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r + 1].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r + 1].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r + 2].GetComponent<tile_assentials>().color_numb)
                            {
                                there_are_moves = true;
                            }
                        }
                    }
                    else if(r >= 6)
                    {
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r].GetComponent<tile_assentials>().color_numb)
                        {
                            if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r - 1].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r - 1].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r - 2].GetComponent<tile_assentials>().color_numb)
                            {
                                there_are_moves = true;
                            }
                        }
                    }
                    else
                    {
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r].GetComponent<tile_assentials>().color_numb)
                        {
                            if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r + 1].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r - 1].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r - 2].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r + 1].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r + 1].GetComponent<tile_assentials>().color_numb
                                || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r + 2].GetComponent<tile_assentials>().color_numb)
                            {
                                there_are_moves = true;
                            }
                        }
                    }
                    
                }

                if (c <= 6 && r == 0 || c <= 6 && r == 6)
                {
                    if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r +1].GetComponent<tile_assentials>().color_numb)
                    {
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r + 1].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 2, r].GetComponent<tile_assentials>().color_numb)
                        {
                            there_are_moves = true;
                        }
                    }
                }
                else if (c >= 2 && r == 0 || c >= 2 && r == 6)
                {
                    if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r +1].GetComponent<tile_assentials>().color_numb)
                    {
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c - 1, r + 1].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c - 1, r].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c -2, r +1].GetComponent<tile_assentials>().color_numb)
                        {
                            there_are_moves = true;
                        }
                    }
                }


                else if (c >= 2 && r == 2 || c >= 2 && r == 4)
                {
                    if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r + 1].GetComponent<tile_assentials>().color_numb)
                    {
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c - 1, r + 1].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c - 1, r].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c - 2, r + 1].GetComponent<tile_assentials>().color_numb)
                        {
                            there_are_moves = true;
                        }
                    }
                    if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r - 1].GetComponent<tile_assentials>().color_numb)
                    {
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c - 1, r - 1].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c - 1, r].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c - 2, r - 1].GetComponent<tile_assentials>().color_numb)
                        {
                            there_are_moves = true;
                        }
                    }

                }
                if (c <= 6 && r == 2 || c <= 6 && r == 4)
                {
                    if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r + 1].GetComponent<tile_assentials>().color_numb)
                    {
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r + 1].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 2, r].GetComponent<tile_assentials>().color_numb)
                        {
                            there_are_moves = true;
                        }
                    }
                    if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c, r - 1].GetComponent<tile_assentials>().color_numb)
                    {
                        Debug.Log(c);
                        if (map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r - 1].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 1, r].GetComponent<tile_assentials>().color_numb
                            || map_tiles[c, r].GetComponent<tile_assentials>().color_numb == map_tiles[c + 2, r].GetComponent<tile_assentials>().color_numb)
                        {
                            there_are_moves = true;
                        }
                    }
                }
            }
        }
        return there_are_moves;
    }
}
