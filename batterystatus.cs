List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
List<IMyTextPanel> screens = new List<IMyTextPanel>{};

const string monitorSuffix = "[BATTERY]";

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    List<IMyBatteryBlock> tempBatteries = new List<IMyBatteryBlock>{};
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(tempBatteries);

    foreach (IMyBatteryBlock b in tempBatteries) {
        if (!b.CustomName.StartsWith("Small ")) {  // Ignore the ones on the small ship, if it's attached when we init htis.
            batteries.Add(b);
            Echo("Battery added: " + b.CustomName);
        }
    }

    List<IMyTextPanel> tempScreens = new List<IMyTextPanel>{};
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(tempScreens);
    foreach (IMyTextPanel s in tempScreens) {
        if (s.CustomName.EndsWith(monitorSuffix)) {
            screens.Add(s);
        }
    }
}

public void Main() {
    string monitorContent = "=== BATTERY STATUS ===\n";

    float minPower = 99999999999999; // Set to a value larger than any battery box.
    float avgPower = 0;
    float maxPower = 0;

    float totalPower = 0;
    float totalCapacity = 0;

    float totalInput = 0;
    float totalOutput = 0;

    foreach (IMyBatteryBlock bat in batteries) {
        if (bat.CurrentStoredPower < minPower) {
            minPower = bat.CurrentStoredPower;
        }

        if (bat.CurrentStoredPower > maxPower) {
            maxPower = bat.CurrentStoredPower;
        }

        avgPower += bat.CurrentStoredPower;
        totalPower += bat.CurrentStoredPower;
        totalCapacity += bat.MaxStoredPower;

        totalInput += bat.CurrentInput;
        totalOutput += bat.CurrentOutput;
    }

    // Finish some calculations
    avgPower = avgPower / batteries.Count;
    float percentFull = (totalPower / totalCapacity) * 100;

    // Calculate how long until fully charged / empty
    float powerSituation = (totalInput - totalOutput);
    int hours;
    if (powerSituation < 0) { // If
        hours = MathHelper.RoundToInt(totalPower / -powerSituation);
    } else {
        hours = MathHelper.RoundToInt((totalCapacity - totalPower) / powerSituation);
    }

    // Print
    monitorContent += "POWER STORED:\n";
    monitorContent += "* Min:    " + SimpleRound(minPower) + " MWh\n* Avg:    " + SimpleRound(avgPower) + " MWh\n* Max:    " + SimpleRound(maxPower) + " MWh\n";
    monitorContent += "Total:    " + SimpleRound(totalPower) + " / " + SimpleRound(totalCapacity) + " MWh\n";
    monitorContent += SimpleRound(percentFull) + " % full\n\n";
    monitorContent += "Current Input:  " + SimpleRound(totalInput) + " MW\n";
    monitorContent += "Current Output: " + SimpleRound(totalOutput) + " MW\n\n";

    if (powerSituation < 0) {
        monitorContent += hours + " hours until empty.\n";
    } else {
        monitorContent += hours + " hours until fully charged.\n";
    }

    Echo("[" + batteries.Count + " batteries]");
    Echo("[" + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount + " instructions]");
    monitorContent += "\n[" + batteries.Count + " batteries]\n";
    monitorContent += "[" + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount + " instructions]";

    foreach (IMyTextPanel s in screens) {
        s.ContentType = ContentType.TEXT_AND_IMAGE;
        s.WriteText(monitorContent, false);
    }
}

float SimpleRound(float input) {
    float roundNum = 1000.0f;
    return MathHelper.RoundToInt(input * roundNum) / roundNum;
}
