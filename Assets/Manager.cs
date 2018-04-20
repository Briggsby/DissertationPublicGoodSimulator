using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    #region Board
    [Header("Board")]
    public int boardSize = 15;
    public GameObject unitPrefab;
    public float unitSize = 1f;
    [HideInInspector]
    public Unit[,] board;
    [HideInInspector]
    public Cell[,] cells;

    void CreateBoard()
    {
        CreateBoardArray();
        MakeBoard();
        SetUnitNeighbors();
    }

    public void ResetBoard()
    {
        Culling();
        foreach (Unit u in board)
        {
            if (u != null)
            {
                Destroy(u.gameObject);
            }
        }
        CreateBoard();
    }

    void CreateBoardArray()
    {
        board = new Unit[(4 * boardSize) + 1, (boardSize * 2) + 1];
        cells = new Cell[(4 * boardSize) + 1, (boardSize * 2) + 1];
        for (int x = 0; x < (4*boardSize)+1; x++)
        {
            for (int y = 0; y< (2*boardSize)+1; y++)
            {
                board[x, y] = null;
                cells[x, y] = null;
            }
        }
    }

    public Unit ChooseRandomBoardLocationLeftSide()
    {
        for (int try100Times = 0; try100Times < 100; try100Times++)
        {
            int xCoord = Random.Range(4, 2 * boardSize);
            int yCoord = Random.Range(4, boardSize * 2);
            if (board[xCoord, yCoord] != null)
            {
                return board[xCoord, yCoord];
            }
        }

        return null;

    }

    public Unit ChooseRandomBoardLocationRightSide()
    {
        for (int try100Times = 0; try100Times < 100; try100Times++)
        {
            int xCoord = Random.Range(2*boardSize+4, (4 * boardSize)-4);
            int yCoord = Random.Range(4, (boardSize * 2)-4);
            if (board[xCoord, yCoord] != null)
            {
                return board[xCoord, yCoord];
            }
        }

        return null;
    }

    void MakeBoard()
    {
        float height = 0f;
        float widthAdjust = Mathf.Sqrt(3) * unitSize / 2;
        float heightAdjust = 3 / 4f * unitSize;

        //Do the first row
        CreateUnit(0, 0, widthAdjust, heightAdjust);
        for (int x = 1; x <= boardSize; x++)
        {
            CreateUnit(x, height, widthAdjust, heightAdjust);
            CreateUnit(-x, height, widthAdjust, heightAdjust);
        }


        //Then create the rest of the rows
        bool addHalf = true;
        height = 1f;
        int currentWidth = boardSize - 1;


        while ((height) < boardSize + 1)
        {
            for (float x = 0; x <= currentWidth; x++)
            {
                if (addHalf)
                {
                    x += 0.5f;
                }
                CreateUnit(x, height, widthAdjust, heightAdjust);
                CreateUnit(x, -height, widthAdjust, heightAdjust);
                if (x != 0)
                {
                    CreateUnit(-x, height, widthAdjust, heightAdjust);
                    CreateUnit(-x, -height, widthAdjust, heightAdjust);

                }
                if (addHalf)
                {
                    x -= 0.5f;
                }

            }

            if (!addHalf)
            {
                currentWidth--;
            }

            addHalf = !addHalf;
            height++;
        }
    }

    void CreateUnit(float xCoord, float yCoord, float widthAdjust, float heightAdjust)
    {
        GameObject unitObj = Instantiate(unitPrefab, new Vector3(widthAdjust * xCoord, heightAdjust * yCoord), new Quaternion(0, 0, 0, 0));
        unitObj.transform.localScale = new Vector3(unitSize, unitSize, 0);
        Unit unit = unitObj.GetComponent<Unit>();
        unit.manager = this;
        SetUnitCoordinates(unit, xCoord, yCoord);

    }

    void SetUnitCoordinates(Unit unit, float xCoord, float yCoord)
    {
        unit.xCoord = xCoord;
        unit.xCoordBoard = (int)((xCoord + boardSize) * 2);
        unit.yCoord = yCoord;
        unit.yCoordBoard = (int)unit.yCoord + boardSize;
        board[unit.xCoordBoard, unit.yCoordBoard] = unit;
    }

    public Unit ConvertToBoardUnit(float x, float y)
    {
        int unitXCoord = (int)((x + boardSize) * 2);
        int unitYCoord = (int)y + boardSize;
        int boardXsize = (4 * boardSize) + 1;
        int boardYSize = (boardSize * 2) + 1;

        if (unitXCoord < 0 || unitYCoord < 0 || unitXCoord >= boardXsize || unitYCoord >= boardYSize)
        {
            return null;

        }
        else
        {
            return board[unitXCoord, unitYCoord];

        }
    }

    void SetUnitNeighbors()
    {
        foreach (Unit u in board)
        {
            if (u != null)
            {
                u.neighbors.Clear();

                if (u.xCoordBoard + 1 < (boardSize * 4) + 1)
                {
                    if (u.yCoordBoard + 1 < (boardSize * 2) + 1)
                    {
                        if (board[u.xCoordBoard + 1, u.yCoordBoard + 1] != null)
                        {
                            u.neighbors.Add(board[u.xCoordBoard + 1, u.yCoordBoard + 1]);
                        }
                    }
                    if (u.yCoordBoard - 1 > -1)
                    {
                        if (board[u.xCoordBoard + 1, u.yCoordBoard - 1] != null)
                        {
                            u.neighbors.Add(board[u.xCoordBoard + 1, u.yCoordBoard - 1]);
                        }
                    }
                    if (u.xCoordBoard + 2 < (boardSize * 4) + 1)
                    {
                        if (board[u.xCoordBoard + 2, u.yCoordBoard] != null)
                        {
                            u.neighbors.Add(board[u.xCoordBoard + 2, u.yCoordBoard]);
                        }
                    }

                }
                if (u.xCoordBoard - 1 > -1)
                {
                    if (u.xCoordBoard - 2 > -1)
                    {
                        if (board[u.xCoordBoard - 2, u.yCoordBoard] != null)
                        {
                            u.neighbors.Add(board[u.xCoordBoard - 2, u.yCoordBoard]);
                        }
                    }
                    if (u.yCoordBoard + 1 < (boardSize * 2) + 1)
                    {
                        if (board[u.xCoordBoard - 1, u.yCoordBoard + 1] != null)
                        {
                            u.neighbors.Add(board[u.xCoordBoard - 1, u.yCoordBoard + 1]);
                        }
                    }
                    if (u.yCoordBoard - 1 > -1)
                    {
                        if (board[u.xCoordBoard - 1, u.yCoordBoard - 1] != null)
                        {
                            u.neighbors.Add(board[u.xCoordBoard - 1, u.yCoordBoard - 1]);
                        }
                    }

                }

                ShuffleNeighbors(u);
                u.numberOfNeighbors = u.neighbors.Count;
            }
        }
    }

    void ShuffleNeighbors(Unit unit)
    {
        int n = unit.neighbors.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range((int)0,(int)(n + 1));
            Unit swappedUnit = unit.neighbors[k];
            unit.neighbors[k] = unit.neighbors[n];
            unit.neighbors[n] = swappedUnit;
        }
    }

    public List<Unit> GetUnitsRange(Unit unit, int range)
    {
        List<Unit> units = new List<Unit>();

        float x = range;
        //First try the two units at range distance away on the same height
        Unit unitTry = ConvertToBoardUnit(x + unit.xCoord, unit.yCoord);
        if (unitTry != null)
        {
            units.Add(unitTry);
        }
        unitTry = ConvertToBoardUnit(unit.xCoord - x, unit.yCoord);
        if (unitTry != null)
        {
            units.Add(unitTry);
        }

        //Then add the hex units climbing up to the flat edge
        float y = 0;
        for (int i = range; i > 0; i--)
        {
            x -= 0.5f;
            y++;
            unitTry = ConvertToBoardUnit(unit.xCoord + x, unit.yCoord + y);
            if (unitTry != null)
            {
                units.Add(unitTry);
            }
            unitTry = ConvertToBoardUnit(unit.xCoord - x, unit.yCoord + y);
            if (unitTry != null)
            {
                units.Add(unitTry);
            }
            unitTry = ConvertToBoardUnit(unit.xCoord + x, unit.yCoord - y);
            if (unitTry != null)
            {
                units.Add(unitTry);
            }
            unitTry = ConvertToBoardUnit(unit.xCoord - x, unit.yCoord - y);
            if (unitTry != null)
            {
                units.Add(unitTry);
            }
        }
        //Now do the flat edge, and there are always range-1 to do
        for (int i = range - 1; i > 0; i--)
        {
            x--;
            unitTry = ConvertToBoardUnit(unit.xCoord + x, unit.yCoord + y);
            if (unitTry != null)
            {
                units.Add(unitTry);
            }
            unitTry = ConvertToBoardUnit(unit.xCoord + x, unit.yCoord - y);
            if (unitTry != null)
            {
                units.Add(unitTry);
            }
        }

        return units;
    }



    #endregion

    #region Camera
    [Header("Camera")]
    public float cameraSpeed;
    public float cameraScrollSpeed;

    void DoCamera()
    {
        CameraMove();
        CameraScroll();
    }

    void CameraMove()
    {
        //move the camera around
        if (Input.GetAxis("Horizontal") != 0 | Input.GetAxis("Vertical") != 0)
        {
            Vector3 cameraPos = Camera.main.transform.position;
            cameraPos.x += Input.GetAxis("Horizontal") * cameraSpeed;
            cameraPos.y += Input.GetAxis("Vertical") * cameraSpeed;
            Camera.main.transform.position = cameraPos;
        }
    }

    void CameraScroll()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Camera.main.transform.Translate(Vector3.back * -cameraScrollSpeed * Input.GetAxis("Mouse ScrollWheel"));
        }
    }

    #endregion

    #region Editing
    [Header("Editing")]
    public InputField[] rgbRange = new InputField[4];
    public Toggle editorModeToggle;
    public bool editorMode = false;
    public float cellFitness;
    public bool overwritingCell = false;
    public bool producer = true;

    public void Controls()
    {
        DoCamera();
        Pausing();
        ResetBoardCheck();
        CullingCheck();
        InstantSeedCheck();
        HalfAndHalf();
    }

    public void EditPauseToggle()
    {
        editorModeToggle.gameObject.SetActive(!editorModeToggle.gameObject.activeSelf);
        foreach (InputField i in rgbRange)
        {
            i.gameObject.SetActive(!i.gameObject.activeSelf);
        }
    }

    public void EditorModeToggle()
    {
        editorMode = !editorMode;
    }

    void SetEditorValuesTo0IfBlank()
    {
        foreach(InputField i in rgbRange)
        {
            if (string.IsNullOrEmpty(i.text))
            {
                i.text = "0";
            }
        }
    }

    public void PlaceColor(Unit unit)
    {
        SetEditorValuesTo0IfBlank();

        int brushSize = int.Parse(rgbRange[3].text);
        long r = long.Parse(rgbRange[0].text);
        long g = long.Parse(rgbRange[1].text);
        long b = long.Parse(rgbRange[2].text);

        for (int i = 0; i<=brushSize; i++)
        {
            foreach (Unit u in GetUnitsRange(unit, i))
            {
                u.rgb[0] = r;
                u.rgb[1] = g;
                u.rgb[2] = b;
                u.UpdateColor();
            }
        }
    }
    
    public Cell PlaceCell(Unit unit, bool fromEdit = false)
    {
        GameObject cellObj = Instantiate(cellPrefab, unit.gameObject.transform);
        cellCount++;
        Cell cell = cellObj.GetComponent<Cell>();
        cell.manager = this;
        cell.fitness = cellFitness;
        cell.unit = unit;
        unit.cell = cell;
        cell.cellID = AssignCellID();

        if (fromEdit)
        {
            if (producer)
            {
                cell.producer = true;
                cell.GetComponent<SpriteRenderer>().color = producerColor;
            }
            else
            {
                cell.producer = false;
                cell.GetComponent<SpriteRenderer>().color = exploiterColor;
            }
            UpdateProducerCount(cell, BirthOrDeath.Birth);
            cell.ApplyBiofilm();
        }

        return cell;
    }

    public void DisplayInfo(Unit unit)
    {
        for (int colourIndex = 0; colourIndex<3; colourIndex++)
        {
            rgbRange[colourIndex].text = string.Format("{0}", unit.rgb[colourIndex]);
        }
        if (unit.cell != null)
        {
            fitnessText.text = string.Format("{0}", unit.cell.fitness);
        }
        else
        {
            fitnessText.text = "";
        }
    }

    public void CullingCheck()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Culling();
        }
    }

    public void Culling()
    {
        if (!paused)
        {
            PauseToggle();
        }

        ResetVariablesForCull();


        foreach (Unit u in board)
        {
            if (u != null)
            {
                u.rgb[0] = 0;
                u.rgb[1] = 0;
                u.rgb[2] = 0;
                u.UpdateColor();
                if (u.cell != null)
                {
                    Destroy(u.cell.gameObject);
                    u.cell = null;
                }
                u.viscosity = 1;
                u.biofilms.Clear();

            }
        }
    }

    void ResetVariablesForCull()
    {
        timePassed = 0f;
        cellID = 0;
        cellCount = 0;
        producerCount = 0;
        highestProducerCount = 0;
        lowestProducerCount = 9999;
        timeAtHighestProdCount = 0f;
        timeAtLowestProdCount = 0f;
        producerCountReachedTrough = false;
        highestProducerCountAfterStable = 0;
        lowestProducerCountAfterStable = 9999;
        timeAtHighestProdCountAfterStable = 0f;
        timeAtLowestProdCountAfterStable = 0f;
        producerCountReachedTroughAfterStable = false;

    }

    public void InstantSeedCheck()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            InstantSeed();
        }
    }

    public void InstantSeed()
    {
        producer = true;
        ChooseRandomBoardLocationLeftSide().TryPlaceCellUnitsRange(8);
        producer = false;
        ChooseRandomBoardLocationRightSide().TryPlaceCellUnitsRange(8);
    }

    public void HalfAndHalf()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Culling();
            foreach (Unit u in board)
            {
                if (u != null)
                {
                    producer = true;
                    if (u.yCoordBoard > boardSize)
                    {
                        producer = false;
                    }
                    PlaceCell(u, true);
                }
            }
        }
    }

    public void ResetBoardCheck()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBoard();
        }
    }
    #endregion

    #region Info
    [Header("Info")]
    public Text fitnessText;


    #endregion

    #region TimeStep
    [Header("Time")]
    public float timeStepPerSecond;
    public bool adjustTimeStep;
    public bool paused = true;
    public float timePassed = 0f;
    public long cellID = 0;


    public void PauseToggle()
    {
        paused = !paused;
        EditPauseToggle();
    }

    void Pausing()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PauseToggle();
        }
    }

    void TimePassing()
    {
        if (paused)
        {
            return;
        }
        timePassed += timeStepPerSecond * Time.deltaTime;
        DoTimers(timeStepPerSecond * Time.deltaTime);
    }

    void DoTimers(float time)
    {
        CountingTimer(time);
    }

    long AssignCellID()
    {
        cellID++;
        return cellID;
    }
    #endregion

    #region Diffusion
    [Header("Diffusion")]
    public bool doDiffusion = true;
    public float diffusionTimeStep = 100f;
    public float[] diffusionConsts = new float[3] { 0f, 0f, 0f };
    public long diffusionCutOff = 1;

    #endregion

    #region Cells
    [Header("Cells")]
    public GameObject cellPrefab;

    [Header("FitnessStep")]
    public float selection = 1f;
    public float fitnessTimeStep = 1f;
    public float costToLive = 0f;
    public float costToLivePerRep = 0f;

    [Header("Fitness Thresholds")]
    public float fitnessDeathThreshold = 0f;
    public float fitnessRepThreshold = 1f;

    [Header("Reproduction")]
    public float costOfCompetition;
    public bool bothSidesCompetingCost;
    public float mutationRate;
    public float minimumFitnessSplit;
    public float maximumFitnessSplit;

    #endregion

    #region Public Goods

    [Header("Direct Fitness")]
    public bool doDirectFitness = true;
    public float directFitnessTimer = 1f;
    public int directFitnessRange = 1;
    public float directFitnessBenefit = 1f;
    public float directFitnessCost = 0.5f;
    public float directFitnessExploitability = 0.5f;


    [Header("Color Secretion")]
    public bool doColorSecretion = true;
    public bool selfRegulateSecretion = false;
    public float colorSecretionTimer = 1f;
    public int colorSecreted = 2;
    public long amountColorSecreted = 1;
    public float colorSecretionCost = 0f;

    #endregion

    #region Biofilm
    [Header("Biofilm")]
    public bool doBiofilms;
    public bool exploiterDoBiofilms;
    public bool exploiterBiofilmsOverwrite = false;
    public float biofilmViscosity;
    public int biofilmRange;
    public float biofilmCost;
    public float biofilmExploiterViscosityMult = 1f;
    public float biofilmExploiterCostMult = 1f;
    public float[] biofilmUptakeMults = new float[3];
    public float biofilmExploiterUptakeMult = 0f;

    #endregion

    #region ColorMetabolism

    [Header("Color Metabolism")]
    public bool doColorMetabolism = true;
    public bool colorMetabolismWithoutUptake = false;
    public float colorMetabolismTimer = 1f;
    public long[] colorUptake = new long[3] { 0, 0, 0 };
    public float[] fitnessPerUnitColor = new float[3] { 0f, 0f, 0.001f };
    public float[] colorUptakeMultExploiters = new float[3] { 1f, 1f, 1f };
    public float[] colorFitnessMultExploiters = new float[3] { 1f, 1f, 1f };

    #endregion

    #region Counting
    [Header("Counting")]
    public bool doCounting = false;
    public float countingTimer;

    public float countingTimestepCounter = 0f;


    void CountingTimer(float time)
    {
        if (!doCounting)
        {
            return;
        }

        countingTimestepCounter += time;
        if (countingTimestepCounter > countingTimer)
        {
            Counting();
            countingTimestepCounter -= countingTimer;
        }

    }

    void Counting()
    {
        Debug.Log(string.Format("Producer Frequency At {0}: {1}",timePassed, (float)((float)producerCount / (float)cellCount)));
    }

    #endregion

    #region Data
    [Header("Data")]
    public CSVDataRecorder csvRecorder;
    public long cellCount = 0;
    public long producerCount = 0;
    public long highestProducerCount = 0;
    public long lowestProducerCount = 9999;
    public float timeAtHighestProdCount = 0f;
    public float timeAtLowestProdCount = 0f;
    private bool producerCountReachedTrough = false;

    public long highestProducerCountAfterStable = 0;
    public long lowestProducerCountAfterStable = 9999;
    public float timeAtHighestProdCountAfterStable = 0f;
    public float timeAtLowestProdCountAfterStable = 0f;
    private bool producerCountReachedTroughAfterStable = false;


    public enum BirthOrDeath { Birth, Death }

    public void UpdateProducerCount(Cell cell, BirthOrDeath birthOrDeath )
    {
        if (birthOrDeath == BirthOrDeath.Birth)
        {
            if (cell.producer)
            {
                producerCount++;
                
                if (producerCount > highestProducerCount)
                {
                    highestProducerCount = producerCount;
                    timeAtHighestProdCount = timePassed;

                }

                if (timePassed >= csvRecorder.timeStableBy)
                {
                    if (producerCount > highestProducerCountAfterStable)
                    {
                        highestProducerCountAfterStable = producerCount;
                        timeAtHighestProdCountAfterStable = timePassed;
                    }
                }
            }
        }
        else if (birthOrDeath == BirthOrDeath.Death)
        {
            if (cell.producer)
            {
                producerCount--;

                if (!producerCountReachedTrough && producerCount < lowestProducerCount)
                {
                    lowestProducerCount = producerCount;
                    timeAtLowestProdCount = timePassed;
                    if (producerCount == 0)
                    {
                        producerCountReachedTrough = true;
                    }
                }
                if (!producerCountReachedTroughAfterStable && timePassed >= csvRecorder.timeStableBy)
                {
                    if (producerCount < lowestProducerCountAfterStable)
                    {
                        lowestProducerCountAfterStable = producerCount;
                        timeAtLowestProdCountAfterStable = timePassed;

                        if (producerCount == 0)
                        {
                            producerCountReachedTroughAfterStable = true;
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Flow or Degredation
    [Header("Flow")]
    public bool doFlow;
    public bool flowProportionalToAmount;
    public bool flowOutOfBoard;
    public int[] positiveFlowXDir = new int[3];
    public int[] positiveflowYDir = new int[3];
    public float flowTimeStep = 1f;
    public float[] flowAmountPerTimeStep = new float[3];

    [Header("Degredation")]
    public bool doDegredation = false;
    public bool degredationReducedByBiofilm = true;
    public float degredationTimeStep = 1f;
    public bool degredationProportionalToAmount = false;
    public float[] degredationAmountPerTimeStep = new float[3];

    #endregion

    #region ColourConvertColour
    [Header("ColorConvertingColor")]
    public bool doColorConvertColor = false;
    public int colorConverting = 2;
    public int colorConverted = 0;
    public int colorConvertedTo = 1;
    public float colorConvertColorTimeStep = 1f;
    public long amountColorConvertPerUnit = 1;
    public float convertingColorDegredationFraction = 1f;

    #endregion

    [Header("Misc")]
    public float colorDivide;
    public Color producerColor;
    public Color exploiterColor;
    public int red = 0;
    public int green = 1;
    public int blue = 2;


    // Use this for initialization
    void Start () {
        CreateBoard();
	}
	
	// Update is called once per frame
	void Update () {
        Controls();
        TimePassing();
	}
}
