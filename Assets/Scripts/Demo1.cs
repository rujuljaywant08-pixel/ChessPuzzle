using UnityEngine;

public class Demo1 : MonoBehaviour
{
    int coins = 0;
    bool hasWon = false;
    public int coinsToWin = 10;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddCoin();
            CheckWin();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            for (int i = 0; i < 5; i++)
            {
                AddCoin();
            }
            CheckWin();
        }
    }

    void AddCoin()
    {
        coins++;
        Debug.Log("Number of coins: " + coins);
    }

    void CheckWin()
    {
        if (coins >= coinsToWin && !hasWon)
        {
            Debug.Log("You win");
            hasWon = true;
        }
    }
}