
private List<string> s_CythonShieldSubtypeIds = new List<string>()
{
    "SmallShipSmallShieldGeneratorBase",
    "SmallShipMicroShieldGeneratorBase",
    "LargeShipSmallShieldGeneratorBase",
    "LargeShipLargeShieldGeneratorBase"
};
System.Text.RegularExpressions.Regex s_QSIRegex = new System.Text.RegularExpressions.Regex(":*.\\([^)]*\\)");

int m_QSIUpdateCount = 0;

const string s_QSIVersion = "v1.0";
//const string s_QSITag = "QSIRequest";

string m_QSIEchoString = "";
//string m_QSILogString = "";
System.Text.RegularExpressions.Match m_QSIShieldInfoMatch = null;
List<IMyTerminalBlock> m_QSIShields = new List<IMyTerminalBlock>();
IMyTerminalBlock m_QSIMyShield = null;
//IMyRadioAntenna m_QSIMyAntenna = null;

public void Init_QuickShieldInfo()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    Echo += (string _text) =>
    {
        //m_CAPLogString += _text;
    };
}

public void Run_QuickShieldInfo()
{
    m_QSIEchoString = "";
    //m_QSILogString = "";
    m_QSIShields.Clear();

    Sub();

    Echo(m_QSIEchoString);
}

private void Sub()
{
    m_QSIEchoString += "Grid '" + Me.CubeGrid.DisplayName + "': \n";
    if (m_QSIMyShield == null)
    {
        GridTerminalSystem.GetBlocksOfType(m_QSIShields, (IMyTerminalBlock _block) =>
        {
            return (Me.IsSameConstructAs(_block) && s_CythonShieldSubtypeIds.Contains(_block.BlockDefinition.SubtypeId));
        });
        m_QSIEchoString += "  Shield found: " + m_QSIShields.Count + "\n";

        if (m_QSIShields.Count <= 0)
        {
            m_QSIEchoString += "  No Shield Generator found on grid " + Me.CubeGrid.DisplayName + "\n";
            return;
        }

        m_QSIMyShield = m_QSIShields[0];
    }

    if (!m_QSIMyShield.CustomName.EndsWith(":") && !m_QSIMyShield.CustomName.Contains(":"))
    {
        m_QSIMyShield.CustomName += ":";
        return;
    }

    m_QSIShieldInfoMatch = s_QSIRegex.Match(m_QSIMyShield.CustomName);
    if (m_QSIShieldInfoMatch.Length <= 0)
    {
        m_QSIEchoString += "  Captured block '" + m_QSIMyShield.CustomName + "' is not a Shield Generator.\n";
        return;
    }

    string infoStrip = m_QSIShieldInfoMatch.Value;
    infoStrip = infoStrip.Substring(3, infoStrip.Length - 4);
    string[] infos = infoStrip.Split('/');
    long curE = 0;
    long maxE = 0;
    if (!long.TryParse(infos[0], out curE) || !long.TryParse(infos[1], out maxE))
    {
        m_QSIEchoString += "  Cannot parse shield info '" + infoStrip + "'.\n";
        return;
    }

    m_QSIEchoString += string.Format(System.Globalization.CultureInfo.InvariantCulture, "  {0:#,#}/{1:#,#}\n", curE, maxE);
    
    m_QSIEchoString += string.Format("\nLast runtime: {0:F}\nCurrent Instruction Count: {1:0}", Runtime.LastRunTimeMs, Runtime.CurrentInstructionCount);
}

public Program()
{
    Init_QuickShieldInfo();
}

public void Main(string argument, UpdateType updateSource)
{
    ++m_QSIUpdateCount;
    Echo("Update Count: " + m_QSIUpdateCount);

    Run_QuickShieldInfo();
}