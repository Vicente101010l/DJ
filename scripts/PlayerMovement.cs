using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Vector2 movement;

    [SerializeField] private Transform ghost;

    

    // 🔹 DISTÂNCIA
    private Vector3 lastPosition;
    private float distanceAccumulator = 0f;
    
    // 🔹 ID do jogador
    private string playerID = "";
    private bool isInitialized = false;
    private bool hasLoggedGameStart = false;
    
    // 🔹 CONTADOR DE COLISÕES
    private int obstacleCollisionCount = 0;
    
    // 🔹 CHECKPOINTS - GARANTIR APENAS UMA LOG POR CHECKPOINT
    private bool checkpoint1Reached = false;
    private bool checkpoint2Reached = false;
    private DateTime gameStartTime;
    private DateTime checkpoint1Time;
    private DateTime checkpoint2Time;

    void Start()
    {
        // Inicializa componentes básicos (todos os jogadores)
        rb = GetComponent<Rigidbody2D>();
        
        Transform spriteTransform = transform.Find("Sprite");
        if (spriteTransform != null)
        {
            anim = spriteTransform.GetComponent<Animator>();
            sr = spriteTransform.GetComponent<SpriteRenderer>();
        }

        // Apenas o owner continua
        if (!IsOwner) return;

        // Determina o ID do jogador
        playerID = IsHost ? "Player1" : "Player2";
        
        Debug.Log($"<color={(IsHost ? "green" : "yellow")}>[{playerID}] Iniciando...</color>");

        // Configura posição inicial
        SetupInitialPosition();
        
        // Inicializa tracking
        lastPosition = transform.position;
        isInitialized = true;

        // Marca início do jogo
        gameStartTime = DateTime.Now;

        // Inicia logging
        StartCoroutine(StartLogging());
    }

    void SetupInitialPosition()
    {
        GameObject p1Spawn = GameObject.Find("PlayerSpawn1");
        GameObject p2Spawn = GameObject.Find("PlayerSpawn2");

        if (p1Spawn != null && p2Spawn != null)
        {
            if (IsHost)
            {
                transform.position = p1Spawn.transform.position;
                Debug.Log($"[Player1] Posicionado em {transform.position}");
            }
            else
            {
                transform.position = p2Spawn.transform.position;
                Debug.Log($"[Player2] Posicionado em {transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("Spawn points não encontrados!");
        }
    }

    IEnumerator StartLogging()
    {
        // Aguarda o CSVManager estar pronto
        while (CSVManager.Instance == null || !CSVManager.Instance.IsReady())
        {
            Debug.Log($"[{playerID}] Aguardando CSVManager...");
            yield return new WaitForSeconds(0.5f);
        }

        // Pequeno delay extra
        yield return new WaitForSeconds(0.5f);

        // 🔹 LOG DE INÍCIO DE JOGO (game_start)
        LogGameStart();

        // 🔹 LOG DE SPAWN (player_joined)
        LogSpawn();

        // Inicia logging periódico
        InvokeRepeating(nameof(LogMovement), 1f, 1f);

        Debug.Log($"[{playerID}] Logging iniciado!");
    }

    void LogGameStart()
    {
        if (hasLoggedGameStart) return;
        
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "game";
        string typeofAction = "game_start";
        
        string currentTest = "1";
        if (CSVManager.Instance != null)
        {
            currentTest = CSVManager.Instance.GetCurrentTestNumber();
        }
        
        string payload = $"player={playerID},host={IsHost},clientId={OwnerClientId},test_number={currentTest}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
        hasLoggedGameStart = true;
        
        Debug.Log($"<color=green>[{playerID}] Jogo iniciado! Teste: {currentTest}</color>");
    }

    void LogSpawn()
    {
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "player";
        string typeofAction = "player_joined";
        string payload = $"posX={transform.position.x:F2},posY={transform.position.y:F2},host={IsHost},clientId={OwnerClientId}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
    }

    void Update()
    {
        if (!IsOwner || !isInitialized) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (anim != null)
        {
            anim.SetInteger("horizontal", (int)movement.x);
            anim.SetInteger("vertical", (int)movement.y);
            anim.SetBool("isMoving", movement.magnitude > 0);

            if (movement.x != 0)
                sr.flipX = movement.x < 0;
        }

        // 🔹 LOG DE AÇÕES
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LogAction("jump", "pressed_space");
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            LogAction("interact", "pressed_e");
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            LogAction("reload", "pressed_r");
        }
        
        // 🔹 DEBUG: Mudar número do teste
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (CSVManager.Instance != null)
            {
                CSVManager.Instance.SetTestNumber(1);
                Debug.Log($"Teste alterado para: {CSVManager.Instance.GetCurrentTestNumber()}");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (CSVManager.Instance != null)
            {
                CSVManager.Instance.SetTestNumber(2);
                Debug.Log($"Teste alterado para: {CSVManager.Instance.GetCurrentTestNumber()}");
            }
        }
        
        // 🔹 DEBUG: Forçar checkpoint (para testes)
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log($"[{playerID}] Forçando checkpoint1 (tecla C)");
            LogCheckpoint("Manual_Checkpoint1", 1);
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log($"[{playerID}] Forçando checkpoint2 (tecla V)");
            LogCheckpoint("Manual_Checkpoint2", 2);
        }
    }

    void LogAction(string actionType, string actionDetails)
    {
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "action";
        string typeofAction = actionType;
        string payload = $"action={actionDetails},posX={transform.position.x:F2},posY={transform.position.y:F2}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
        
        Debug.Log($"[{playerID}] Ação: {actionType} - {actionDetails}");
    }

    void FixedUpdate()
    {
        if (!IsOwner || !isInitialized) return;

        // Movimento
        if (rb != null)
        {
            rb.linearVelocity = movement.normalized * speed;
        }

        // Acumula distância
        distanceAccumulator += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
    }

    void LogMovement()
    {
        if (!IsOwner || !isInitialized) return;

        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "movement";
        string typeofAction = "update";
        string payload = $"distance={distanceAccumulator:F2},posX={transform.position.x:F2},posY={transform.position.y:F2},moving={movement.magnitude > 0},velocityX={rb?.linearVelocity.x:F2},velocityY={rb?.linearVelocity.y:F2}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);

        distanceAccumulator = 0f;
    }

    // 🔹 COLISÃO COM OBSTÁCULOS
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner || !isInitialized) return;
        
        // 🔹 LOG DE COLISÃO COM OBSTÁCULO
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            obstacleCollisionCount++;
            LogObstacleCollision(collision);
        }
    }

    void LogObstacleCollision(Collision2D collision)
    {
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "collision";
        string typeofAction = "obstacle_hit";
        
        GameObject obstacle = collision.gameObject;
        Vector2 obstaclePos = obstacle.transform.position;
        string obstacleName = obstacle.name;
        float collisionForce = collision.relativeVelocity.magnitude;
        Vector2 contactPoint = collision.contacts[0].point;
        
        string payload = $"obstacle_name={obstacleName},obstacle_posX={obstaclePos.x:F2},obstacle_posY={obstaclePos.y:F2}," +
                        $"player_posX={transform.position.x:F2},player_posY={transform.position.y:F2}," +
                        $"contactX={contactPoint.x:F2},contactY={contactPoint.y:F2}," +
                        $"force={collisionForce:F2},collision_count={obstacleCollisionCount}," +
                        $"velocityX={rb?.linearVelocity.x:F2},velocityY={rb?.linearVelocity.y:F2}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
        
        Debug.Log($"<color=red>[{playerID}] Colisão com obstáculo: {obstacleName} (Força: {collisionForce:F2})</color>");
    }

    // 🔹 TRIGGER PARA CHECKPOINTS - LOG APENAS UMA VEZ
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOwner || !isInitialized) return;
        
        // 🔹 CHECKPOINT 1 (apenas uma vez)
        if (other.CompareTag("checkpoint1") && !checkpoint1Reached)
        {
            checkpoint1Reached = true;
            checkpoint1Time = DateTime.Now;
            LogCheckpoint(other.gameObject.name, 1);
            
            Debug.Log($"<color=yellow>[{playerID}] CHECKPOINT 1 ALCANÇADO! Tempo: {(int)(checkpoint1Time - gameStartTime).TotalSeconds}s</color>");
        }
        
        // 🔹 CHECKPOINT 2 (FIM DO JOGO - apenas uma vez)
        else if (other.CompareTag("checkpoint2") && !checkpoint2Reached)
        {
            checkpoint2Reached = true;
            checkpoint2Time = DateTime.Now;
            LogCheckpoint(other.gameObject.name, 2);
            
            // 🔹 LOG DE JOGO COMPLETADO
            LogGameCompleted();
            
            Debug.Log($"<color=green>[{playerID}] CHECKPOINT 2 ALCANÇADO! JOGO COMPLETADO!</color>");
            Debug.Log($"<color=green>Tempo total: {(int)(checkpoint2Time - gameStartTime).TotalSeconds}s</color>");
            Debug.Log($"<color=green>Tempo checkpoint1→checkpoint2: {(int)(checkpoint2Time - checkpoint1Time).TotalSeconds}s</color>");
            
            // Opcional: Desativar controles
            // enabled = false;
        }
    }

    // 🔹 LOG DE CHECKPOINT (garante log apenas uma vez)
    void LogCheckpoint(string checkpointName, int checkpointNumber)
    {
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "checkpoint";
        string typeofAction = "checkpoint_reached";
        
        TimeSpan timeToCheckpoint = DateTime.Now - gameStartTime;
        
        string payload = $"checkpoint_name={checkpointName},checkpoint_number={checkpointNumber}," +
                        $"time_to_checkpoint={(int)timeToCheckpoint.TotalSeconds}," +
                        $"player_posX={transform.position.x:F2},player_posY={transform.position.y:F2}," +
                        $"obstacle_collisions={obstacleCollisionCount}," +
                        $"checkpoint1_reached={checkpoint1Reached},checkpoint2_reached={checkpoint2Reached}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
    }

    // 🔹 LOG DE JOGO COMPLETADO
    void LogGameCompleted()
    {
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "game";
        string typeofAction = "game_completed";
        
        TimeSpan totalTime = DateTime.Now - gameStartTime;
        TimeSpan betweenCheckpoints = checkpoint1Reached ? (checkpoint2Time - checkpoint1Time) : TimeSpan.Zero;
        
        string payload = $"player={playerID},total_time={(int)totalTime.TotalSeconds}," +
                        $"time_to_checkpoint1={(checkpoint1Reached ? (int)(checkpoint1Time - gameStartTime).TotalSeconds : 0)}," +
                        $"time_between_checkpoints={(checkpoint1Reached && checkpoint2Reached ? (int)betweenCheckpoints.TotalSeconds : 0)}," +
                        $"total_obstacle_collisions={obstacleCollisionCount}," +
                        $"checkpoint1_reached={checkpoint1Reached},checkpoint2_reached={checkpoint2Reached}," +
                        $"final_posX={transform.position.x:F2},final_posY={transform.position.y:F2}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
    }

    void LogToCSV(string csvLine)
    {
        if (CSVManager.Instance == null)
        {
            Debug.LogError($"[{playerID}] CSVManager.Instance é NULL!");
            return;
        }

        try
        {
            CSVManager.Instance.LogLine(playerID, csvLine);
        }
        catch (Exception e)
        {
            Debug.LogError($"[{playerID}] Erro ao logar: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (IsOwner)
        {
            CancelInvoke(nameof(LogMovement));
            
            // 🔹 LOG RESUMO DE COLISÕES
            LogCollisionSummary();
            
            // 🔹 LOG DE FIM DE JOGO (se não completou)
            if (!checkpoint2Reached)
            {
                LogGameEnd();
            }
            
            // Log de desconexão do jogador
            if (isInitialized)
            {
                LogPlayerDisconnect();
            }
        }
    }

    void LogCollisionSummary()
    {
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "summary";
        string typeofAction = "obstacle_collisions";
        string payload = $"total_collisions={obstacleCollisionCount},player={playerID}," +
                        $"checkpoint1_reached={checkpoint1Reached},checkpoint2_reached={checkpoint2Reached}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
        
        Debug.Log($"<color=yellow>[{playerID}] Resumo: Colisões={obstacleCollisionCount} | CP1={checkpoint1Reached} | CP2={checkpoint2Reached}</color>");
    }

    void LogGameEnd()
    {
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "game";
        string typeofAction = "game_end";
        
        TimeSpan timePlayed = DateTime.Now - gameStartTime;
        
        string payload = $"player={playerID},play_time={(int)timePlayed.TotalSeconds}," +
                        $"obstacle_collisions={obstacleCollisionCount}," +
                        $"checkpoint1_reached={checkpoint1Reached},checkpoint2_reached={checkpoint2Reached}," +
                        $"posX={transform.position.x:F2},posY={transform.position.y:F2}";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
        
        Debug.Log($"<color=red>[{playerID}] Jogo terminado! Tempo: {(int)timePlayed.TotalSeconds}s | Colisões: {obstacleCollisionCount}</color>");
    }

    void LogPlayerDisconnect()
    {
        string datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        string statevars = "level=1";
        string type = "player";
        string typeofAction = "player_left";
        string payload = $"posX={transform.position.x:F2},posY={transform.position.y:F2}," +
                        $"obstacle_collisions={obstacleCollisionCount}," +
                        $"checkpoint1_reached={checkpoint1Reached},checkpoint2_reached={checkpoint2Reached}," +
                        $"reason=disconnect";

        string csvLine = $"{playerID},{statevars},{datetime},{type},{typeofAction},{payload}";
        
        LogToCSV(csvLine);
    }

    void OnGUI()
    {
        if (!IsOwner) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 350));
        
        GUILayout.Label($"<size=16><b>{playerID}</b></size>");
        GUILayout.Space(10);
        
        string currentTest = CSVManager.Instance != null ? CSVManager.Instance.GetCurrentTestNumber() : "N/A";
        GUILayout.Label($"Teste Atual: <color=yellow>{currentTest}</color>");
        GUILayout.Label($"Posição: {transform.position.x:F1}, {transform.position.y:F1}");
        GUILayout.Label($"Velocidade: {rb?.linearVelocity.magnitude:F1} u/s");
        
        // 🔹 STATUS DOS CHECKPOINTS
        GUILayout.Label($"<color={(checkpoint1Reached ? "green" : "orange")}>• Checkpoint 1: {(checkpoint1Reached ? "ALCANÇADO" : "NÃO ALCANÇADO")}</color>");
        if (checkpoint1Reached)
        {
            TimeSpan toCP1 = checkpoint1Time - gameStartTime;
            GUILayout.Label($"  Tempo até CP1: {(int)toCP1.TotalSeconds}s");
        }
        
        GUILayout.Label($"<color={(checkpoint2Reached ? "green" : "orange")}>• Checkpoint 2: {(checkpoint2Reached ? "ALCANÇADO - JOGO COMPLETO!" : "NÃO ALCANÇADO")}</color>");
        if (checkpoint2Reached)
        {
            TimeSpan totalTime = checkpoint2Time - gameStartTime;
            TimeSpan betweenCPs = checkpoint2Time - checkpoint1Time;
            GUILayout.Label($"  Tempo total: {(int)totalTime.TotalSeconds}s");
            GUILayout.Label($"  Tempo CP1→CP2: {(int)betweenCPs.TotalSeconds}s");
        }
        else if (checkpoint1Reached)
        {
            TimeSpan sinceCP1 = DateTime.Now - checkpoint1Time;
            GUILayout.Label($"  Tempo desde CP1: {(int)sinceCP1.TotalSeconds}s");
        }
        
        // 🔹 COLISÕES
        GUILayout.Label($"<color=red>Colisões com obstáculos: {obstacleCollisionCount}</color>");
        
        GUILayout.Space(10);
        
        // 🔹 BOTÕES DE TESTE
        if (GUILayout.Button("Forçar Checkpoint 1 (C)"))
        {
            if (!checkpoint1Reached)
            {
                checkpoint1Reached = true;
                checkpoint1Time = DateTime.Now;
                LogCheckpoint("Forçado_Checkpoint1", 1);
            }
            else
            {
                Debug.Log($"Checkpoint 1 já foi alcançado!");
            }
        }
        
        if (GUILayout.Button("Forçar Checkpoint 2 (V)"))
        {
            if (!checkpoint2Reached)
            {
                checkpoint2Reached = true;
                checkpoint2Time = DateTime.Now;
                LogCheckpoint("Forçado_Checkpoint2", 2);
                LogGameCompleted();
            }
            else
            {
                Debug.Log($"Checkpoint 2 já foi alcançado!");
            }
        }
        
        if (GUILayout.Button("Simular Colisão"))
        {
            obstacleCollisionCount++;
            Debug.Log($"Colisão simulada! Total: {obstacleCollisionCount}");
        }
        
        GUILayout.Space(10);
        
        // 🔹 MUDAR TESTE
        GUILayout.Label("<b>Mudar Teste:</b>");
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Teste 1"))
        {
            CSVManager.Instance?.SetTestNumber(1);
        }
        
        if (GUILayout.Button("Teste 2"))
        {
            CSVManager.Instance?.SetTestNumber(2);
        }
        
        if (GUILayout.Button("Teste 3"))
        {
            CSVManager.Instance?.SetTestNumber(3);
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        GUILayout.Label("<size=10>Teclas: C=Checkpoint1 | V=Checkpoint2 | 1/2/3=Mudar teste</size>");
        
        GUILayout.EndArea();
    }
}