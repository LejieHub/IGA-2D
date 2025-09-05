using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterAttributeGenerator : MonoBehaviour
{
    public static CharacterAttributeGenerator Instance { get; private set; }

    // 玩家属性
    public PlayerController.Personality personality; // 性格
    public PlayerController.AcademicLevel academicLevel; // 学业水平
    public PlayerController.IQ iq; // 智商
    public PlayerController.FamilyBackground familyBackground; // 家庭背景
    public PlayerController.Hobby hobby; // 兴趣爱好

    [Header("UI Components")]
    public Text attributeDisplayText; // Text
    public LineRenderer radarLineRenderer; // for Radar - Line Renderer

    private float[] attributeValues = new float[5]; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GenerateAndDisplayAttributes()
    {
        GenerateAttributes();
        ConvertToValues();
        DisplayAttributes();
        DisplayRadarChart();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void GenerateAttributes()
    {
        float personalityValue = Random.value;
        float academicValue = Random.value;
        float iqValue = Random.value;
        float familyValue = Random.value;
        float hobbyValue = Random.value;

        personality = personalityValue < 0.5f ? PlayerController.Personality.Introvert : PlayerController.Personality.Extrovert;
        academicLevel = academicValue switch
        {
            < 0.33f => PlayerController.AcademicLevel.Poor,
            < 0.66f => PlayerController.AcademicLevel.Medium,
            _ => PlayerController.AcademicLevel.Good
        };
        iq = iqValue switch
        {
            < 0.33f => PlayerController.IQ.Poor,
            < 0.66f => PlayerController.IQ.Medium,
            _ => PlayerController.IQ.Good
        };
        familyBackground = familyValue < 0.5f ? PlayerController.FamilyBackground.Bad : PlayerController.FamilyBackground.Good;
        hobby = hobbyValue switch
        {
            < 0.33f => PlayerController.Hobby.Game,
            < 0.66f => PlayerController.Hobby.Music,
            _ => PlayerController.Hobby.Sport
        };

        Debug.Log($"生成的属性：性格={personality}, 学业水平={academicLevel},IQ={iq},  家庭背景={familyBackground}, 兴趣爱好={hobby}");
    }

    private void ConvertToValues()
    {
        // 将枚举转换为0-100的数值
        attributeValues[0] = (personality == PlayerController.Personality.Introvert) ? 40f : 60f; // 内向40，外向60
        attributeValues[1] = academicLevel switch
        {
            PlayerController.AcademicLevel.Poor => 30f,
            PlayerController.AcademicLevel.Medium => 50f,
            PlayerController.AcademicLevel.Good => 70f,
            _ => 50f
        };
        attributeValues[2] = iq switch
        {
            PlayerController.IQ.Poor => 30f,
            PlayerController.IQ.Medium => 50f,
            PlayerController.IQ.Good => 70f,
            _ => 50f
        };
        attributeValues[3] = (familyBackground == PlayerController.FamilyBackground.Good) ? 60f : 40f; // 良好60，较差40
        attributeValues[4] = hobby switch
        {
            PlayerController.Hobby.Game => 50f,
            PlayerController.Hobby.Music => 60f,
            PlayerController.Hobby.Sport => 70f,
            _ => 50f
        };
    }

    private void DisplayRadarChart()
    {
        if (radarLineRenderer == null)
        {
            Debug.LogWarning("未分配Line Renderer组件！");
            return;
        }

        radarLineRenderer.positionCount = 6;
        float angleStep = 2 * Mathf.PI / 5;
        Vector3[] points = new Vector3[6];

        for (int i = 0; i < 5; i++)
        {
            float angle = i * angleStep;
            float radius = attributeValues[i] / 100f * 5f; // radar max radius
            points[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }
        points[5] = points[0]; 

        radarLineRenderer.SetPositions(points);
    }
    
    private void DisplayAttributes()
    {
        if (attributeDisplayText != null)
        {
            attributeDisplayText.text = $"角色属性：\n" +
                                        $"性格：{(personality == PlayerController.Personality.Introvert ? "内向" : "外向")}\n" +
                                        $"学业水平：{(academicLevel == PlayerController.AcademicLevel.Good ? "优秀" : academicLevel == PlayerController.AcademicLevel.Medium ? "中等" : "较差")}\n" +
                                        $"智商：{(iq == PlayerController.IQ.Good ? "高" : iq == PlayerController.IQ.Medium ? "中等" : "低")}\n" +
                                        $"家庭背景：{(familyBackground == PlayerController.FamilyBackground.Good ? "良好" : "较差")}\n" +
                                        $"兴趣爱好：{(hobby == PlayerController.Hobby.Game ? "游戏" : hobby == PlayerController.Hobby.Music ? "音乐" : "运动")}";
        }
        else
        {
            Debug.LogWarning("未分配属性显示的Text组件！");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.personality = personality;
            playerController.academicLevel = academicLevel;
            playerController.iq = iq;
            playerController.familyBackground = familyBackground;
            playerController.hobby = hobby;
            Debug.Log("属性已成功赋值给PlayerController");
        }
        else
        {
            Debug.LogWarning("未在场景中找到PlayerController！");
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 可选：提供Chart.js风格的配置（仅供参考，不直接渲染）
    public void ShowChartConfig()
    {
        string chartConfig = @"{
            ""type"": ""radar"",
            ""data"": {
                ""labels"": [""Personality"", ""Academic"", ""IQ"", ""Family"", ""Hobby""],
                ""datasets"": [{
                    ""label"": ""Player Attributes"",
                    ""data"": [" + attributeValues[0] + ", " + attributeValues[1] + ", " + attributeValues[2] + ", " + attributeValues[3] + ", " + attributeValues[4] + @"],
                    ""backgroundColor"": ""rgba(54, 162, 235, 0.2)"",
                    ""borderColor"": ""rgba(54, 162, 235, 1)"",
                    ""borderWidth"": 2
                }]
            },
            ""options"": {
                ""scales"": {
                    ""r"": {
                        ""beginAtZero"": true,
                        ""min"": 0,
                        ""max"": 100
                    }
                }
            }
        }";
        Debug.Log("Chart Config: " + chartConfig);
        if (attributeDisplayText != null)
        {
            attributeDisplayText.text = "雷达图数据配置已生成（见控制台）";
        }
    }
}