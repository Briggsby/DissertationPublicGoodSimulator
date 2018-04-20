using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class CSVDataRecorder : MonoBehaviour {

    public Manager manager;

    public List<string[]> csv = new List<string[]>();
    public List<string[]> allCSVResultsInAuto = new List<string[]>();
    public List<string[]> csvAnalysis = new List<string[]>();
    public List<string[]> csvOverallAnalysis = new List<string[]>();
    public List<string[]> csvOverallAnalysisSummary = new List<string[]>();
    public List<string[]> autoCSVCyclerFullResults = new List<string[]>();
    public List<string[]> autoCSVCyclerFullSummaries = new List<string[]>();

    #region Timers
    [Header("Timer")]
    public float csvTimer;
    public bool adjustTimeStep = true;
    public float csvTimeCounter;
    public bool currentlyWritingCSV = false;
    public float timeStableBy;


    void CSVTimers()
    {
        if (manager.paused)
        {
            return;
        }

        if (currentlyWritingCSV)
        {
            CSVWritingTimer();
        }
    }

    void CSVWritingTimer()
    {
        csvTimeCounter += Time.deltaTime * manager.timeStepPerSecond;

        if (csvTimeCounter <= csvTimer && adjustTimeStep)
        {
            manager.timeStepPerSecond++;
        }

        if (csvTimeCounter >= csvTimer)
        {
            if (adjustTimeStep)
            {
                manager.timeStepPerSecond--;
            }
            CSVWriteRow();
            csvTimeCounter = 0f;
        }
    }



    #endregion

    #region Saving


    void SaveCSV()
    {
        currentlyWritingCSV = false;
        Debug.Log("Saving CSV");
        string[][] csvOutput = ConvertCSVToOutputArray(csv);

        string filePath = GetCSVFilePath();

        SaveCSVOutput(csvOutput, filePath);
        SaveCSVAnalysis();

        if (doingAutoRepCSV)
        {
            DoAutoCSVRepetition();
        }
    }

    string[][] ConvertCSVToOutputArray(List<string[]> csvFile)
    {
        string[][] output = new string[csvFile.Count][];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = csvFile[i];
        }
        return output;

    }

    void SaveCSVOutput(string[][] output, string filePath)
    {
        int length = output.GetLength(0);
        string delimiter = ",";
        StringBuilder sb = new StringBuilder();
        for (int index = 0; index < length; index++)
        {
            sb.AppendLine(string.Join(delimiter, output[index]));
        }

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }


    #region FilePaths
    [Header("File Paths")]
    public string csvFileName;

    string GetCSVFilePath(bool analysis = false)
    {
        if (analysis)
        {
            if (!doingAutoRepCSV)
            {
                return Application.dataPath + "/CSV/" + csvFileName + "analysis" + ".csv";
            }
            else
            {
                return Application.dataPath + "/CSV/" + "/" + folderName + "/" + csvFileName + string.Format("rep{0}analysis", currentRepetition) + ".csv";
            }
        }


        if (!doingAutoRepCSV)
        {
            return Application.dataPath + "/CSV/" + csvFileName + ".csv";
        }
        else
        {
            return Application.dataPath + "/CSV/" + "/" + folderName + "/" + csvFileName + string.Format("rep{0}", currentRepetition) + ".csv";
        }
    }

    #endregion


    #endregion

    #region CSV Recordings
    public enum CSVRecordings {CellCount, NumberOfCellsProduced, ProducerCount, ProducerFrequency, HighestProducerCount, TimeAtHighestProdCount, LowestProducerCount, TimeAtLowestProdCount}
    [System.Serializable]
    public enum DefaultEntries
    {
        BoardSize, MutationRate, TypeOfStartEnvironment,
        FitnessTimeStep, CostToLive, FitnessRepThreshold,
        DirectFitnessTimer, DirectFitnessRange, DirectFitnessBenefit,
        DirectFitnessCost, DirectFitnessbcRatio, DirectFitnessExploitability, DoDirectFitness,
        DiffusionTimeStep, DiffusionConstG, DiffusionCutOff, DoDiffusion,
        ColorSecretionTimeStep, ColorSecreted, AmountColorSecreted, ColorSecretionCostPerTimeStep, DoColorSecretion,
        ColorMetabolismTimeStep, ColorMetNoUptake, ColorUptakeG, ColorUptakeB, ColorUptakeR, ColorFitnessPerUnitG, ColorFitnessPerUnitB, ColorFitnessPerUnitR, DoColorMetabolism,
        DoBiofilm, ExploitersDoBiofilms, BiofilmViscosity, BiofilmRange, BiofilmCost, BiofilmUptakeMultR,
        DoFlow, FlowOutOfBoard, FlowTimeStep, FlowProportionalToAmount, FlowAmountPerTimeStep,
        DoDegradation, DegradationReducedByBiofilm, DegradationTimeStep, DegradationProportional, DegradationAmountPerTimeStep,
        TimeTaken, CellID, HighestProducerCount, TimeAtHighestProdCount, LowestProducerCount, TimeAtLowestProdCount,
        HighestProducerCountAfterStable, TimeAtHighestProdCountAfterStable, LowestProducerCountAfterStable, TimeAtLowestProdCountAfterStable,
    }

    [Header("Variables To Record")]
    public List<CSVRecordings> csvRecordings;
    public DefaultEntries[] defaultDataEntries;
    public int defaultDataEntriesToMean;

    #region DefaultEntryReturns

    string ReturnCSVDefaultEntryTitle(DefaultEntries entry)
    {
        return entry.ToString();
    }

    string ReturnCSVDefaultEntryData(DefaultEntries dataEntry)
    {
        if (dataEntry == DefaultEntries.TimeTaken)
        {
            return string.Format("{0}", manager.timePassed - csvTimeStart);
        }
        else if (dataEntry == DefaultEntries.CellID)
        {
            return string.Format("{0}", manager.cellID);
        }
        else if (dataEntry == DefaultEntries.BoardSize)
        {
            return string.Format("{0}", manager.boardSize);
        }
        else if (dataEntry == DefaultEntries.MutationRate)
        {
            return string.Format("{0}", manager.mutationRate);
        }
        else if (dataEntry == DefaultEntries.TypeOfStartEnvironment)
        {
            return startSetting.ToString();
        }
        else if (dataEntry == DefaultEntries.HighestProducerCount)
        {
            return string.Format("{0}", manager.highestProducerCount);
        }
        else if (dataEntry == DefaultEntries.LowestProducerCount)
        {
            return string.Format("{0}", manager.lowestProducerCount);
        }
        else if (dataEntry == DefaultEntries.TimeAtHighestProdCount)
        {
            return string.Format("{0}", manager.timeAtHighestProdCount);
        }
        else if (dataEntry == DefaultEntries.TimeAtLowestProdCount)
        {
            return string.Format("{0}", manager.timeAtLowestProdCount);
        }
        else if (dataEntry == DefaultEntries.HighestProducerCountAfterStable)
        {
            return string.Format("{0}", manager.highestProducerCountAfterStable);
        }
        else if (dataEntry == DefaultEntries.LowestProducerCountAfterStable)
        {
            return string.Format("{0}", manager.lowestProducerCountAfterStable);
        }
        else if (dataEntry == DefaultEntries.TimeAtHighestProdCountAfterStable)
        {
            return string.Format("{0}", manager.timeAtHighestProdCountAfterStable);
        }
        else if (dataEntry == DefaultEntries.TimeAtLowestProdCountAfterStable)
        {
            return string.Format("{0}", manager.timeAtLowestProdCountAfterStable);
        }
        else if (dataEntry == DefaultEntries.FitnessTimeStep)
        {
            return string.Format("{0}", manager.fitnessTimeStep);
        }
        else if (dataEntry == DefaultEntries.CostToLive)
        {
            return string.Format("{0}", manager.costToLive);
        }
        else if (dataEntry == DefaultEntries.FitnessRepThreshold)
        {
            return string.Format("{0}", manager.fitnessRepThreshold);
        }
        else if (dataEntry == DefaultEntries.DirectFitnessTimer)
        {
            return string.Format("{0}", manager.directFitnessTimer);
        }
        else if (dataEntry == DefaultEntries.DirectFitnessRange)
        {
            return string.Format("{0}", manager.directFitnessRange);
        }
        else if (dataEntry == DefaultEntries.DirectFitnessBenefit)
        {
            return string.Format("{0}", manager.directFitnessBenefit);
        }
        else if (dataEntry == DefaultEntries.DirectFitnessCost)
        {
            return string.Format("{0}", manager.directFitnessCost);
        }
        else if (dataEntry == DefaultEntries.DirectFitnessbcRatio)
        {
            return string.Format("{0}", manager.directFitnessBenefit / manager.directFitnessCost);
        }
        else if (dataEntry == DefaultEntries.DirectFitnessExploitability)
        {
            return string.Format("{0}", manager.directFitnessExploitability);
        }
        else if (dataEntry == DefaultEntries.DoDirectFitness)
        {
            return string.Format("{0}", manager.doDirectFitness);
        }
        else if (dataEntry == DefaultEntries.DiffusionTimeStep)
        {
            return string.Format("{0}", manager.diffusionTimeStep);
        }
        else if (dataEntry == DefaultEntries.DiffusionConstG)
        {
            return string.Format("{0}", manager.diffusionConsts[1]);
        }
        else if (dataEntry == DefaultEntries.DiffusionCutOff)
        {
            return string.Format("{0}", manager.diffusionCutOff);
        }
        else if (dataEntry == DefaultEntries.DoDiffusion)
        {
            return string.Format("{0}", manager.doDiffusion);
        }
        else if (dataEntry == DefaultEntries.ColorSecretionTimeStep)
        {
            return string.Format("{0}", manager.colorSecretionTimer);
        }
        else if (dataEntry == DefaultEntries.ColorSecreted)
        {
            if (manager.colorSecreted == manager.red)
            {
                return "Red";
            }
            else if (manager.colorSecreted == manager.green)
            {
                return "Green";
            }
            else if (manager.colorSecreted == manager.blue)
            {
                return "Blue";
            }
            else
            {
                return "null";
            }
        }
        else if (dataEntry == DefaultEntries.AmountColorSecreted)
        {
            return string.Format("{0}", manager.amountColorSecreted);
        }
        else if (dataEntry == DefaultEntries.ColorSecretionCostPerTimeStep)
        {
            return string.Format("{0}", manager.colorSecretionCost);
        }
        else if (dataEntry == DefaultEntries.DoColorSecretion)
        {
            return string.Format("{0}", manager.doColorSecretion);
        }
        else if (dataEntry == DefaultEntries.ColorMetabolismTimeStep)
        {
            return string.Format("{0}", manager.colorMetabolismTimer);
        }
        else if (dataEntry == DefaultEntries.ColorMetNoUptake)
        {
            return string.Format("{0}", manager.colorMetabolismWithoutUptake);
        }
        else if (dataEntry == DefaultEntries.ColorUptakeG)
        {
            return string.Format("{0}", manager.colorUptake[1]);
        }
        else if (dataEntry == DefaultEntries.ColorUptakeB)
        {
            return string.Format("{0}", manager.colorUptake[2]);
        }
        else if (dataEntry == DefaultEntries.ColorUptakeR)
        {
            return string.Format("{0}", manager.colorUptake[0]);
        }
        else if (dataEntry == DefaultEntries.ColorFitnessPerUnitG)
        {
            return string.Format("{0}", manager.fitnessPerUnitColor[1]);
        }
        else if (dataEntry == DefaultEntries.ColorFitnessPerUnitB)
        {
            return string.Format("{0}", manager.fitnessPerUnitColor[2]);

        }
        else if (dataEntry == DefaultEntries.ColorFitnessPerUnitR)
        {
            return string.Format("{0}", manager.fitnessPerUnitColor[0]);
        }
        else if (dataEntry == DefaultEntries.DoColorMetabolism)
        {
            return string.Format("{0}", manager.doColorMetabolism);
        }
        else if (dataEntry == DefaultEntries.DoBiofilm)
        {
            return string.Format("{0}", manager.doBiofilms);
        }
        else if (dataEntry == DefaultEntries.ExploitersDoBiofilms)
        {
            return string.Format("{0}", manager.exploiterDoBiofilms);
        }
        else if (dataEntry == DefaultEntries.BiofilmViscosity)
        {
            return string.Format("{0}", manager.biofilmViscosity);
        }
        else if (dataEntry == DefaultEntries.BiofilmRange)
        {
            return string.Format("{0}", manager.biofilmRange);
        }
        else if (dataEntry == DefaultEntries.BiofilmCost)
        {
            return string.Format("{0}", manager.biofilmCost);
        }
        else if (dataEntry == DefaultEntries.BiofilmUptakeMultR)
        {
            return string.Format("{0}", manager.biofilmUptakeMults[0]);
        }
        else if (dataEntry == DefaultEntries.DoFlow)
        {
            return string.Format("{0}", manager.doFlow);
        }
        else if (dataEntry == DefaultEntries.FlowOutOfBoard)
        {
            return string.Format("{0}", manager.flowOutOfBoard);
        }
        else if (dataEntry == DefaultEntries.FlowTimeStep)
        {
            return string.Format("{0}", manager.flowTimeStep);
        }
        else if (dataEntry == DefaultEntries.FlowProportionalToAmount)
        {
            return string.Format("{0}", manager.flowProportionalToAmount);
        }
        else if (dataEntry == DefaultEntries.FlowAmountPerTimeStep)
        {
            return string.Format("{0}", manager.flowAmountPerTimeStep[1]);
        }
        else if (dataEntry == DefaultEntries.DoDegradation)
        {
            return string.Format("{0}", manager.doDegredation);
        }
        else if (dataEntry == DefaultEntries.DegradationReducedByBiofilm)
        {
            return string.Format("{0}", manager.degredationReducedByBiofilm);
        }
        else if (dataEntry == DefaultEntries.DegradationTimeStep)
        {
            return string.Format("{0}", manager.degredationTimeStep);
        }
        else if (dataEntry == DefaultEntries.DegradationProportional)
        {
            return string.Format("{0}", manager.degredationProportionalToAmount);
        }
        else if (dataEntry == DefaultEntries.DegradationAmountPerTimeStep)
        {
            return string.Format("{0}", manager.degredationAmountPerTimeStep[2]);
        }
        else
        {
            return "null";
        }
    }
    #endregion

    string[] CSVTitles()
    {
        string[] rowData = new string[csvRecordings.Count+1];

        rowData[0] = "TimePassed";

        for (int i = 0; i < csvRecordings.Count; i++)
        {
            rowData[i+1] = csvRecordings[i].ToString();
        }

        return rowData;

    }

    void CSVWriteRow()
    {
        string[] rowData = new string[csvRecordings.Count+1];

        rowData[0] = string.Format("{0}", manager.timePassed);

        for (int i = 0; i<csvRecordings.Count; i++)
        {
            rowData[i+1] = RecordDataPoint(csvRecordings[i]);
        }

        csv.Add(rowData);
    }

    string RecordDataPoint(CSVRecordings dataPoint)
    {
        if (dataPoint == CSVRecordings.CellCount)
        {
            return string.Format("{0}", manager.cellCount);
        }
        else if (dataPoint == CSVRecordings.ProducerCount)
        {
            return string.Format("{0}", manager.producerCount);
        }
        else if (dataPoint == CSVRecordings.ProducerFrequency)
        {
            return string.Format("{0}", (float)manager.producerCount/(float)manager.cellCount);
        }
        else if (dataPoint == CSVRecordings.NumberOfCellsProduced)
        {
            return string.Format("{0}", manager.cellID);
        }
        else if (dataPoint == CSVRecordings.HighestProducerCount)
        {
            return string.Format("{0}", manager.highestProducerCount);
        }
        else if (dataPoint == CSVRecordings.TimeAtHighestProdCount)
        {
            return string.Format("{0}", manager.timeAtHighestProdCount);
        }
        else if (dataPoint == CSVRecordings.LowestProducerCount)
        {
            return string.Format("{0}", manager.lowestProducerCount);
        }
        else if (dataPoint == CSVRecordings.TimeAtLowestProdCount)
        {
            return string.Format("{0}", manager.timeAtLowestProdCount);
        }
        else
        {
            return "0";
        }
    }

    #endregion

    #region CSV Controls

    void CSVControls()
    {
        if (Input.GetKeyDown(KeyCode.C) && !currentlyWritingCSV)
        {
            CSVCreation();
            return;
        }

        if (Input.GetKeyDown(KeyCode.C) && currentlyWritingCSV)
        {
            SaveCSV();
            return;
        }
        if (Input.GetKeyDown(KeyCode.U) && !currentlyWritingCSV && !doingAutoRepCSV)
        {
            StartAutoCSVRepetitions();
            return;
        }
        if (Input.GetKeyDown(KeyCode.U) && doingAutoRepCSV)
        {
            CancelAutoCSVRepetitions();
        }

        if (Input.GetKeyDown(KeyCode.Y) && !autoCSVCycling)
        {
            StartAutoCSVCycler();
        }
    }

    #endregion

    #region Initialization

    public float csvTimeStart;
    void CSVCreation()
    {
        Debug.Log("Starting CSV Creation");
        csv.Clear();
        csvTimeStart = manager.timePassed;
        csv.Add(CSVTitles());
        CSVWriteRow();
        currentlyWritingCSV = true;
        csvTimeCounter = 0f;
    }
    #endregion

    #region StartSettings

    public enum StartSettings { SingleProducer, AllProducers, AllExploiters, SmallSeedEach, LargeSeedEach, SeedOfExploitersInProducerPop, HalfAndHalf, HalfAndRedInterface}
    [Header("Start Setting")]
    public StartSettings startSetting = StartSettings.AllProducers;


    public void StartSettingsAutoCSV()
    {
        if (resetBoard)
        {
            manager.ResetBoard();
        }

        if (startSetting == StartSettings.AllProducers)
        {
            manager.producer = true;
            foreach (Unit u in manager.board)
            {
                if (u != null)
                {
                    manager.PlaceCell(u, true);
                }
            }
        }
        else if (startSetting == StartSettings.AllExploiters)
        {
            manager.producer = false;
            foreach (Unit u in manager.board)
            {
                if (u != null)
                {
                    manager.PlaceCell(u, true);
                }
            }
        }
        else if (startSetting == StartSettings.HalfAndHalf)
        {
            foreach (Unit u in manager.board)
            {
                if (u != null)
                {
                    manager.producer = true;
                    if (u.yCoordBoard > manager.boardSize)
                    {
                        manager.producer = false;
                    }
                    manager.PlaceCell(u, true);
                }
            }
        }
        else if (startSetting == StartSettings.HalfAndRedInterface)
        {
            foreach (Unit u in manager.board)
            {
                if (u != null)
                {
                    manager.producer = true;
                    if (u.yCoordBoard > manager.boardSize)
                    {
                        manager.producer = false;
                    }
                    if (u.xCoordBoard > manager.boardSize * 2)
                    {
                        u.rgb[0] = 999999999;
                        u.UpdateColor();
                    }
                    manager.PlaceCell(u, true);
                }
            }
        }
        else if (startSetting == StartSettings.SingleProducer)
        {
            manager.producer = true;
            manager.PlaceCell(manager.ConvertToBoardUnit(0f, 0f), true);
        }

        else if (startSetting == StartSettings.SmallSeedEach)
        {
            manager.producer = true;
            manager.ChooseRandomBoardLocationLeftSide().TryPlaceCellUnitsRange(2);
            manager.producer = false;
            manager.ChooseRandomBoardLocationRightSide().TryPlaceCellUnitsRange(2);
            
        }
        
        else if (startSetting == StartSettings.LargeSeedEach)
        {
            manager.producer = true;
            manager.ChooseRandomBoardLocationLeftSide().TryPlaceCellUnitsRange(8);
            manager.producer = false;
            manager.ChooseRandomBoardLocationRightSide().TryPlaceCellUnitsRange(8);
        }

        else if (startSetting == StartSettings.SeedOfExploitersInProducerPop)
        {
            manager.producer = true;
            foreach (Unit u in manager.board)
            {
                if (u != null)
                {
                    manager.PlaceCell(u, true);
                }
            }
            float leftOrRight = Random.Range(0f, 1f);
            manager.overwritingCell = true;
            manager.producer = false;
            if (leftOrRight < 0.5)
            {
                manager.ChooseRandomBoardLocationLeftSide().TryPlaceCellUnitsRange(2);
            }
            else
            {
                manager.ChooseRandomBoardLocationLeftSide().TryPlaceCellUnitsRange(2);
            }
        }
    }


    #endregion

    #region Auto CSVing

    [Header("Auto CSV Repeats")]
    public string folderName;
    public int repetitions;
    public float timeEachRep;

    public int currentRepetition = 1;
    public bool doingAutoRepCSV = false;
    public bool cancelAutoRepCSV = false;

    void StartAutoCSVRepetitions()
    {
        Debug.Log("Starting AutoCSV");
        cancelAutoRepCSV = false;
        csvOverallAnalysis.Clear();
        currentRepetition = 0;
        SetupAllCSVResultsInAuto();
        System.IO.Directory.CreateDirectory(Application.dataPath + "/CSV/" + folderName);
        doingAutoRepCSV = true;
        DoAutoCSVRepetition();


    }

    //NOT DONE
    #region AllCSVResults from Auto Reps in one file

    void SetupAllCSVResultsInAuto()
    {
    }

    void AddCSVToAllCSVResultsInAuto()
    {
    }

    void SaveAllCSVResultsInAuto()
    {
    }

    #endregion

    void DoAutoCSVRepetition()
    {
        if (cancelAutoRepCSV)
        {
            cancelAutoRepCSV = false;
            SaveAllCSVResultsInAuto();
            SaveAutoCSVOverallAnalysis();
            Debug.Log("Cancelling AutoCSV");
        }
        if (currentRepetition >= repetitions)
        {
            doingAutoRepCSV = false;
            SaveAllCSVResultsInAuto();
            SaveAutoCSVOverallAnalysis();
            Debug.Log("Finishing AutoCSV");

            if (autoCSVCycling)
            {
                NextAutoCSVCycle();
            }
        }
        else
        {
            AddCSVToAllCSVResultsInAuto();
            manager.Culling();
            currentRepetition++;
            StartSettingsAutoCSV();
            CSVCreation();
            manager.PauseToggle();
        }
    }

    void CancelAutoCSVRepetitions()
    {
        cancelAutoRepCSV = true;
        SaveCSV();
    }

    void CheckAutoCSVRepetitionFinished()
    {
        if (doingAutoRepCSV)
        {
            if (manager.timePassed > timeEachRep)
            {
                SaveCSV();
            }
        }
    }

    void SaveAutoCSVOverallAnalysis()
    {
        string[][] output = ConvertCSVToOutputArray(csvOverallAnalysis);

        string filePath = Application.dataPath + "/CSV/" + "/" + folderName + "/" + csvFileName + string.Format("OverallAnalysis", currentRepetition) + ".csv";

        SaveCSVOutput(output, filePath);

        SaveToAutoCSVCyclerFullResults();
        SaveAutoCSVOverallAnalysisSummary();
    }

    #endregion

    #region Auto CSV Cycler

    public enum AutoCSVCycleVariable {  BoardSize, PublicGoodSetting,
                                        DirectFitnessExploitability, DirectFitnessRange, DirectFitnessBenefit,
                                        DiffusionConstG, AmountColorSecreted, FitnessPerUnitG, ColorSecretionCost,
                                        CostToLive, MutationRate,
                                        BiofilmViscosity, ExploiterBiofilms,
                                        FlowAmount,
                                        DegradationAmountB
                                    }

    public enum AutoPublicGoodSetting { DF, PM, SM }
    private int currentPublicGoodSetting = 0;
    [Header("AutoCSVCycler")]
    public string autoCSVCyclerAnalysisFileName;
    public string autoCSVCyclerFolder;
    public AutoCSVCycleVariable[] autoCSVCycleVariables;
    public float[] autoCSVCycleIncrements;
    public float[] autoCSVCycleStart;
    public float[] autoCSVCycleStops;

    private bool resetBoard = false;

    public bool autoCSVCycling = false;
    public int currentIndexCycling;

    void ConvertAutoPublicGoodSettingToSettings(int autoPublicGoodSetting)
    {
        currentPublicGoodSetting = autoPublicGoodSetting;
        if (autoPublicGoodSetting == (int)AutoPublicGoodSetting.DF)
        {
            SetUpDFPublicGood();
        }
        else if (autoPublicGoodSetting == (int)AutoPublicGoodSetting.PM)
        {
            SetUpPMPublicGood();
        }
        else if (autoPublicGoodSetting == (int)AutoPublicGoodSetting.SM)
        {
            SetUpSMPublicGood();
        }
    }

    void SetUpDFPublicGood()
    {
        manager.doDirectFitness = true;
        manager.doColorSecretion = false;
    }

    void SetUpPMPublicGood()
    {
        manager.doDirectFitness = false;
        manager.doColorSecretion = true;
        manager.colorSecreted = 1;
        manager.colorMetabolismWithoutUptake = false;
        manager.doDegredation = false;
    }

    void SetUpSMPublicGood()
    {
        manager.doDirectFitness = false;
        manager.doColorSecretion = true;
        manager.colorSecreted = 2;
        manager.colorMetabolismWithoutUptake = true;
        manager.doDegredation = true;
    }

    void StartAutoCSVCycler()
    {
        Debug.Log("Starting Auto CSV Cycler");

        System.IO.Directory.CreateDirectory(Application.dataPath + "/CSV/" + autoCSVCyclerFolder);

        for (int i = 0; i<autoCSVCycleVariables.Length; i++)
        {
            if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.BoardSize)
            {
                resetBoard = true;
                manager.boardSize = (int)autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DirectFitnessExploitability)
            {
                manager.directFitnessExploitability = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DirectFitnessRange)
            {
                manager.directFitnessRange = (int)autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DiffusionConstG)
            {
                manager.diffusionConsts[1] = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.AmountColorSecreted)
            {
                manager.amountColorSecreted = (long)autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.FitnessPerUnitG)
            {
                manager.fitnessPerUnitColor[1] = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DirectFitnessBenefit)
            {
                manager.directFitnessBenefit = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.ColorSecretionCost)
            {
                manager.colorSecretionCost = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.CostToLive)
            {
                manager.costToLive = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.MutationRate)
            {
                manager.mutationRate = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.ExploiterBiofilms)
            {
                if (autoCSVCycleStart[i] == 1)
                {
                    manager.exploiterDoBiofilms = true;
                }
                else
                {
                    manager.exploiterDoBiofilms = false;
                }
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.PublicGoodSetting)
            {
                ConvertAutoPublicGoodSettingToSettings((int)autoCSVCycleStart[i]);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.BiofilmViscosity)
            {
                manager.biofilmViscosity = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.FlowAmount)
            {
                manager.flowAmountPerTimeStep[0] = autoCSVCycleStart[i];
                manager.flowAmountPerTimeStep[1] = autoCSVCycleStart[i];
                manager.flowAmountPerTimeStep[2] = autoCSVCycleStart[i];
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DegradationAmountB)
            {
                manager.degredationAmountPerTimeStep[2] = autoCSVCycleStart[i];
            }
        }

        autoCSVCyclerFullResults.Clear();
        autoCSVCyclerFullSummaries.Clear();

        autoCSVCycling = true;
        ResetCurrentIndexCycle();
        AutoCSVCycle();

    }

    void AutoCSVCycle()
    {

        folderName = GetAutoCSVCycleInCyclerFolderName();
        StartAutoCSVRepetitions();
    }

    string GetAutoCSVCycleInCyclerFolderName()
    {
        string name = autoCSVCyclerFolder + "/";
        for (int i = 0; i < autoCSVCycleVariables.Length; i++)
        {
            if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.BoardSize)
            {
                name += string.Format("BoardSize{0}", manager.boardSize);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DirectFitnessExploitability)
            {
                name += string.Format("DFExp{0}", manager.directFitnessExploitability);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DirectFitnessRange)
            {
                name += string.Format("DFRange{0}", manager.directFitnessRange);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DiffusionConstG)
            {
                name += string.Format("DiffConstG{0}", manager.diffusionConsts[1]);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.AmountColorSecreted)
            {
                name += string.Format("GSec{0}", manager.amountColorSecreted);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.FitnessPerUnitG)
            {
                name += string.Format("FitPerG{0}", manager.fitnessPerUnitColor[1]);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.ColorSecretionCost)
            {
                name += string.Format("SecCost{0}", manager.colorSecretionCost);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DirectFitnessBenefit)
            {
                name += string.Format("DFBen{0}", manager.directFitnessBenefit);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.CostToLive)
            {
                name += string.Format("CostToLive{0}", manager.costToLive);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.MutationRate)
            {
                name += string.Format("MutationRate{0}", manager.mutationRate);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.BiofilmViscosity)
            {
                name += string.Format("BiofilmVisc{0}", manager.biofilmViscosity);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.ExploiterBiofilms)
            {
                name += string.Format("ExploitersDoBiofilm{0}", manager.exploiterDoBiofilms);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.PublicGoodSetting)
            {
                string nameAdd = "";
                if (manager.doDirectFitness)
                {
                    nameAdd = "DF";
                }
                else if (manager.doColorSecretion)
                {
                    if (manager.colorMetabolismWithoutUptake)
                    {
                        nameAdd = "SM";
                    }
                    else
                    {
                        nameAdd = "PM";
                    }
                }
                name += string.Format(nameAdd);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.FlowAmount)
            {
                name += string.Format("Flow{0}", manager.flowAmountPerTimeStep[0]);
            }
            else if (autoCSVCycleVariables[i] == AutoCSVCycleVariable.DegradationAmountB)
            {
                name += string.Format("DegB{0}", manager.degredationAmountPerTimeStep[2]);
            }
            else
            {
                name += "NULL";
            }
        }

        return name;
    }

    void ResetCurrentIndexCycle() {

        currentIndexCycling = autoCSVCycleVariables.Length - 1;

    }

    void NextAutoCSVCycle()
    {
        bool endOfIndexCycle = false;

        if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.DirectFitnessExploitability)
        {
            if (manager.directFitnessExploitability >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.directFitnessExploitability += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.BoardSize)
        {
            if (manager.boardSize >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.boardSize += (int)autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.DirectFitnessRange)
        {
            if (manager.directFitnessRange >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.directFitnessRange += (int)autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.DiffusionConstG)
        {
            if (manager.diffusionConsts[1] >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.diffusionConsts[1] += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.AmountColorSecreted)
        {
            if (manager.amountColorSecreted >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.amountColorSecreted += (long)autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.FitnessPerUnitG)
        {
            if (manager.fitnessPerUnitColor[1] >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.fitnessPerUnitColor[1] += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.ColorSecretionCost)
        {
            if (manager.colorSecretionCost >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.colorSecretionCost += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.DirectFitnessBenefit)
        {
            if (manager.directFitnessBenefit >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.directFitnessBenefit += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.CostToLive)
        {
            if (manager.costToLive >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.costToLive += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.MutationRate)
        {
            if (manager.mutationRate >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.mutationRate += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.BiofilmViscosity)
        {
            if (manager.biofilmViscosity >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.biofilmViscosity += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.ExploiterBiofilms)
        {
            if (manager.exploiterDoBiofilms == true && autoCSVCycleStops[currentIndexCycling] == 1f)
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.exploiterDoBiofilms = !manager.exploiterDoBiofilms;
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.PublicGoodSetting)
        {
            if (currentPublicGoodSetting >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                currentPublicGoodSetting += (int)autoCSVCycleIncrements[currentIndexCycling];
                ConvertAutoPublicGoodSettingToSettings(currentPublicGoodSetting);
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.FlowAmount)
        {
            if (manager.flowAmountPerTimeStep[0] >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.flowAmountPerTimeStep[0] += autoCSVCycleIncrements[currentIndexCycling];
                manager.flowAmountPerTimeStep[1] += autoCSVCycleIncrements[currentIndexCycling];
                manager.flowAmountPerTimeStep[2] += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }
        else if (autoCSVCycleVariables[currentIndexCycling] == AutoCSVCycleVariable.DegradationAmountB)
        {
            if (manager.degredationAmountPerTimeStep[2] >= autoCSVCycleStops[currentIndexCycling])
            {
                endOfIndexCycle = true;
            }
            else
            {
                manager.degredationAmountPerTimeStep[2] += autoCSVCycleIncrements[currentIndexCycling];
                ResetCurrentIndexCycle();
                AutoCSVCycle();
            }
        }

        if (endOfIndexCycle)
        {
            if (currentIndexCycling == 0)
            {
                FinishAutoCSVCycler();
            }
            else
            {
                ResetIndexToStart(currentIndexCycling);
                currentIndexCycling--;
                NextAutoCSVCycle();
                return;
            }
        }

    }

    void ResetIndexToStart(int index)
    {
        if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.DirectFitnessExploitability)
        {
            manager.directFitnessExploitability = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.BoardSize)
        {
            manager.boardSize = (int)autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.DirectFitnessRange)
        {
            manager.directFitnessRange = (int)autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.DiffusionConstG)
        {
            manager.diffusionConsts[1] = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.AmountColorSecreted)
        {
            manager.amountColorSecreted = (long)autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.FitnessPerUnitG)
        {
            manager.fitnessPerUnitColor[1] = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.ColorSecretionCost)
        {
            manager.colorSecretionCost = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.DirectFitnessBenefit)
        {
            manager.directFitnessBenefit = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.CostToLive)
        {
            manager.costToLive = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.MutationRate)
        {
            manager.mutationRate = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.BiofilmViscosity)
        {
            manager.biofilmViscosity = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.ExploiterBiofilms)
        {
            if (autoCSVCycleStart[index] == 1)
            {
                manager.exploiterDoBiofilms = true;
            }
            else
            {
                manager.exploiterDoBiofilms = false;
            }
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.PublicGoodSetting)
        {
            ConvertAutoPublicGoodSettingToSettings((int)autoCSVCycleStart[index]);
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.FlowAmount)
        {
            manager.flowAmountPerTimeStep[0] = autoCSVCycleStart[index];
            manager.flowAmountPerTimeStep[1] = autoCSVCycleStart[index];
            manager.flowAmountPerTimeStep[2] = autoCSVCycleStart[index];
        }
        else if (autoCSVCycleVariables[index] == AutoCSVCycleVariable.DegradationAmountB)
        {
            manager.degredationAmountPerTimeStep[2] = autoCSVCycleStart[index];
        }
    }

    void FinishAutoCSVCycler()
    {
        Debug.Log("Finished Auto CSV Cycler");
        SaveAutoCSVCyclerFullSummaries();
        SaveAutoCSVCyclerFullResults();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    void SaveAutoCSVCyclerFullSummaries()
    {
        string[][] csvOutput = ConvertCSVToOutputArray(autoCSVCyclerFullSummaries);
        string filePath = Application.dataPath + "/CSV/" + autoCSVCyclerFolder + "/" + autoCSVCyclerAnalysisFileName + "FullSummaries" + ".csv";
        SaveCSVOutput(csvOutput, filePath);

    }

    void SaveToAutoCSVCyclerFullSummaries()
    {
        if (!autoCSVCycling)
        {
            return;
        }

        if (autoCSVCyclerFullSummaries.Count == 0)
        {
            autoCSVCyclerFullSummaries.Add(csvOverallAnalysisSummary[0]);
        }
        autoCSVCyclerFullSummaries.Add(csvOverallAnalysisSummary[1]);
    }

    void SaveToAutoCSVCyclerFullResults()
    {
        if (autoCSVCyclerFullResults.Count == 0)
        {
            autoCSVCyclerFullResults.Add(csvOverallAnalysis[0]);
        }
        for (int i = 1; i < csvOverallAnalysis.Count; i++)
        {
            autoCSVCyclerFullResults.Add(csvOverallAnalysis[i]);
        }
    }

    void SaveAutoCSVCyclerFullResults()
    {
        string[][] csvOutput = ConvertCSVToOutputArray(autoCSVCyclerFullResults);
        string filePath = Application.dataPath + "/CSV/" + autoCSVCyclerFolder + "/" + autoCSVCyclerAnalysisFileName + "FullResults" + ".csv";
        SaveCSVOutput(csvOutput, filePath);
    }

    #endregion

    #region Analysis

    void SaveCSVAnalysis()
    {
        csvAnalysis.Clear();

        csvAnalysis.Add(AnalysisTitles());

        if (csvOverallAnalysis.Count == 0)
        {
            csvOverallAnalysis.Add(AnalysisTitles());
        }

        csvAnalysis.Add(CSVAnalysisData());

        csvOverallAnalysis.Add(csvAnalysis[csvAnalysis.Count - 1]);

        SaveCSVAnalysisFile(csvAnalysis);
    }

    void SaveCSVAnalysisFile(List<string[]> csvAnalysisFile)
    {

        string[][] csvOutput = ConvertCSVToOutputArray(csvAnalysisFile);

        string filePath = GetCSVFilePath(true);

        SaveCSVOutput(csvOutput, filePath);

    }

    string[] CSVAnalysisData()
    {
        int numberOfDataPoints = defaultDataEntries.Length + (4 * csvRecordings.Count);
        string[] rowData = new string[numberOfDataPoints];

        for (int i = 0; i<defaultDataEntries.Length; i++)
        {
            rowData[i] = ReturnCSVDefaultEntryData(defaultDataEntries[i]);
        }

        int index = 1;
        for (int i = defaultDataEntries.Length; i < numberOfDataPoints; i += 4)
        {
            rowData[i] = CalculateMean(csv, index);
            rowData[i + 1] = CalculateVariance(csv, index);
            rowData[i + 2] = CalculateMean(csv, index, timeStableBy);
            rowData[i + 3] = CalculateVariance(csv, index, timeStableBy);
            index++;
        }

        return rowData;

    }

    string[] AnalysisTitles()
    {
        int numberOfTitles = defaultDataEntries.Length + (4 * csvRecordings.Count);
        string[] rowData = new string[numberOfTitles];

        for (int i = 0; i < defaultDataEntries.Length; i++)
        {
            rowData[i] = ReturnCSVDefaultEntryTitle(defaultDataEntries[i]);
        }

        int index = 0;
        for (int i = defaultDataEntries.Length; i<numberOfTitles; i+=4)
        {
            rowData[i] = csvRecordings[index].ToString()+"mean";
            rowData[i + 1] = csvRecordings[index].ToString() + "variance" ;
            rowData[i + 2] = csvRecordings[index].ToString() + "mean" + string.Format("Past{0}",timeStableBy);
            rowData[i + 3] = csvRecordings[index].ToString() + "variance" + string.Format("Past{0}", timeStableBy);
            index++;
        }

        return rowData;

    }

    string CalculateMean(List<string[]> csvFile, int dataPointIndex, float pastWhatTime = 0f)
    {
        float value = 0f;
        int count = 0;
        for (int i = 1; i<csvFile.Count; i++)
        {
            if (float.Parse(csvFile[i][0]) >= pastWhatTime)
            {
                value += float.Parse(csvFile[i][dataPointIndex]);
                count++;
            }
        }

        return string.Format("{0}", value / count);
    }

    string CalculateVariance(List<string[]> csvFile, int dataPointIndex, float pastWhatTime = 0f)
    {
        float mean = float.Parse(CalculateMean(csvFile, dataPointIndex, pastWhatTime));
        float variance = 0f;
        int count = 0;

        for (int i = 1; i < csvFile.Count; i++)
        {
            if (float.Parse(csvFile[i][0]) >= pastWhatTime)
            {
                float dataMinusMean = float.Parse(csvFile[i][dataPointIndex]) - mean;
                variance += dataMinusMean*dataMinusMean;
                count++;
            }
        }

        return string.Format("{0}", variance / count);


    }

    #region OverallAnalysisSummary

    void SaveAutoCSVOverallAnalysisSummary()
    {
        csvOverallAnalysisSummary.Clear();

        csvOverallAnalysisSummary.Add(OverallAnalysisSummaryTitles());

        csvOverallAnalysisSummary.Add(OverallAnalysisSummaryData());

        string[][] csvOutput = ConvertCSVToOutputArray(csvOverallAnalysisSummary);
        string filePath = GetOverallAnalysisSummaryFilePath();
        SaveCSVOutput(csvOutput, filePath);

        SaveToAutoCSVCyclerFullSummaries();
    }


    string GetOverallAnalysisSummaryFilePath()
    {
        return Application.dataPath + "/CSV/" + "/" + folderName + "/" + csvFileName + string.Format("OverallAnalysisSummary", currentRepetition) + ".csv";
    }

    string[] OverallAnalysisSummaryTitles()
    {
        int numberOfTitles = (defaultDataEntries.Length - defaultDataEntriesToMean) + (2*defaultDataEntriesToMean) + (4 * csvRecordings.Count);
        string[] rowData = new string[numberOfTitles];

        for (int i = 0; i < defaultDataEntries.Length - defaultDataEntriesToMean; i++)
        {
            rowData[i] = ReturnCSVDefaultEntryTitle(defaultDataEntries[i]);
        }

        int index = defaultDataEntries.Length - defaultDataEntriesToMean;
        for (int i = defaultDataEntries.Length - defaultDataEntriesToMean; i < numberOfTitles - (4 * csvRecordings.Count); i += 2)
        {
            rowData[i] = defaultDataEntries[index].ToString() + "mean";
            rowData[i + 1] = defaultDataEntries[index].ToString() + "variance";
            index++;
        }


        index = 0;
        for (int i = (defaultDataEntries.Length - defaultDataEntriesToMean) + (2 * defaultDataEntriesToMean); i < numberOfTitles; i += 4)
        {
            rowData[i] = csvRecordings[index].ToString() + "mean";
            rowData[i + 1] = csvRecordings[index].ToString() + "variance";
            rowData[i + 2] = csvRecordings[index].ToString() + "mean" + string.Format("Past{0}", timeStableBy);
            rowData[i + 3] = csvRecordings[index].ToString() + "variance" + string.Format("Past{0}", timeStableBy);
            index ++;
        }

        return rowData;
    }

    string[] OverallAnalysisSummaryData()
    {
        int numberOfDataPoints = (defaultDataEntries.Length - defaultDataEntriesToMean) + (2 * defaultDataEntriesToMean) + (4 * csvRecordings.Count);
        string[] rowData = new string[numberOfDataPoints];

        for (int i = 0; i < defaultDataEntries.Length-defaultDataEntriesToMean; i++)
        {
            rowData[i] = ReturnCSVDefaultEntryData(defaultDataEntries[i]);
        }

        int index = defaultDataEntries.Length - defaultDataEntriesToMean;
        for (int i = defaultDataEntries.Length - defaultDataEntriesToMean; i < numberOfDataPoints - (4 * csvRecordings.Count); i += 2)
        {
            rowData[i] = CalculateMean(csvOverallAnalysis, index);
            rowData[i + 1] = CalculateVariance(csvOverallAnalysis,index);
            index++;
        }

        index = defaultDataEntries.Length;
        for (int i = (defaultDataEntries.Length - defaultDataEntriesToMean) + (2 * defaultDataEntriesToMean); i < numberOfDataPoints; i += 4)
        {
            rowData[i] = CalculateMean(csvOverallAnalysis, index);
            rowData[i + 1] = CalculateVariance(csvOverallAnalysis, index);
            rowData[i + 2] = CalculateMean(csvOverallAnalysis, index+2);
            rowData[i + 3] = CalculateVariance(csvOverallAnalysis, index+2);
            index += 4;
        }

        return rowData;
    }



    #endregion

    #endregion

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        CSVControls();
        CSVTimers();
        CheckAutoCSVRepetitionFinished();
	}



}
