using UnityEngine;

public class Demo : MonoBehaviour
{
    int coins = 0;
    bool hasWon = false;   // remembered between frames

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            coins = coins + 1;
            Debug.Log("Coins: " + coins);
        }

        if (coins >= 10 && hasWon == false)
        {
            Debug.Log("You Win!!!");
            hasWon = true;   // now this block will never run again
        }
    }
}