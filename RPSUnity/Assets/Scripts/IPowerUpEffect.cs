public interface IPowerUpEffect
{
    void Initialize(PowerUp source, HandController player, HandController enemy);
    void OnRoundStart();
    void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result);
    void Cleanup(); // Optional for one-time effects
}
