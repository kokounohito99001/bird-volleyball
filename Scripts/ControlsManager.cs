using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ControlSettings
{
    public List<string> move_left = new List<string> { "a", "ArrowLeft" };
    public List<string> move_right = new List<string> { "d", "ArrowRight" };
    public List<string> low_pass = new List<string> { "Space", "MouseLeft" };
    public List<string> smash = new List<string> { "Shift", "MouseRight" };
    public List<string> reset_round = new List<string> { "r" };
}

[System.Serializable]
public class SensitivitySettings
{
    public float movement_speed = 300f;
    public float jump_force = 400f;
}

[System.Serializable]
public class ControlsConfig
{
    public ControlSettings controls;
    public SensitivitySettings sensitivity;
}

public class ControlsManager : MonoBehaviour
{
    public static ControlsManager Instance { get; private set; }
    
    private ControlsConfig config;
    
    public float MovementSpeed => config.sensitivity.movement_speed;
    public float JumpForce => config.sensitivity.jump_force;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadControls();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void LoadControls()
    {
        string path = "Controls";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        
        if (jsonFile != null)
        {
            config = JsonUtility.FromJson<ControlsConfig>(jsonFile.text);
        }
        else
        {
            config = new ControlsConfig
            {
                controls = new ControlSettings(),
                sensitivity = new SensitivitySettings()
            };
            Debug.LogWarning("Controls config not found, using defaults");
        }
    }
    
    public bool IsMoveLeft()
    {
        foreach (string key in config.controls.move_left)
        {
            if (Input.GetKey(key)) return true;
        }
        return false;
    }
    
    public bool IsMoveRight()
    {
        foreach (string key in config.controls.move_right)
        {
            if (Input.GetKey(key)) return true;
        }
        return false;
    }
    
    public bool IsLowPassPressed()
    {
        foreach (string key in config.controls.low_pass)
        {
            if (key == "MouseLeft" && Input.GetMouseButtonDown(0)) return true;
            if (key == "MouseRight" && Input.GetMouseButtonDown(1)) return true;
            if (Input.GetKeyDown(key)) return true;
        }
        return false;
    }
    
    public bool IsSmashPressed()
    {
        foreach (string key in config.controls.smash)
        {
            if (key == "MouseLeft" && Input.GetMouseButtonDown(0)) return true;
            if (key == "MouseRight" && Input.GetMouseButtonDown(1)) return true;
            if (Input.GetKeyDown(key)) return true;
        }
        return false;
    }
    
    public bool IsResetPressed()
    {
        foreach (string key in config.controls.reset_round)
        {
            if (Input.GetKeyDown(key)) return true;
        }
        return false;
    }
}
