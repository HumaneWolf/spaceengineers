List<IMyAirVent> vents = new List<IMyAirVent>();
List<IMyTextPanel> screens = new List<IMyTextPanel>{};

const string monitorSuffix = "[OXYGEN]";

StringBuilder monitorContent = new StringBuilder();

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    GridTerminalSystem.GetBlocksOfType<IMyAirVent>(vents);

    List<IMyTextPanel> tempScreens = new List<IMyTextPanel>{};
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(tempScreens);
    foreach (IMyTextPanel s in tempScreens) {
        if (s.CustomName.EndsWith(monitorSuffix)) {
            screens.Add(s);
        }
    }
}

public void Main() {
    monitorContent.Append("=== OXYGEN VENT STATUS ===\n");

    foreach (IMyAirVent vent in vents) {
        monitorContent.Append("* ");
        monitorContent.Append(vent.CustomName.Replace(" Air Vent", ""));
        monitorContent.Append(" - ");
        monitorContent.Append(vent.Status.ToString());
        monitorContent.Append("\n");
    }

    Echo("[" + vents.Count + " vents]");
    Echo("[" + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount + " instructions]");
    monitorContent.Append("\n\n[" + vents.Count + " vents]\n");
    monitorContent.Append("[" + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount + " instructions]");

    foreach (IMyTextPanel s in screens) {
        s.ContentType = ContentType.TEXT_AND_IMAGE;
        s.WriteText(monitorContent, false);
    }
    monitorContent.Clear();
}


float SimpleRound(float input) {
    float roundNum = 10.0f;
    return MathHelper.RoundToInt(input * roundNum) / roundNum;
}
