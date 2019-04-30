using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

using UnityEngine.SceneManagement;



public class NewStackManager : MonoBehaviour

{

    public Text scoreText;                              // 점수

    public Color32[] gameColors = new Color32[4];       // 블럭 색

    public Material stackMaterial;                      // 블럭 Material

    public GameObject endPanel;                         // 게임 오버 판넬

    public GameObject clickPanel;                       // 터치를 위한 판넬 

    public ParticleSystem comboParticle;
    public AudioSource tapAudio;


    private const float BOUND_SIZE = 3.5f;                              // 블럭 사이즈

    private const float STACK_MOVING_SPEED = 5.0f;                      // 스택 이동 속도

    private const float ERROR_MARGIN = 0.1f;                            // 블럭 위치 마진



    private GameObject[] theStack;                                      // 스택 오브젝트

    private Vector2 stackBound = new Vector2(BOUND_SIZE, BOUND_SIZE);   // 스택 사이즈



    private int stackIndex = 0;     // 스택 순번

    private int scoreCount = 0;     // 점수

    private int combo = 0;          // 콤보( 4연 콤보부터 스택 사이즈 늘어남)



    private float tileTransition = 0.0f;        // 블럭 위치 변화

    private float tileSpeed = 4.5f;             // 블럭 이동 속도

    private float secondaryPosition;            // 움직일 위치



    private Vector3 desiredPosition;            // 스택이 움직일 위치

    private Vector3 lastTilePosition;           // 이전 블럭 위치



    private Vector3 preXPosition;           // X축 블럭 시작점

    private Vector3 nextXPosition;          // X축 블럭 도착점

    private Vector3 preZPosition;           // Z축 블럭 시작점

    private Vector3 nextZPosition;          // Z축 블럭 도착점



    private bool isMoveOnX = true;      // X축 OR Z축 방향    

    private bool isDead = false;



    int pos = 0;

    //int height = 0;



    private void Start()

    {

        theStack = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)

        {

            theStack[i] = transform.GetChild(i).gameObject;

            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);

        }



        stackIndex = transform.childCount - 1;

        preXPosition = new Vector3(3.5f, 0f, 0f);

        nextXPosition = new Vector3(-3.5f, 0f, 0f);

        preZPosition = new Vector3(0f, 0f, 3.5f);

        nextZPosition = new Vector3(0f, 0f, -3.5f);

    }



    private void Update()

    {

        if (isDead)

            return;



        MoveConstantly();



        transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);    // 전체 스택의 위치 조정



    }



    public void PlaceTile()

    {

        if (PlaceIt())

        {
            tapAudio.Play();
            scoreCount++;

            SpawnTile();

            scoreText.text = scoreCount.ToString();

        }

        else

        {

            EndGame();

        }

    }



    private void CreateRubble(Vector3 pos, Vector3 sca)     // 블럭의 잘리는 부분을 생성

    {

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);



        go.transform.localPosition = pos;

        go.transform.localScale = sca;

        go.AddComponent<Rigidbody>();



        go.GetComponent<MeshRenderer>().material = stackMaterial;

        ColorMesh(go.GetComponent<MeshFilter>().mesh);



        Destroy(go, 5f);

    }



    private void MoveConstantly()

    {

        tileTransition += Time.deltaTime * tileSpeed;



        switch (pos)

        {

            case 0:

                theStack[stackIndex].transform.position = new Vector3(preXPosition.x - tileTransition, 0, 0);

                break;

            case 1:

                theStack[stackIndex].transform.position = new Vector3(preXPosition.x + tileTransition, 0, 0);

                break;

            case 2:

                theStack[stackIndex].transform.position = new Vector3(0, 0, preZPosition.z - tileTransition);

                break;

            case 3:

                theStack[stackIndex].transform.position = new Vector3(0, 0, preZPosition.z + tileTransition);

                break;

        }

        if (Vector3.SqrMagnitude(theStack[stackIndex].transform.position - nextXPosition) <= 0.01 || Vector3.SqrMagnitude(theStack[stackIndex].transform.position - nextZPosition) <= 0.01)

        {

            ChangeDestination();

            if (isMoveOnX)

                pos = pos == 0 ? 1 : 0;

            else

                pos = pos == 2 ? 3 : 2;

        }

    }



    private void ChangeDestination()

    {

        if (isMoveOnX)

        {

            tileTransition = 0;

            preXPosition = nextXPosition;

            nextXPosition = (nextXPosition == preXPosition) ? nextXPosition : preXPosition;

            print(nextXPosition);

        }

        else

        {

            tileTransition = 0;

            preZPosition = nextZPosition;

            nextZPosition = (nextZPosition != preZPosition) ? preZPosition : nextZPosition;

        }

    }



    private void SpawnTile()

    {

        tileTransition = 0;

        lastTilePosition = theStack[stackIndex].transform.localPosition;



        stackIndex--;

        if (stackIndex < 0)

        {

            stackIndex = transform.childCount - 1;

        }



        desiredPosition = (Vector3.down) * scoreCount;

        // 새로운 블럭의 위치

        if (isMoveOnX)

        {

            preXPosition = new Vector3(3.5f, scoreCount, 0);

            theStack[stackIndex].transform.localPosition = new Vector3(3.5f, 0, 0);

        }

        else

        {

            preZPosition = new Vector3(0, scoreCount, 3.5f);

            theStack[stackIndex].transform.localPosition = new Vector3(0, 0, 3.5f);

        }

        // 새 블럭의 크기

        theStack[stackIndex].transform.localScale = new Vector3(stackBound.x, 1, stackBound.y);



        ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);

    }



    private bool PlaceIt()

    {

        Transform t = theStack[stackIndex].transform;

        var parCount = comboParticle.emission;          // 콤보 이펙트



        // 블럭위치에 따라 잔해를 생성하거나 콤보이펙트가 생기거나

        if (isMoveOnX)

        {

            float deltaX = lastTilePosition.x - t.position.x;

            if (Mathf.Abs(deltaX) > ERROR_MARGIN)

            {

                // cut the tile



                combo = 0;

                stackBound.x -= Mathf.Abs(deltaX);

                if (stackBound.x <= 0)

                    return false;



                float middle = lastTilePosition.x + t.localPosition.x / 2;

                t.localScale = new Vector3(stackBound.x, 1, stackBound.y);

                CreateRubble(

                    new Vector3((t.position.x > 0)

                        ? t.position.x + (t.localScale.x / 2)

                        : t.position.x - (t.localScale.x / 2)

                        , t.position.y, t.position.z),

                    new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z));

                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);

            }

            else

            {

                combo++;

                comboParticle.transform.position = new Vector3(0, 0, 0);

                parCount.rateOverTime = combo;

                comboParticle.Play();



                if (combo > 3)

                {

                    stackBound.x += 0.25f;



                    float middle = lastTilePosition.x + t.localPosition.x / 2;

                    t.localScale = new Vector3(stackBound.x, 1, stackBound.y);

                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);

                }

                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);



            }

        }

        else

        {

            float deltaZ = lastTilePosition.z - t.position.z;

            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)

            {

                // cut the tile



                combo = 0;

                stackBound.y -= Mathf.Abs(deltaZ);

                if (stackBound.y <= 0)

                    return false;



                float middle = lastTilePosition.z + t.localPosition.z / 2;

                t.localScale = new Vector3(stackBound.x, 1, stackBound.y);

                CreateRubble(

                    new Vector3(t.position.x, t.position.y,

                        (t.position.z > 0)

                        ? t.position.z + (t.localScale.z / 2)

                        : t.position.z - (t.localScale.z / 2)),

                    new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ)));

                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));

            }

            else

            {

                combo++;

                comboParticle.transform.position = new Vector3(0, 0, 0);

                comboParticle.Play();



                if (combo > 3)

                {

                    stackBound.x += 0.25f;

                    float middle = lastTilePosition.z + t.localPosition.z / 2;

                    t.localScale = new Vector3(stackBound.x, 1, stackBound.y);

                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));

                }



                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);



            }

        }



        secondaryPosition = (isMoveOnX) ? t.localPosition.x : t.localPosition.z;



        // 진행 방향 바꾸기

        if (pos == 0 || pos == 1)

            pos = 2;

        else

            pos = 0;



        isMoveOnX = !isMoveOnX;



        return true;

    }



    private void EndGame()

    {

        if (PlayerPrefs.GetInt("score") < scoreCount)

        {

            PlayerPrefs.SetInt("score", scoreCount);

        }



        isDead = true;

        clickPanel.SetActive(false);

        endPanel.SetActive(true);

    }



    public void SceneChange(string sceneName)

    {

        SceneManager.LoadScene(sceneName);

    }



    private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)

    {

        if (t < 0.33f)

            return Color.Lerp(a, b, t / 0.33f);

        else if (t < 0.66f)

            return Color.Lerp(b, c, (t - 0.33f) / 0.33f);

        else

            return Color.Lerp(c, d, (t - 0.66f) / 0.66f);

    }



    private void ColorMesh(Mesh mesh)

    {

        Vector3[] vertices = mesh.vertices;

        Color32[] colors = new Color32[vertices.Length];

        float f = Mathf.Sin(scoreCount * 0.25f);



        for (int i = 0; i < vertices.Length; i++)

        {

            colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);

        }



        mesh.colors32 = colors;

    }



}
