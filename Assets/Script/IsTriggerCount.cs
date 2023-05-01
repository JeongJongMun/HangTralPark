using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsTriggerCount : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && Timer.GetFinishedFlag() == false)
        {
            PanaltyCount.AddPanalty(1); //노란타일이 플레이어에게 닿으면 패널티 +1
        }
    }
}
