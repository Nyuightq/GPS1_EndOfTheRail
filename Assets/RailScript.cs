// --------------------------------------------------------------
// Creation Date: 2025-09-30 00:42
// Author: User
// Description: -
// --------------------------------------------------------------
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class RailScript : MonoBehaviour
{

    [SerializeField] private LayerMask railLayer; 
    [SerializeField] private Sprite[] slices;

    private int adjacencyMapIndex;
    private Sprite railSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //immediately change the rail once placed 
        changeRail(new HashSet<RailScript>());
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnDestroy()
    {
        changeRail(new HashSet<RailScript>());
    }

    //get the adjacent rails
    private List<RailScript> getAdjacent()
    {
        Vector2[] directions = new Vector2[]{Vector2.left, Vector2.right, Vector2.up, Vector2.down};
        List<RailScript> adjacentList = new List<RailScript>();

        //loop through every direction and add adjacent rails into the script
        foreach(Vector2 direction in directions)
        {
            Vector2 point = (Vector2)gameObject.transform.position + direction;
            Collider2D railCol = Physics2D.OverlapPoint(point, railLayer);

            if (railCol != null)
            {
                RailScript adjacentRailScript = railCol.GetComponent<RailScript>();
                adjacentList.Add(adjacentRailScript);
            }
        }
        return adjacentList;
    }

    //changes the rails sprite based on adjacent rails
    private void changeRail(HashSet<RailScript> visitedRails)
    {
        //prevents infinite recursion of the function
        if (visitedRails.Contains(this)) return; else visitedRails.Add(this);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        List<RailScript> adjacentRails = getAdjacent();

        int right = 0, left = 0, up = 0, down = 0;

        //get which side has adjacent rails
        foreach(RailScript rail in adjacentRails)
        {
            Vector2 direction = (Vector2)rail.transform.position - (Vector2)gameObject.transform.position;
            if (direction.x == 1) right = 1;
            if (direction.x == -1) left = 1;
            if (direction.y == 1) up = 1;
            if (direction.y == -1) down = 1;
        }

        //curves would have 1 vertical and 1 horizontal
        bool curve = (right + left == 1) && (up + down == 1);

        //force straight rails if connected both sides horizontally || vertically
        if (!curve)
        {
            if (right + left >= 2)
            {
                //ensures a curve doesnt happen
                up = 0;
                down = 0;
            }
            else if (up + down >= 2)
            {
                //ensures a curve doesnt happen
                right = 0;
                left = 0;
            }
        }

        //calculate the map index which determines the sprite of the rail
        adjacencyMapIndex = right + (left * 2) + (up * 4) + (down * 8);

        //combines rail and the mapindex to find the correct sprite
        string spriteName = $"rail_{adjacencyMapIndex}";

        foreach (var slice in slices)
        {
            if (slice.name == spriteName)
            {
                railSprite = slice;
            }
        }

        //if the chosen railsrpite is valid
        if (sr != null && railSprite != null)
        {
            sr.sprite = railSprite;
            
            //calls changeRail again for every adjacent rail 
            foreach(RailScript rail in adjacentRails)
            {
                rail.changeRail(visitedRails);
            }
        }
        else
        {
            return;
        }
    }
}
