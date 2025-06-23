using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeMatchState : IGameplaySubstate
{
    public void Enter()
    {
        Debug.LogError("here");
        InitializePlayer();
        
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.StartRoomSequence();
        }
        else
        {
            Debug.LogError("RoomManager instance is missing!");
        }

        GameplayStateMachine.Instance.ChangeState(new EnemySpawnState());
    }

    private void InitializePlayer()
    {
        if (GameStateManager.Instance.playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }

        if (GameStateManager.Instance.playerSpawnPoint == null)
        {
            Debug.LogError("Player spawn point is not assigned!");
            return;
        }

        if (GameStateManager.Instance.rockPaperScissorsGame.playerInstance == null)
        {
            var newPlayer = GameObject.Instantiate(GameStateManager.Instance.playerPrefab, GameStateManager.Instance.playerSpawnPoint.position, GameStateManager.Instance.playerSpawnPoint.rotation);
            GameStateManager.Instance.rockPaperScissorsGame.UpdatePlayerReference(newPlayer);
            Debug.Log("Player initialized at spawn point.");

            GameStateManager.Instance.rockPaperScissorsGame.playerInstance.OnDeathAnimationFinished += GameStateManager.Instance.HandlePlayerDeathAnimationFinished;
        }
        else
        {
            Debug.LogWarning("Player already exists. Skipping instantiation.");
        }
    }

    //private void WaitForRoomAndInitializeGame()
    //{
    //    Debug.Log("Room and enemy are ready. Initializing game...");
    //    var currentEnemyInstance = RoomManager.Instance.GetCurrentEnemy();
    //    rockPaperScissorsGame.InitializeGame(playerInstance, currentEnemyInstance);
    //    rockPaperScissorsGame.StartGame();
    //}

    public void Exit()
    {
    
    }

    public void Update()
    {
        // No per-frame behavior needed
    }
}
