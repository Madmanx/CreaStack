using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour {

    private static GameManager instance;


    [Header("UI Références")]
    public Text scoreText;
    public GameObject previousScore;

    [Space]
    [Header("Prefabs Références")]
    public GameObject prefabCube;

    [Space]
    [Header("Gameplay Parameters")]
    [Range(0.5f, 100)]
    public float movingSpeed;
    public Vector3 currentOffset;

    public int maxCombo = 9;

    [Header("Color")]
    public float changingColorSpeed = 0.2f;
    public Color[] color = { Color.red, Color.blue, Color.green, Color.magenta };

    //private int i = 0;
    //private int j = 1;
    //private float lerp = 0;

    [Space]
    [Header("Gameplay Utils")]
    public bool lose = false;
    public Transform cameraTarget;
    public GameObject CurrentTopBloc;
    private GameObject CurrentMovingBloc;

    private Vector3 maxLocalScale;

    [Space]
    [Header("Variables")]
    private int score;

    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            score = value;
            UpdateUI();
        }
    }

    private int combo = 0;

    public int Combo
    {
        get
        {
            return combo;
        }

        set
        {
            combo = value;
            if (combo >= maxCombo) combo = maxCombo;
        }
    }

    public static GameManager Instance
    {
        get
        {
            return instance;
        }

        set
        {
            instance = value;
        }
    }

    public void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
        if (color.Length < 2) Debug.LogError("At least 2 color pls");

        CreateBloc();
        maxLocalScale = CurrentTopBloc.transform.localScale;
    }
	
	// Update is called once per frame
	void Update () {
		if(!lose && Input.GetMouseButtonDown(0))
        {
            Destroy(CurrentMovingBloc.GetComponent<PingPongBehavior>());

            // win
            bool isPing = (score % 2 == 0);
            if ((isPing && Vector3.Distance(CurrentMovingBloc.transform.position /*x*/, CurrentTopBloc.transform.position/*x*/) <= (0.1f + CurrentTopBloc.GetComponent<MeshRenderer>().bounds.size.x))
                || (!isPing && Vector3.Distance(CurrentMovingBloc.transform.position/*z*/, CurrentTopBloc.transform.position/*z*/) <= (0.1f + CurrentTopBloc.GetComponent<MeshRenderer>().bounds.size.z)))
            {

                // scaling process
                ProcessCut(isPing);

                // win sound depends on combo
                if (AudioManager.instance != null && AudioManager.instance.sound_C != null)
                    AudioManager.instance.PlayComboSound();

                // Reset for next iteration
                CurrentTopBloc = CurrentMovingBloc;
                Score += 1;
                CreateBloc();
         
            } 
            else // lose
            {
                lose = true;

                // lose sound
                if (AudioManager.instance != null && AudioManager.instance.sound_C != null)
                    AudioManager.instance.PlayDefeatSound();


                // Falling effect
                CurrentMovingBloc.AddComponent<Rigidbody>();

                // Handle Score
                if (score > PlayerPrefs.GetFloat("Player Score"))
                    PlayerPrefs.SetFloat("Player Score", score);
                else
                {
                    previousScore.GetComponentInChildren<Text>().text = "Previous Best Score: " + PlayerPrefs.GetFloat("Player Score");
                    previousScore.gameObject.SetActive(true);

                    StartCoroutine(ReloadGame());
                }
            }

        }

        if (Input.GetKeyDown(KeyCode.R))
        {

            StartCoroutine(ReloadGame());
        }
	}

    public void ProcessCut(bool isPing)
    {
        float currentMovingBlocValue = 0;
        float currentTopBlocValue = 0;
        float bounds = 0;
        if (isPing)
        {
            currentMovingBlocValue = CurrentMovingBloc.transform.position.x;
            currentTopBlocValue = CurrentTopBloc.transform.position.x;
            bounds = CurrentTopBloc.GetComponent<MeshRenderer>().bounds.size.x;
        }
        else
        {
            currentMovingBlocValue = CurrentMovingBloc.transform.position.z;
            currentTopBlocValue = CurrentTopBloc.transform.position.z;
            bounds = CurrentTopBloc.GetComponent<MeshRenderer>().bounds.size.z;
        }

        // Code
        if (Mathf.Abs(currentMovingBlocValue - currentTopBlocValue) <= 0.1f)  // 0.1 anchor
        {
            Combo += 1;
            if (Combo == maxCombo)
            {
                UpLocalScale(isPing);
            }
        }
        else
        {

            Combo = 0;
            ///TODO : ICI c'est de la merde par ce que je crois les bounds c'est la moitié de la taille j'aurais du utilisé les extends
            float toCut = Mathf.Abs(currentMovingBlocValue  - bounds);
    

            DownLocalScale(isPing, toCut);
            
            // scale oblige to create offset
            if (isPing)
            {
                CurrentMovingBloc.transform.position = cameraTarget.position - new Vector3((currentMovingBlocValue - bounds) /2, 0, 0);
            }
            else
            {
                CurrentMovingBloc.transform.position = cameraTarget.position - new Vector3(0, 0, (currentMovingBlocValue - bounds) / 2);
            }


            // CreateFallingBloc
            CreateFakeBloc(isPing, currentMovingBlocValue - bounds);

        }
    }

    public void CreateBloc()
    {
        SetCameraTarget();

        //lerp = (score % 10) * 5 / 100;
        //if ( (float)lerp >= 1){
        //    i = j;
        //    j += 1;

        //    if (j >= color.Length) j = 0;
        //}


        CurrentMovingBloc = Instantiate(prefabCube, cameraTarget.position - currentOffset, prefabCube.transform.rotation);
        //CurrentMovingBloc.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(color[i], color[j], lerp)) ;
        CurrentMovingBloc.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(color[0], color[1], (float)Score / 100)) ;

        CurrentMovingBloc.transform.localScale = CurrentTopBloc.transform.localScale;

    }

    public void CreateFakeBloc(bool isPing, float scale)
    {

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);


        if (isPing)
        {
            go.transform.position = cameraTarget.position - new Vector3((scale), 0, 0);
            go.transform.localScale = new Vector3(Mathf.Abs(scale), CurrentMovingBloc.transform.localScale.y, CurrentMovingBloc.transform.localScale.z)  ;

        }
        else
        {
            go.transform.position = cameraTarget.position - new Vector3(0, 0, (scale));
            go.transform.localScale = new Vector3(CurrentMovingBloc.transform.localScale.x, CurrentMovingBloc.transform.localScale.y, Mathf.Abs( scale)) ;
        }

        go.GetComponent<MeshRenderer>().material.SetColor("_Color", CurrentMovingBloc.GetComponent<MeshRenderer>().material.color);
        go.AddComponent<Rigidbody>();
    }

    public void SetCameraTarget()
    {
        cameraTarget.position = new Vector3(0, cameraTarget.position.y + prefabCube.GetComponent<MeshRenderer>().bounds.size.y, 0);

        if (Score % 2 == 0)
        {
            currentOffset = new Vector3(cameraTarget.position.x - 4, 0, 0);
        }
        else
        {
            currentOffset = new Vector3(0, 0, cameraTarget.position.z - 4);
        }

    }

    public void UpLocalScale(bool isPing)
    {
        if (isPing)
        {
            if (CurrentMovingBloc.transform.localScale.x + 0.2f < maxLocalScale.x)
                CurrentMovingBloc.transform.localScale = new Vector3(CurrentMovingBloc.transform.localScale.x + 0.2f, CurrentMovingBloc.transform.localScale.y, CurrentMovingBloc.transform.localScale.z);
        }
       
        else
        {
            if (CurrentMovingBloc.transform.localScale.z + 0.2f < maxLocalScale.z)
                CurrentMovingBloc.transform.localScale = new Vector3(CurrentMovingBloc.transform.localScale.x, CurrentMovingBloc.transform.localScale.y, CurrentMovingBloc.transform.localScale.z + 0.2f);
        }
    }

    public void DownLocalScale(bool isPing, float toCut)
    {
        if (isPing)
        {
            if (CurrentMovingBloc.transform.localScale.x - toCut > 0.01f)
                CurrentMovingBloc.transform.localScale = new Vector3(CurrentMovingBloc.transform.localScale.x - toCut, CurrentMovingBloc.transform.localScale.y, CurrentMovingBloc.transform.localScale.z);
        }

        else
        {
            if (CurrentMovingBloc.transform.localScale.z - toCut > 0.01f)
                CurrentMovingBloc.transform.localScale = new Vector3(CurrentMovingBloc.transform.localScale.x, CurrentMovingBloc.transform.localScale.y, CurrentMovingBloc.transform.localScale.z - toCut);
        }
    }

    public void UpdateUI()
    {
        scoreText.text = "" + score;
    }


    public IEnumerator ReloadGame()
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(0);
    }
}
