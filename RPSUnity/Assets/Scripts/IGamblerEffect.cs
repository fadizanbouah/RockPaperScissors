using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGamblerEffect
{
    void SetBetAmount(int amount);
    int GetCurrentBet();
    int GetMaxBet();
    int GetBonusDamage();
    int GetSnappedBetAmount(int rawAmount);
}
