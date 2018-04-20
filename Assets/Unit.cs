using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    [HideInInspector]
    public Manager manager;

    #region Coordinates
    public float xCoord;
    public float yCoord;
    public int xCoordBoard;
    public int yCoordBoard;
    public List<Unit> neighbors = new List<Unit>();
    public int numberOfNeighbors;
    #endregion

    #region Colour and Editing
    public SpriteRenderer sprite;
    public long[] rgb = new long[3] { 0, 0, 0 };

    public void UpdateColor()
    {
        sprite.color = new Color(rgb[0] / manager.colorDivide, rgb[1] / manager.colorDivide, rgb[2] / manager.colorDivide);
    }

    private void OnMouseDown()
    {

            //Stops it happening if the mouse is over a UI thing
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            //if the mouse is left-clicked on a unit, and the cycle is the latest, change the color to the manager's coloredit
            if (manager.editorMode)
            {

                manager.PlaceColor(this);

            }


            //if the mouse is left-clicked on a unit, and editor mode isn't on, then display information about that unit
            if (!manager.editorMode)
            {
                manager.DisplayInfo(this);
            }

    }

    private void OnMouseEnter()
    {

            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                if (manager.editorMode)
                {
                    manager.PlaceColor(this);
                }
            }

            //if the mouse is over the unit, and the right mouse button is clicked, create a microbe of the manager's microbe creater on this unit

            if (Input.GetMouseButton(1) && manager.editorMode)
            {
                TryPlaceCellUnitsRange(int.Parse(manager.rgbRange[3].text));
            }

    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButton(1) && manager.editorMode)
        {
            TryPlaceCellUnitsRange(int.Parse(manager.rgbRange[3].text));
        }
    }

    public void TryPlaceCellUnitsRange(int range)
    {
        for (int r = 0; r<=range;r++)
        {
            foreach (Unit u in manager.GetUnitsRange(this, r))
            {
                u.TryPlaceCell();
            }
        }
    }

    public void TryPlaceCell()
    {
        if (cell != null)
        {
            if (!manager.overwritingCell)
            {
                return;
            }
            else
            {
                cell.CellDeath();
                manager.PlaceCell(this, true);
            }
        }
        else
        {
            manager.PlaceCell(this, true);
        }
    }
    #endregion

    #region Cells

    public Cell cell;

    #endregion

    #region Timers

    void DoTimers()
    {
        if (manager.paused)
        {
            return;
        }

        float timePassed = Time.deltaTime * manager.timeStepPerSecond;

        DiffusionTimer(timePassed);
        FlowTimer(timePassed);
        ColorConvertColorTimer(timePassed);
        DegredationTimer(timePassed);
    }

    #endregion

    #region Diffusion

    public float viscosity = 1f;
    public float diffusionCounter = 0f;

    void DiffusionTimer(float time)
    {
        if (!manager.doDiffusion)
        {
            return;
        }
        diffusionCounter += time;

        if (manager.diffusionTimeStep <= 0)
        {
            manager.diffusionTimeStep = 0.1f;
        }

        if (diffusionCounter <= manager.diffusionTimeStep && manager.adjustTimeStep)
        {
            manager.timeStepPerSecond++;
        }

        while (diffusionCounter >= manager.diffusionTimeStep)
        {
            if (manager.adjustTimeStep && manager.timeStepPerSecond > 1)
            {
                manager.timeStepPerSecond--;
            }
            Diffuse();
            diffusionCounter -= manager.diffusionTimeStep;
        }
    }

    void Diffuse()
    {
        for (int c = 0; c < 3; c++)
        {
            if (rgb[c] > manager.diffusionCutOff)
            {

                float diffusionConst = manager.diffusionConsts[c];

                float[] diffAmounts = new float[neighbors.Count];
                float diffTotal = 0f;

                float diffStart = neighbors.Count;

                for (int i = neighbors.Count - 1; i > -1; i--)
                {
                    long concGrad = neighbors[i].rgb[c] - rgb[c];
                    if (concGrad < 0)
                    {
                        diffAmounts[i] = -concGrad * diffusionConst * (1/viscosity);
                        diffTotal += diffAmounts[i];
                    }
                }


                if (diffTotal > rgb[c])
                {
                    for (int i = neighbors.Count - 1; i > -1; i--)
                    {
                        diffAmounts[i] = diffAmounts[i] * rgb[c] / diffTotal;
                    }
                }

                if (diffTotal > 0)
                {
                    for (int i = neighbors.Count - 1; i > -1; i--)
                    {
                        rgb[c] -= (long)diffAmounts[i];
                        neighbors[i].rgb[c] += (long)diffAmounts[i];
                        neighbors[i].UpdateColor();

                    }
                }
            }
        }

        UpdateColor();
    }
    #endregion

    #region Biofilm
    /*
    public long biofilmID;
    public bool biofilmExploiter = false;

    public void CheckNewBiofilm()
    {
        foreach (Unit u in manager.GetUnitsRange(this, manager.biofilmRange))
        {
            if (u.cell != null)
            {
                if (u.cell.producer)
                {
                    biofilmID = u.cell.cellID;
                    biofilmExploiter = false;
                    viscosity = manager.biofilmViscosity;
                    break;
                }
                else
                {
                    if (manager.exploiterDoBiofilms)
                    {
                        biofilmID = u.cell.cellID;
                        biofilmExploiter = true;
                        viscosity = manager.biofilmViscosity * manager.biofilmExploiterViscosityMult;
                    }
                }
            }
        }
    }
    */

    public List<bool> biofilms = new List<bool>();
    public bool producerBiofilmActive = false;
    public bool exploiterBiofilmActive = false;

    public void CheckBiofilms()
    {
        if (!manager.doBiofilms)
        {
            return;
        }
        if (biofilms.Count == 0)
        {
            viscosity = 1f;
            producerBiofilmActive = false;
            exploiterBiofilmActive = false;
        }

        else
        {
            producerBiofilmActive = false;
            exploiterBiofilmActive = false;
            foreach (bool b in biofilms)
            {
                if (b)
                {
                    producerBiofilmActive = true; ;
                }
                if (!b)
                {
                    exploiterBiofilmActive = true;
                }
            }
        }

        CheckBiofilmOverwrite();
        SetBiofilmViscosity();
    }

    public void CheckBiofilmOverwrite()
    {
        if (producerBiofilmActive && exploiterBiofilmActive)
        {
            if (!manager.exploiterBiofilmsOverwrite)
            {
                producerBiofilmActive = true;
                exploiterBiofilmActive = false;
            }
            if (manager.exploiterBiofilmsOverwrite)
            {
                producerBiofilmActive = false;
                exploiterBiofilmActive = true;
            }
        }
    }

    public void SetBiofilmViscosity()
    {
        if (producerBiofilmActive)
        {
            viscosity = manager.biofilmViscosity;
        }
        if (exploiterBiofilmActive)
        {
            viscosity = manager.biofilmViscosity * manager.biofilmExploiterViscosityMult;
        }
        if (!producerBiofilmActive && !exploiterBiofilmActive)
        {
            viscosity = 1f;
        }
    }


    #endregion

    #region Flow

    public float flowCounter;

    void FlowTimer(float time)
    {
        if (!manager.doFlow)
        {
            return;
        }
        flowCounter += time;

        if (flowCounter <= manager.flowTimeStep && manager.adjustTimeStep)
        {
            manager.timeStepPerSecond++;
        }

        while (flowCounter >= manager.flowTimeStep)
        {
            if (manager.adjustTimeStep && manager.timeStepPerSecond > 1)
            {
                manager.timeStepPerSecond--;
            }
            Flow();
            flowCounter -= manager.flowTimeStep;
        }
    }

    void Flow()
    {
        for (int c = 0; c<3; c++)
        {
            if (rgb[c] > 0) {
                Unit flowUnitDirection = FindFlowUnit(c);
                if (flowUnitDirection == null && manager.flowOutOfBoard)
                {
                    FlowIntoUnit(c, null);
                }
                else if (flowUnitDirection != null)
                {
                    FlowIntoUnit(c, flowUnitDirection);
                }
            }
        }
    }

    Unit FindFlowUnit(int colourIndex)
    {
        int xCoordBoardOfUnit = xCoordBoard + manager.positiveFlowXDir[colourIndex];
        int yCoordBoardOfUnit = yCoordBoard + manager.positiveflowYDir[colourIndex];

        if (xCoordBoardOfUnit >= (4 * manager.boardSize)+ 1 || xCoordBoardOfUnit<0 || yCoordBoardOfUnit >= (manager.boardSize * 2) + 1 || yCoordBoardOfUnit < 0)
        {
            return null;
        }

        if (manager.board[xCoordBoardOfUnit, yCoordBoardOfUnit] != null)
        {
            return (manager.board[xCoordBoard + manager.positiveFlowXDir[colourIndex], yCoordBoard + manager.positiveflowYDir[colourIndex]]);
        }
        else
        {
            return null;
        }
    }

    long FindFlowAmount(int colourIndex)
    {
        if (manager.flowProportionalToAmount)
        {
            return (long)((rgb[colourIndex] * manager.flowAmountPerTimeStep[colourIndex])/viscosity);
        }
        else
        {
            return (long)((manager.flowAmountPerTimeStep[colourIndex])/viscosity);
        }
    }

    void FlowIntoUnit(int colourIndex, Unit unit)
    {
        if (unit == null)
        {
            if (manager.flowOutOfBoard)
            {
                rgb[colourIndex] -= FindFlowAmount(colourIndex);
                if (rgb[colourIndex] < 0)
                {
                    rgb[colourIndex] = 0;
                }
            }
        }
        else
        {
            long flowAmount = FindFlowAmount(colourIndex);
            if (rgb[colourIndex] > flowAmount)
            {
                unit.rgb[colourIndex] += flowAmount;
                rgb[colourIndex] -= flowAmount;
            }
            else
            {
                unit.rgb[colourIndex] += rgb[colourIndex];
                rgb[colourIndex] = 0;
            }
            unit.UpdateColor();
        }

        UpdateColor();

    }

    #endregion

    #region Degredation

    public float degredationCounter;

    void DegredationTimer(float time)
    {
        if (!manager.doDegredation)
        {
            return;
        }
        degredationCounter += time;

        if (degredationCounter <= manager.degredationTimeStep && manager.adjustTimeStep)
        {
            manager.timeStepPerSecond++;
        }

        while (degredationCounter >= manager.degredationTimeStep)
        {
            if (manager.adjustTimeStep && manager.timeStepPerSecond > 1)
            {
                manager.timeStepPerSecond--;
            }
            Degredation();
            degredationCounter -= manager.degredationTimeStep;
        }
    }

    void Degredation()
    {
        for (int c = 0; c < 3; c++)
        {
            if (rgb[c] > 0)
            {

                long degredationAmount = 0;

                if (manager.degredationProportionalToAmount)
                {
                    degredationAmount = (long)(rgb[c] * manager.degredationAmountPerTimeStep[c]);
                }
                else
                {
                    degredationAmount = (long)manager.degredationAmountPerTimeStep[c];
                }
                if (manager.degredationReducedByBiofilm)
                {
                    degredationAmount = (long)(degredationAmount / viscosity);
                }

                if (rgb[c] > degredationAmount)
                {
                    rgb[c] -= degredationAmount;
                }
                else
                {
                    rgb[c] = 0;
                }
            }
        }

    }

    #endregion

    #region ColorConvertColor 

    float colorConvertColorCounter;

    void ColorConvertColorTimer(float time)
    {
        if (!manager.doColorConvertColor)
        {
            return;
        }
        colorConvertColorCounter += time;

        if (colorConvertColorCounter <= manager.colorConvertColorTimeStep && manager.adjustTimeStep)
        {
            manager.timeStepPerSecond++;
        }

        while (colorConvertColorCounter >= manager.colorConvertColorTimeStep)
        {
            if (manager.adjustTimeStep && manager.timeStepPerSecond > 1)
            {
                manager.timeStepPerSecond--;
            }
            ColorConvertColor();
            colorConvertColorCounter -= manager.colorConvertColorTimeStep;
        }
    }

    void ColorConvertColor()
    {
        long amountCanConvert = (long)(rgb[manager.colorConverting] * manager.amountColorConvertPerUnit);

        if (rgb[manager.colorConverted] >= amountCanConvert)
        {
            rgb[manager.colorConvertedTo] += amountCanConvert;
            rgb[manager.colorConverted] -= amountCanConvert;
        }
        else
        {
            rgb[manager.colorConvertedTo] += rgb[manager.colorConverted];
            rgb[manager.colorConverted] = 0;
        }

        UpdateColor();

        ConvertingColorDegredation();
    }

    void ConvertingColorDegredation()
    {
        rgb[manager.blue] = (long)(rgb[manager.blue] * manager.convertingColorDegredationFraction);
        UpdateColor();
    }

    #endregion

    // Use this for initialization
    void Start () {
        sprite = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        DoTimers();
        CheckBiofilms();
	}
}
