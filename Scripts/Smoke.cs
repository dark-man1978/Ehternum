using Godot;
using System;
using System.Linq;
using System.Text.Json;

namespace Ehternum;
public partial class Game
{
    private void Smoke()
    {
        int checks=0;
        void Check(bool condition,string description)
        {if(!condition)throw new Exception(description);checks++;GD.Print("PASS · "+description);}
        try
        {
            NewGame();
            Check(M.S.Tech.SetEquals(new[]{"supply"}),"Startarchiv und Startinventar");
            Check(!M.Unlock("rescue"),"Kommunikation ist an Voraussetzungen gebunden");
            var before=JsonSerializer.Serialize(M.S.Bag);
            Check(!M.Pay(new(){["ehternum"]=100},true)&&before==JsonSerializer.Serialize(M.S.Bag),"Fehlgeschlagene Transaktion verbraucht nichts");
            M.S.Bag["ehternum"]=300;M.S.Bag["universerium"]=300;M.S.Bag["elekrerium"]=100;
            Check(M.ExpandMemory()&&M.S.Memory==1,"Ehternum erweitert den Speicher");
            Check(M.Unlock("tools")&&M.Unlock("medical"),"Erste Technologieverzweigung");
            Check(M.ExpandMemory()&&M.Unlock("power")&&M.Unlock("habitat"),"Energie- und Habitatarchiv");
            Check(M.ExpandMemory()&&M.Unlock("rescue"),"Kommunikationsarchiv erreichbar");
            // Reduce oversized testing resources to valid starter stacks before crafting.
            M.S.Bag["ehternum"]=50;M.S.Bag["universerium"]=50;M.S.Bag["elekrerium"]=20;
            M.S.Energy=100;
            Craft(M.Recipes["laser2"]);Check(M.S.PendingRecipe=="laser2","Herstellung startet");
            M.S.JobRemaining=0;FinishJob();Check(M.Consume("laser2")&&M.S.LaserUpgrade,"Werkzeug wird produziert und ausgerüstet");
            var chest=new ChestState{X=210,Y=150};M.S.Chests.Add(chest);
            Check(M.Transfer(chest,"universerium",true),"Rohstoffe einlagern");
            Check(M.Count("universerium",true)>M.Count("universerium"),"Basiskiste wird für Herstellung berücksichtigt");
            Check(M.Transfer(chest,"universerium",false),"Rohstoffe entnehmen");
            M.S.Poison=80;M.S.Illness=50;M.S.Injury=30;M.S.Energy=100;Heal();
            Check(M.S.Poison==0&&M.S.Illness==0&&M.S.Injury==0&&M.S.Energy==82,"Bio-Bett heilt alle Zustände mit Energieverbrauch");
            M.S.Bag["garden"]=1;M.S.Bag["beacon"]=1;M.S.X=180;M.S.Y=100;
            Place("garden");Place("beacon");Check(M.S.Garden&&M.S.Beacon,"Habitat und Sender platzierbar");
            M.S.Signal=59.95f;M.S.Energy=80;Simulate(.1f);
            Check(M.S.Rescued,"Rettung wird freigeschaltet");
            M.S.PendingRecipe="water";M.S.JobRemaining=1.2f;
            Check(Save(),"Spielstand wird geschrieben");
            M.S.LaserUpgrade=false;M.S.Rescued=false;
            Check(Load()&&M.S.LaserUpgrade&&M.S.Rescued&&M.S.Chests.Count==1&&M.S.PendingRecipe=="water","Spielstand inklusive Baufortschritt und Produktionsauftrag geladen");
            string json=JsonSerializer.Serialize(M.S,Model.Json);var independent=new Model(Read("items"),Read("recipes"),Read("technologies"));independent.Load(json);
            Check(independent.S.Tech.SetEquals(M.S.Tech),"JSON-Roundtrip erhält den Technologiebaum");
            bool rejected=false;try{independent.Load(json.Replace("\"Version\": 1","\"Version\": 99"));}catch{rejected=true;}
            Check(rejected,"Unbekannte Spielstandversion wird abgelehnt");
            foreach(var menu in new[]{"title","help","base","craft","tech","bag","chest","rescue","ending","pause"})ShowMenu(menu);
            Check(true,"Alle Menüs ohne Ausnahme aufgebaut");
            GD.Print($"SMOKE SUCCESS: {checks} checks");GetTree().Quit(0);
        }
        catch(Exception e){GD.PushError("SMOKE FAILURE: "+e);GetTree().Quit(1);}
    }
}
