using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    public string Name = "Unit Name";
    public int HitPoints = 100;
    public int Strength = 8;
    public int Movement = 2;
    public int MovementRemaining = 2;

   public Hex Hex {get; protected set;}

   public delegate void UnitMovedDelegate (Hex originalHex, Hex finalHex);

   public UnitMovedDelegate OnUnitMoved;

   public void SetHex(Hex finalHex) 
   {
        Hex originalHex = Hex;
        if (Hex != null) 
        {
            Hex.RemoveUnit(this);
        }

        Hex = finalHex;

        Hex.AddUnit(this);

        if(OnUnitMoved != null)
        {
            OnUnitMoved(originalHex, finalHex);
        }
   }

   public void DoTurn() 
   {
        Hex originalHex = Hex;
        Hex finalHex = originalHex.HexMap.GetHexAt(originalHex.Q + 1, originalHex.R);

        SetHex(finalHex);
   }
}
