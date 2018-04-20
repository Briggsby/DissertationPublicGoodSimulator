using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {

    public Manager manager;
    public long cellID;

    #region Unit
    public Unit unit;

    #endregion

    #region FitnessStep
    public float fitness = 0f;
    public float fitnessCounter = 0f;
    public float effectiveFitness = 0f;
    public float actualFitness = 0f;


    void FitnessTimer(float time)
    {
        fitnessCounter += time;

        if (fitnessCounter <= manager.fitnessTimeStep && manager.adjustTimeStep)
        {
            manager.timeStepPerSecond++;
        }

        while (fitnessCounter > manager.fitnessTimeStep)
        {

            if (manager.adjustTimeStep && manager.timeStepPerSecond > 1)
            {
                manager.timeStepPerSecond--;
            }

            fitnessCounter -= manager.fitnessTimeStep;
            FitnessStep();
        }
    }

    void FitnessStep()
    {
        fitness -= manager.costToLive;
        fitness -= biofilmCost;
        fitness -= manager.costToLivePerRep * reproductions;

        FitnessResponse();
        
    }

    void FitnessResponse()
    {
        fitness = 1 - manager.selection + (manager.selection * fitness);

        CheckDeath();
        CheckReproduce();
    }



    #endregion

    #region Reproducing
    public float reproductions = 0;

    //THIS WAY OF REPRODUCING DOES NOT REPRODUCE IF IT PICKS AN OCCUPIED SPACE AND FAILS TO COMPETE
    void CheckReproduce()
    {
        if (fitness > manager.fitnessRepThreshold)
        {
            Reproduce();
        }
    }

    void Reproduce()
    {
        Unit location = PickReproduceLocation();
        if (location == null)
        {
            return;
        }

        SplitDaughterCell(location);
    }

    void SplitDaughterCell(Unit location)
    {
        reproductions++;
        Cell daughter = manager.PlaceCell(location);
        SplitFitness(daughter);
        CloneDaughter(daughter);
        if (CheckMutation())
        {
            MutateDaughter(daughter);
        }

        daughter.ApplyBiofilm();

        manager.UpdateProducerCount(daughter, Manager.BirthOrDeath.Birth);
    }

    void SplitFitness(Cell daughter)
    {
        float split = Random.Range(manager.minimumFitnessSplit, manager.maximumFitnessSplit);
        daughter.fitness = fitness * split;
        fitness -= fitness*split;
    }

    void CloneDaughter(Cell cell)
    {
        cell.producer = producer;
        cell.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
    }

    bool CheckMutation()
    {
        if (Random.Range(0f, 1) < manager.mutationRate)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void MutateDaughter(Cell cell)
    {
        cell.producer = !producer;
        if (cell.producer)
        {
            cell.GetComponent<SpriteRenderer>().color = manager.producerColor;
        }
        else
        {
            cell.GetComponent<SpriteRenderer>().color = manager.exploiterColor;
        }
    }

    Unit PickReproduceLocation()
    {
        Unit unitPicked = unit.neighbors[Random.Range(0, unit.numberOfNeighbors)];
        if (unitPicked.cell == null)
        {
            return unitPicked;
        }
        else
        {
            if (Compete(unitPicked.cell))
            {
                unitPicked.cell.CellDeath();
                return unitPicked;
            }
            else
            {
                return null;
            }
        }
    }

    bool Compete(Cell cell)
    {
        bool returnValue = false;
        if (fitness > cell.fitness)
        {
            returnValue = true;
        }
        else if (fitness == cell.fitness)
        {
            if (Random.Range(0f, 1f) > 0.5f)
            {
                returnValue = true;
            }
        }

        fitness -= manager.costOfCompetition;
        if (manager.bothSidesCompetingCost)
        {
            cell.fitness -= manager.costOfCompetition;
        }

        return returnValue;

    }


    #endregion

    #region Producers
    public bool producer;


    void PublicGoodTimers(float time)
    {
        if (!producer)
        {
            return;
        }
        DirectFitnessTimer(time);

        ColorSecretionTimer(time);
    }

    #region DirectFitness

    public float directFitnessCounter = 0f;


    void DirectFitnessTimer(float time)
    {
        if (!manager.doDirectFitness)
        {
            return;
        }

        directFitnessCounter += time;

        if (directFitnessCounter <= manager.directFitnessTimer && manager.adjustTimeStep)
        {
            manager.timeStepPerSecond++;
        }

        while (directFitnessCounter > manager.directFitnessTimer)
        {

            if (manager.adjustTimeStep && manager.timeStepPerSecond > 1)
            {
                manager.timeStepPerSecond--;
            }

            DirectFitness();
            directFitnessCounter -= manager.directFitnessTimer;
        }
    }

    void DirectFitness()
    {
        List<Cell> recipients = GetDirectFitnessRecipients();
        foreach (Cell c in recipients)
        {
            if (fitness-manager.fitnessDeathThreshold > manager.directFitnessCost)
            {
                //Debug.Log(string.Format("Fitness Before: {0}, Recip fitness Before: {1}, Recip Prod: {2}", fitness, c.fitness, c.producer));
                c.fitness += manager.directFitnessBenefit;
                fitness -= manager.directFitnessCost;
                //Debug.Log(string.Format("Fitness After: {0}, Recip fitness After: {1}", fitness, c.fitness));
            }
            else
            {
                break;
            }
        }
    }

    List<Cell> GetDirectFitnessRecipients()
    {
        List<Cell> recipients = new List<Cell>();

        for (int r = 1; r<=manager.directFitnessRange; r++)
        {
            foreach (Unit u in manager.GetUnitsRange(unit, r))
            {
                if (u.cell != null)
                {
                    if (u.cell.producer)
                    {
                        recipients.Add(u.cell);
                    }
                    else
                    {
                        if (Random.Range(0f,1f) < manager.directFitnessExploitability)
                        {
                            recipients.Add(u.cell);
                        }
                    }
                }
            }
        }

        return recipients;
    }

    #endregion

    #region Secretion

    public float colorSecretionCounter = 0f;

    void ColorSecretionTimer(float time)
    {
        if (!manager.doColorSecretion)
        {
            return;
        }
        colorSecretionCounter += time;

        if (colorSecretionCounter <= manager.colorSecretionTimer && manager.adjustTimeStep)
        {
            manager.timeStepPerSecond--;
        }

        while (colorSecretionCounter > manager.colorSecretionTimer)
        {
            if (manager.adjustTimeStep && manager.timeStepPerSecond > 1)
            {
                manager.timeStepPerSecond--;
            }

            ColorSecrete();
            colorSecretionCounter -= manager.colorSecretionTimer;
        }
    }

    void ColorSecrete()
    {
        if (fitness-manager.fitnessDeathThreshold > manager.colorSecretionCost)
        {
            if (manager.selfRegulateSecretion && unit.rgb[manager.colorSecreted] >= manager.colorUptake[manager.colorSecreted])
            {
                return;
            }


            unit.rgb[manager.colorSecreted] += manager.amountColorSecreted;
            unit.UpdateColor();
            fitness -= manager.colorSecretionCost;
        }
    }

    #endregion

    #endregion

    #region CellDeath

    void CheckDeath()
    {
        if (fitness < manager.fitnessDeathThreshold)
        {
            CellDeath();
        }
    }

    public void CellDeath()
    {
        RemoveBiofilm();

        unit.cell = null;
        manager.cellCount--;
        manager.UpdateProducerCount(this, Manager.BirthOrDeath.Death);
        Destroy(this.gameObject);
    }

    #endregion

    #region ColorMetabolism

    float colorMetabolismCounter = 0f;

    void ColorMetabolismTimer(float time)
    {
        if (!manager.doColorMetabolism)
        {
            return;
        }
        colorMetabolismCounter += time;

        if (colorMetabolismCounter <= manager.colorMetabolismTimer && manager.adjustTimeStep)
        {
            manager.timeStepPerSecond++;
        }

        while (colorMetabolismCounter > manager.colorMetabolismTimer)
        {
            if (manager.adjustTimeStep && manager.timeStepPerSecond > 1)
            {
                manager.timeStepPerSecond--;
            }

            ColorMetabolism();
            colorMetabolismCounter -= manager.colorMetabolismTimer;
        }
    }

    void ColorMetabolism()
    {
        if (manager.colorMetabolismWithoutUptake)
        {
            for (int c = 0; c < 3; c++)
            {
                if (unit.rgb[c] > 0)
                {
                    float maxAmount = manager.fitnessPerUnitColor[c] * manager.colorUptake[c];
                    if (unit.producerBiofilmActive)
                    {
                        maxAmount *= manager.biofilmUptakeMults[c];
                    }
                    else if (unit.exploiterBiofilmActive)
                    {
                        maxAmount *= manager.biofilmUptakeMults[c] * manager.biofilmExploiterUptakeMult;
                    }
                    float fitnessInc = unit.rgb[c] * manager.fitnessPerUnitColor[c];
                    if (fitnessInc*fitnessInc <= maxAmount*maxAmount)
                    {
                        fitness += unit.rgb[c] * manager.fitnessPerUnitColor[c];
                    }
                    else
                    {
                        fitness += maxAmount;
                    }
                }
            }
            return;
        }

        for (int c = 0; c < 3; c++)
        {
            if (unit.rgb[c] > 0)
            {
                float uptake = manager.colorUptake[c];

                if (manager.doBiofilms)
                {

                        if (unit.exploiterBiofilmActive)
                        {
                            uptake *= manager.biofilmUptakeMults[c] * manager.biofilmExploiterUptakeMult;
                        }
                        else if (unit.producerBiofilmActive)
                        {
                            uptake *= manager.biofilmUptakeMults[c];
                        }
                }


                if (!producer)
                {
                    uptake *= manager.colorUptakeMultExploiters[c];
                }

                if (unit.rgb[c] > uptake)
                {
                    unit.rgb[c] -= (long)uptake;
                    uptake = (long)uptake;
                }
                else
                {
                    uptake = unit.rgb[c];
                    unit.rgb[c] = 0;
                }

                float fitnessChange = uptake * manager.fitnessPerUnitColor[c];
                if (!producer)
                {
                    fitnessChange *= manager.colorFitnessMultExploiters[c];
                }

                fitness += fitnessChange;
            }
        }
        unit.UpdateColor();
    }

    #endregion

    #region Timers

    void DoTimers()
    {
        if (manager.paused)
        {
            return;
        }
        float timePassed = Time.deltaTime * manager.timeStepPerSecond;

        ColorMetabolismTimer(timePassed);
        PublicGoodTimers(timePassed);
        FitnessTimer(timePassed);
    }
    #endregion

    #region Biofilm

    public float biofilmCost = 0;
    public void ApplyBiofilm()
    {
        if (!manager.doBiofilms)
        {
            return;
        }

        if (!producer && !manager.exploiterDoBiofilms)
        {
            return;
        }

        int range = manager.biofilmRange;

        
        float biofilmViscosity = manager.biofilmViscosity;
        biofilmCost = manager.biofilmCost;

        if (!producer)
        {
            biofilmViscosity *= manager.biofilmExploiterViscosityMult;
            biofilmCost *= manager.biofilmExploiterCostMult;
        } 

        List<Unit> biofilmUnitsReached = FindBiofilmUnitsReached(range);

        foreach (Unit u in biofilmUnitsReached)
        {
            u.biofilms.Add(producer);
        }

        /*
        foreach (Unit u in biofilmUnitsReached)
        {
            if (u.viscosity < biofilmViscosity)
            {
                u.viscosity = biofilmViscosity;
                u.biofilmID = cellID;
                if (producer)
                {
                    u.biofilmExploiter = false;
                }
                else
                {
                    u.biofilmExploiter = true;
                }
            }
        }
        */

    }

    List<Unit> FindBiofilmUnitsReached(int range)
    {
        List<Unit> biofilmUnitsReached = new List<Unit>();

        for (int r = 0; r <= range; r++)
        {
            foreach (Unit u in manager.GetUnitsRange(unit, r))
            {
                biofilmUnitsReached.Add(u);
            }
        }

        return biofilmUnitsReached;
    }

    public void RemoveBiofilm()
    {

        int range = manager.biofilmRange;
        float biofilmViscosity = manager.biofilmViscosity;
        biofilmCost = 0;

        if (!producer)
        {
            biofilmViscosity *= manager.biofilmExploiterViscosityMult;
            biofilmCost *= manager.biofilmExploiterCostMult;
        }


        List<Unit> biofilmUnitsReached = FindBiofilmUnitsReached(range);


        foreach (Unit u in biofilmUnitsReached)
        {
            u.biofilms.Remove(producer);
        }

        /*
        foreach (Unit u in biofilmUnitsReached)
        {
            if (u.biofilmID == cellID)
            {
                Debug.Log(cellID);
                u.viscosity = 1f;
                u.biofilmID = -1;
                u.CheckNewBiofilm();
            }
        }

        */
    }

    #endregion

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        DoTimers();		
	}

}
