/*
 * R e a d m e
 * -----------
 * 
 * In this file you can include any instructions or other comments you want to have injected onto the 
 * top of your final script. You can safely delete this file if you do not want any such comments.
 */

//MyItemType s_CannonItemType = MyItemType.Parse("MyObjectBuilder_AmmoMagazine/Inky35mmShell");

//const int s_MaxAmmoCount = 500;

int m_UpdateCount = 0;
//int s_TotalAmmo = 0;
//List<IMyTerminalBlock> cargos;
//List<IMyTerminalBlock> cannons;
//List<IMyTextPanel> panels;
//IMyTextPanel bridgePanel;
//IMyTextPanel smallPanel;

const string s_StartString = "=== CannonAmmoPuller ===";
const string s_EndString = "=== End ===";
string m_EchoString = "";
string m_CustomdataString = "";
string m_LogString = "";

List<IMyTerminalBlock> m_Cannons = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> m_Cargos = new List<IMyTerminalBlock>();

public void Init_CannonAmmoPuller()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    m_CustomdataString = Me.CustomData;
    int starts = m_CustomdataString.IndexOf(s_StartString);
    if (starts == -1)
    {
        m_CustomdataString += s_StartString + "\n";
    }
    else
    {
        m_CustomdataString = Me.CustomData.Substring(starts + s_StartString.Length);
        int ends = m_CustomdataString.IndexOf(s_EndString);
        if (ends == -1)
        {
            m_CustomdataString += s_EndString + "\n";
        }
        else
        {
            m_CustomdataString = m_CustomdataString.Remove(ends);
        }
    }



}

public void Run_CannonAmmoPuller()
{
    m_Cannons.Clear();
    m_Cargos.Clear();
    m_EchoString = "";
    m_LogString = "";
    m_CustomdataString = Me.CustomData;
    int starts = m_CustomdataString.IndexOf(s_StartString);
    if (starts == -1)
    {
        m_CustomdataString += s_StartString + "\n";
    }
    else
    {
        m_CustomdataString = Me.CustomData.Substring(starts + s_StartString.Length);
        int ends = m_CustomdataString.IndexOf(s_EndString);
        if (ends == -1)
        {
            m_CustomdataString += s_EndString + "\n";
        }
        else
        {
            m_CustomdataString = m_CustomdataString.Remove(ends);
        }
    }

    if (Me.TerminalRunArgument.StartsWith("getid"))
    {
        RunGetidMode();
    }
    else if (Me.TerminalRunArgument.StartsWith("purge"))
    {
        RunPurgeMode();
    }
    else
    {
        RunAmmoPullMode();
    }

    // TODO: print diag info;
    m_EchoString += "\n";
    Echo(m_EchoString);
}

private void RunGetidMode()
{
    m_EchoString += "=== CannonAmmoPuller - GETID mode ===\n";

    string argsString = Me.TerminalRunArgument.Remove(0, 5).Trim();
    string[] names = argsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    m_EchoString += "Found " + names.Length + " request(s).\n";

    IMyTerminalBlock block = null;
    List<MyItemType> items = new List<MyItemType>();
    foreach (string name in names)
    {
        block = GridTerminalSystem.GetBlockWithName(name);
        if (block == null)
        {
            m_EchoString += "No block with name " + name + ".\n";
            continue;
        }
        //if (!Me.IsSameConstructAs(block))
        //{
        //    echoString += "Block " + name + " is on different grid;
        //    continue;
        //}
        if (!block.HasInventory)
        {
            m_EchoString += "Block " + name + " has no inventory.\n";
            continue;
        }

        m_EchoString += "gun = " + block.BlockDefinition.SubtypeId + ", ammo = ";

        block.GetInventory().GetAcceptedItems(items);
        foreach (MyItemType item in items)
        {
            m_EchoString += item.SubtypeId + " ";
        }
        items.Clear();

        m_EchoString += "\n";
    }
    m_EchoString += "\n";
}

private void RunPurgeMode()
{
    m_EchoString += "=== CannonAmmoPuller - PURGE mode ===\n";




}

private void RunAmmoPullMode()
{
    m_EchoString += "=== CannonAmmoPuller ===\n";

    string[] lines = m_CustomdataString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    m_EchoString += "Found " + lines.Length + " gun request(s).\n";

    foreach (string line in lines)
    {
        ParseGunAndPullAmmo(line);
    }

}

private void ParseGunAndPullAmmo(string _line)
{
    const string s_gun = "gun = ";
    const string s_ammo = "ammo = ";

    int starts = _line.IndexOf(s_gun);
    int ends = _line.IndexOf(s_ammo);
    string gunTypeIdString = _line.Substring(s_gun.Length, ends - s_gun.Length).Trim();
    string ammoTypeIdString = _line.Substring(ends).Trim();

    m_EchoString += "Gun " + gunTypeIdString + ": ";
    MyItemType gunTypeId = MyItemType.Parse(gunTypeIdString);
    if (gunTypeId == null)
    {
        m_EchoString += "Invalid Gun TypeId.\n";
        return;
    }
    m_EchoString += "\n";

    m_EchoString += "  Ammo " + ammoTypeIdString + ": ";
    MyItemType ammoTypeId = MyItemType.Parse(ammoTypeIdString);
    if (ammoTypeId == null)
    {
        m_EchoString += "Invalid Ammo TypeId.\n";
        return;
    }
    m_EchoString += "\n";

    PullAmmo(gunTypeIdString, ammoTypeId);

}

/// Here lies the main work.
private void PullAmmo(string _gunTypeIdString, MyItemType _ammoTypeId)
{
    GridTerminalSystem.GetBlocksOfType(m_Cannons, (IMyTerminalBlock _block) =>
    {
        return (Me.IsSameConstructAs(_block) && _block.BlockDefinition.SubtypeId == _gunTypeIdString && _block.HasInventory);
    });
    if (m_Cannons.Count <= 0)
    {
        m_EchoString += "  There is no gun or this gun has no Inventory.\n";
        return;
    }

    GridTerminalSystem.GetBlocksOfType(m_Cargos, (IMyTerminalBlock _block) => {
        return (Me.IsSameConstructAs(_block)
            && (_block.CustomName.Contains("Cargo") || _block.CustomName.Contains("Connector") || _block.CustomName.Contains("Cockpit")));
    });

    long totalAmmoCount = 0;
    IMyInventory inventory = null;
    foreach (IMyTerminalBlock gun in m_Cannons)
    {
        inventory = gun.GetInventory();
        totalAmmoCount += inventory.GetItemAmount(_ammoTypeId).RawValue;
        inventory = null;
    }
    if (m_Cargos.Count > 0)
    {
        foreach (IMyCargoContainer cargo in m_Cargos)
        {
            inventory = cargo.GetInventory();
            totalAmmoCount += inventory.GetItemAmount(_ammoTypeId).RawValue;
            inventory = null;
        }
    }
    long avgAmmoCount = totalAmmoCount / m_Cannons.Count;
    m_EchoString += "  Ammo Count: " + totalAmmoCount + " (" + avgAmmoCount + " per gun).\n";

    m_Cannons.Sort((IMyTerminalBlock block0, IMyTerminalBlock block1) =>
    {
        long ammo0 = block0.GetInventory().GetItemAmount(_ammoTypeId).RawValue;
        long ammo1 = block1.GetInventory().GetItemAmount(_ammoTypeId).RawValue;

        if (ammo0 > ammo1)
            return -1;
        if (ammo0 < ammo1)
            return 1;
        return 0;
    });

    /// Try distribute all surplus ammo IN EACH GUN.
    for (int i = 0; i < m_Cannons.Count; ++i)
    {
        inventory = m_Cannons[i].GetInventory();
        MyFixedPoint surplusAmount = inventory.GetItemAmount(_ammoTypeId);
        if ((long)surplusAmount <= avgAmmoCount)
        {
            continue;
        }

        for (int j = m_Cannons.Count - 1; j > 0; --j)
        {
            if (j == i)
                break;

            //invJ = m_Cannons[j].GetInventory();
            MyFixedPoint ammoJ = m_Cannons[j].GetInventory().GetItemAmount(_ammoTypeId);
            MyFixedPoint neededAmount = (MyFixedPoint)(decimal)avgAmmoCount - ammoJ;
            if (neededAmount <= 0)
            {
                m_Cannons.RemoveAt(j);
            }
            else
            {
                if (neededAmount <= surplusAmount)
                {
                    inventory.TransferItemTo(m_Cannons[j].GetInventory(), inventory.GetItemAt(0).Value, neededAmount);
                    m_Cannons.RemoveAt(j);
                }
                else
                {
                    inventory.TransferItemTo(m_Cannons[j].GetInventory(), inventory.GetItemAt(0).Value, surplusAmount);
                    m_Cannons.RemoveAt(i);
                    --i;
                    --j;
                    break;
                }
            }
        }

        inventory = null;
    }

    if (m_Cannons.Count <= 0)
    {
        return;
    }

    /// Distribute surplus ammo IN CARGO.
    foreach (IMyCargoContainer cargo in m_Cargos)
    {
        inventory = cargo.GetInventory();
        MyFixedPoint surplusAmount = inventory.GetItemAmount(_ammoTypeId);
        if ((long)surplusAmount <= 0)
        {
            continue;
        }

        for (int j = 0; j < m_Cannons.Count; ++j)
        {
            MyFixedPoint ammoJ = m_Cannons[j].GetInventory().GetItemAmount(_ammoTypeId);
            MyFixedPoint neededAmount = (MyFixedPoint)(decimal)avgAmmoCount - ammoJ;
            if (neededAmount <= 0)
            {
                m_Cannons.RemoveAt(j);
                --j;
            }
            else
            {
                if (neededAmount <= surplusAmount)
                {
                    inventory.TransferItemTo(m_Cannons[j].GetInventory(), inventory.GetItemAt(0).Value, neededAmount);
                    m_Cannons.RemoveAt(j);
                    --j;
                }
                else
                {
                    inventory.TransferItemTo(m_Cannons[j].GetInventory(), inventory.GetItemAt(0).Value, surplusAmount);
                    break;
                }
            }
        }
    }


}


public Program()
{
    m_UpdateCount = 0;
    Init_CannonAmmoPuller();

    //panels = new List<IMyTextPanel>();

    //Echo += (string _text) =>
    //{
    //    if (smallPanel != null)
    //    {
    //        smallPanel.WriteText(_text + "\n", true);
    //    }
    //};
}

public void Main(string argument, UpdateType updateSource)
{
    ++m_UpdateCount;
    Echo("Update Count: " + m_UpdateCount);

    Run_CannonAmmoPuller();

}