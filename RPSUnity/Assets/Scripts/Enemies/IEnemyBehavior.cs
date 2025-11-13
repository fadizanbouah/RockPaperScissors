using System.Collections;

public interface IEnemyBehavior
{
    void Initialize(HandController enemyHand, float[] configValues);
    IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice);
    IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result);
}