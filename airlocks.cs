List<DoorPair> doors = new List<DoorPair>();

const string prefix = "airlock_";
const string prefixA = "A_";
const string prefixB = "B_";

const string monitorPrefix = "[AIRLOCKS]";

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    List<IMyDoor> tempDoors = new List<IMyDoor>{};
    GridTerminalSystem.GetBlocksOfType<IMyDoor>(tempDoors);

    IMyDoor tempDoor;
    string tempName;
    foreach (IMyDoor d in tempDoors) {
        if (d.CustomName.StartsWith(prefix + prefixA)) {
            tempName = prefix + prefixB + d.CustomName.Substring(prefix.Length + prefixA.Length);
            tempDoor = GridTerminalSystem.GetBlockWithName(tempName) as IMyDoor;

            if (tempDoor == null) {
                continue;
            }

            doors.Add(new DoorPair(d, tempDoor));
            Echo("Pair added: " + d.CustomName + " and " + tempDoor.CustomName);
        }
    }
}

public void Main() {
    string monitorContent = "=== AIRLOCK STATUS ===\n";

    Echo("Running tick:");
    foreach (DoorPair dp in doors) {
        dp.RunUpdate();
        Echo("* " + dp.Status());
        monitorContent += dp.Status() + "\n";
    }
    Echo("[" + doors.Count + " airlocks]");
    Echo("[" + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount + " instructions]");
    monitorContent += "\n[" + doors.Count + " airlocks]\n";
    monitorContent += "[" + Runtime.CurrentInstructionCount + "/" + Runtime.MaxInstructionCount + " instructions]";

    List<IMyTextPanel> screens = new List<IMyTextPanel>{};
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(screens);
    foreach (IMyTextPanel s in screens) {
        if (s.CustomName.StartsWith(monitorPrefix)) {
            s.ShowPublicTextOnScreen();
            s.WritePublicText(monitorContent, false);
        }
    }
}

public class DoorPair {
    public IMyDoor door1, door2;
    private bool open = false;
    private DateTime openSince;

    public DoorPair(IMyDoor door1, IMyDoor door2) {
        this.door1 = door1;
        this.door2 = door2;
    }

    public string Status() {
        string status;
        if (Door1Open() || Door2Open()) {
            status = "OPEN  ";
        } else {
            status = "CLOSED";
        }

        return status + " : " + door1.CustomName + " - " + door2.CustomName;
    }

    private bool Door1Open() {
        return door1.Status != DoorStatus.Closed;
    }

    private bool Door2Open() {
        return door2.Status != DoorStatus.Closed;
    }

    public void RunUpdate() {
        if (open && DateTime.UtcNow.Subtract(openSince).TotalSeconds > 2) {
            door1.ApplyAction("Open_Off");
            door2.ApplyAction("Open_Off");
            open = false;
        }

        if (Door1Open() && !Door2Open()) {
            door2.ApplyAction("OnOff_Off");

            if (!open) {
                open = true;
                openSince = DateTime.UtcNow;
            }
        }

        if (Door2Open() && !Door1Open()) {
            door1.ApplyAction("OnOff_Off");

            if (!open) {
                open = true;
                openSince = DateTime.UtcNow;
            }
        }

        if (Door1Open() && Door2Open()) {
            door1.ApplyAction("Open_Off");
            door2.ApplyAction("Open_Off");
        }

        if (!Door1Open() && !Door2Open()) {
            door1.ApplyAction("OnOff_On");
            door2.ApplyAction("OnOff_On");
            open = false;
        }
    }
}