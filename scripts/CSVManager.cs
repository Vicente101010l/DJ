using UnityEngine;
using System.IO;
using System;
using System.Collections;

public class CSVManager : MonoBehaviour
{
    public static CSVManager Instance;

    [SerializeField] private string numeroTeste = "1";
    private string desktopPath;
    private string player1Path;
    private string player2Path;
    
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Inicialização imediata
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        try
        {
            // Pasta Desktop
            desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            
            // Pasta para logs
            string logsFolder = Path.Combine(desktopPath, "GameLogs");
            Directory.CreateDirectory(logsFolder);
            
            // Caminhos dos arquivos
            player1Path = Path.Combine(logsFolder, "Teste_Player1.csv");
            player2Path = Path.Combine(logsFolder, "Teste_Player2.csv");

            // Cria arquivos com cabeçalhos
            CreateFile(player1Path);
            CreateFile(player2Path);

            isInitialized = true;
            
            Debug.Log($"<color=green>CSVManager INICIALIZADO (Teste: {numeroTeste})</color>");
            Debug.Log($"Player1: {player1Path}");
            Debug.Log($"Player2: {player2Path}");
            
            // Log inicial do sistema
            LogSystemStart();
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao inicializar CSVManager: {e.Message}");
        }
    }

    void CreateFile(string path)
    {
        try
        {
            bool needsHeader = true;
            
            if (File.Exists(path))
            {
                string[] existingLines = File.ReadAllLines(path);
                if (existingLines.Length > 0 && existingLines[0] == "nr_test,player_id,statevars,datetime,type,typeof,payload")
                {
                    needsHeader = false;
                    Debug.Log($"Arquivo {Path.GetFileName(path)} já existe com cabeçalho correto");
                }
                else
                {
                    Debug.LogWarning($"Arquivo {Path.GetFileName(path)} existe mas não tem cabeçalho correto. Recriando...");
                }
            }
            
            if (needsHeader)
            {
                File.WriteAllText(path, "nr_test,player_id,statevars,datetime,type,typeof,payload\n");
                Debug.Log($"Arquivo criado: {Path.GetFileName(path)}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao criar arquivo: {e.Message}");
        }
    }

    void LogSystemStart()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        
        try
        {
            string systemLine1 = $"{numeroTeste},System,test_start={numeroTeste},{timestamp},system,start,player=1";
            File.AppendAllText(player1Path, systemLine1 + "\n");
            
            string systemLine2 = $"{numeroTeste},System,test_start={numeroTeste},{timestamp},system,start,player=2";
            File.AppendAllText(player2Path, systemLine2 + "\n");
            
            Debug.Log($"<color=cyan>[SYSTEM] Teste {numeroTeste} iniciado às {timestamp}</color>");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao logar início do sistema: {e.Message}");
        }
    }

    public void LogLine(string playerID, string line)
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"CSVManager não inicializado para {playerID}. Inicializando...");
            Initialize();
            
            if (!isInitialized)
            {
                Debug.LogError($"Não foi possível inicializar CSVManager para {playerID}!");
                return;
            }
        }

        try
        {
            string path = playerID == "Player1" ? player1Path : player2Path;
            
            if (string.IsNullOrEmpty(numeroTeste))
            {
                numeroTeste = "1";
                Debug.LogWarning($"Número do teste estava vazio, definido para: {numeroTeste}");
            }
            
            string fullLine = $"{numeroTeste},{line}";
            
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Arquivo {path} não existe. Criando...");
                CreateFile(path);
            }
            
            File.AppendAllText(path, fullLine + "\n");
            
            string color = playerID == "Player1" ? "cyan" : "yellow";
            Debug.Log($"<color={color}>[TESTE {numeroTeste} - {playerID}]</color> {line}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao escrever log para {playerID}: {e.Message}");
        }
    }

    // 🔹 MÉTODO PARA DEFINIR O NÚMERO DO TESTE
    public void SetTestNumber(string newTestNumber)
    {
        if (newTestNumber != numeroTeste)
        {
            Debug.Log($"<color=orange>Número do teste alterado de {numeroTeste} para {newTestNumber}</color>");
            numeroTeste = newTestNumber;
            
            LogTestNumberChange();
        }
    }

    public void SetTestNumber(int newTestNumber)
    {
        SetTestNumber(newTestNumber.ToString());
    }

    void LogTestNumberChange()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        
        try
        {
            string changeLine = $"{numeroTeste},System,test_change={numeroTeste},{timestamp},system,change";
            File.AppendAllText(player1Path, changeLine + "\n");
            File.AppendAllText(player2Path, changeLine + "\n");
            
            Debug.Log($"<color=yellow>[SYSTEM] Teste alterado para {numeroTeste}</color>");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao logar mudança de teste: {e.Message}");
        }
    }

    // 🔹 MÉTODO GETCurrentTestNumber QUE ESTAVA FALTANDO
    public string GetCurrentTestNumber()
    {
        return numeroTeste;
    }

    public bool IsReady()
    {
        return isInitialized;
    }

    // 🔹 MÉTODO PARA VERIFICAR O ÚLTIMO LOG
    public void CheckLastLog(string playerID)
    {
        try
        {
            string path = playerID == "Player1" ? player1Path : player2Path;
            
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length > 1)
                {
                    string lastLine = lines[lines.Length - 1];
                    Debug.Log($"<color=white>Último log de {playerID}: {lastLine}</color>");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao verificar último log: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        
        try
        {
            string endLine = $"{numeroTeste},System,test_end={numeroTeste},{timestamp},system,end";
            File.AppendAllText(player1Path, endLine + "\n");
            File.AppendAllText(player2Path, endLine + "\n");
            
            Debug.Log($"<color=red>[SYSTEM] Teste {numeroTeste} finalizado às {timestamp}</color>");
            
            PrintFileStats();
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao logar fim do teste: {e.Message}");
        }
    }

    public void PrintFileStats()
    {
        if (!isInitialized) return;
        
        try
        {
            Debug.Log("=== ESTATÍSTICAS DOS ARQUIVOS CSV ===");
            
            string[] files = { player1Path, player2Path };
            string[] names = { "Player1", "Player2" };
            
            for (int i = 0; i < files.Length; i++)
            {
                if (File.Exists(files[i]))
                {
                    string[] lines = File.ReadAllLines(files[i]);
                    int dataLines = lines.Length - 1;
                    
                    Debug.Log($"{names[i]}: {dataLines} linhas de dados");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao ler estatísticas: {e.Message}");
        }
    }
}