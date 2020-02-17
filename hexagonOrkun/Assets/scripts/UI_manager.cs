using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_manager : MonoBehaviour
{
    [SerializeField]
    private Text score_text;
    [SerializeField]
    private Text moves_text;
    public int score;
    public int moves;

    public static UI_manager _uı_manager;

    private void Awake()//create the singleton
    {
        if (UI_manager._uı_manager == null )
        {
            UI_manager._uı_manager = this;
        }
    }

    void Start()
    {
        score = 0;
        score_text.text = score.ToString();
        moves = 0;
        moves_text.text = moves.ToString();
    }

    public void update_score(int gained_score)
    {
        score += gained_score * 5;
        score_text.text = score.ToString();
        moves++;
        moves_text.text = moves.ToString();
    }

}
