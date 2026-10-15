using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Ehternum;

public sealed class ItemDef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public int Stack { get; set; } = 50;
}
public sealed class RecipeDef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Output { get; set; } = "";
    public int Amount { get; set; } = 1;
    public Dictionary<string,int> Cost { get; set; } = new();
    public float Energy { get; set; }
    public float Seconds { get; set; } = 1;
}
public sealed class TechDef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Memory { get; set; }
    public List<string> Requires { get; set; } = new();
    public Dictionary<string,int> Cost { get; set; } = new();
    public List<string> Recipes { get; set; } = new();
}
public sealed class ChestState
{
    public float X { get; set; }
    public float Y { get; set; }
    public Dictionary<string,int> Items { get; set; } = new();
}
public sealed class SaveState
{
    public int Version { get; set; } = 1;
    public string PendingRecipe { get; set; } = "";
    public float JobRemaining { get; set; }
    public float X { get; set; } = 155;
    public float Y { get; set; } = 85;
    public float Health { get; set; } = 100;
    public float Hunger { get; set; } = 100;
    public float Thirst { get; set; } = 100;
    public float Energy { get; set; } = 65;
    public float Poison { get; set; }
    public float Illness { get; set; }
    public float Injury { get; set; }
    public int Memory { get; set; }
    public bool LaserUpgrade { get; set; }
    public bool Suit { get; set; }
    public bool PowerUpgrade { get; set; }
    public bool Garden { get; set; }
    public bool Beacon { get; set; }
    public bool Rescued { get; set; }
    public float Signal { get; set; }
    public float Time { get; set; }
    public Dictionary<string,int> Bag { get; set; } = new() { ["universerium"]=12,["elekrerium"]=3,["food"]=3,["water"]=4 };
    public HashSet<string> Tech { get; set; } = new() { "supply" };
    public List<ChestState> Chests { get; set; } = new();
    public Dictionary<int,float> Deposits { get; set; } = new();
    public Dictionary<int,int> DepositUnits { get; set; } = new();
}

// Pure C#: transactions and content validation are testable without the engine.
public sealed class Model
{
    public static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive=true, WriteIndented=true };
    public readonly Dictionary<string,ItemDef> Items;
    public readonly Dictionary<string,RecipeDef> Recipes;
    public readonly Dictionary<string,TechDef> Technologies;
    public SaveState S = new();
    public Model(string items, string recipes, string technologies)
    {
        Items = JsonSerializer.Deserialize<List<ItemDef>>(items,Json)!.ToDictionary(x=>x.Id);
        Recipes = JsonSerializer.Deserialize<List<RecipeDef>>(recipes,Json)!.ToDictionary(x=>x.Id);
        Technologies = JsonSerializer.Deserialize<List<TechDef>>(technologies,Json)!.ToDictionary(x=>x.Id);
        Validate();
    }
    public void Validate()
    {
        foreach(var i in Items.Values)
            if(string.IsNullOrWhiteSpace(i.Id)||i.Stack<1) throw new Exception("Ungültige Gegenstandsdefinition.");
        void Costs(Dictionary<string,int> cost)
        {
            foreach(var c in cost) if(!Items.ContainsKey(c.Key)||c.Value<1) throw new Exception("Ungültige Kosten: "+c.Key);
        }
        foreach(var r in Recipes.Values)
        {
            if(!Items.ContainsKey(r.Output)||r.Amount<1||r.Seconds<=0||r.Energy<0) throw new Exception("Ungültiges Rezept: "+r.Id);
            Costs(r.Cost);
        }
        var visited=new HashSet<string>(); var active=new HashSet<string>();
        void Visit(string id)
        {
            if(!Technologies.ContainsKey(id)) throw new Exception("Unbekannte Technologie: "+id);
            if(active.Contains(id)) throw new Exception("Zyklus im Technologiebaum: "+id);
            if(visited.Contains(id)) return;
            active.Add(id); var t=Technologies[id]; Costs(t.Cost);
            if(t.Memory<0) throw new Exception("Negativer Speicherbedarf.");
            foreach(var r in t.Recipes) if(!Recipes.ContainsKey(r)) throw new Exception("Unbekanntes Rezept: "+r);
            foreach(var p in t.Requires) Visit(p);
            active.Remove(id); visited.Add(id);
        }
        foreach(var t in Technologies.Keys) Visit(t);
    }
    public int Count(string id, bool baseStorage=false) => S.Bag.GetValueOrDefault(id)+(baseStorage?S.Chests.Where(NearBase).Sum(c=>c.Items.GetValueOrDefault(id)):0);
    public static bool NearBase(ChestState c) => c.X*c.X+c.Y*c.Y<360*360;
    public int Slots(Dictionary<string,int> inv) => inv.Sum(x=>(int)Math.Ceiling(x.Value/(double)Items[x.Key].Stack));
    public bool Fits(Dictionary<string,int> inv,string id,int amount,int limit)
    {
        var copy=new Dictionary<string,int>(inv); copy[id]=copy.GetValueOrDefault(id)+amount;
        return Slots(copy)<=limit;
    }
    public bool Add(string id,int amount)
    {
        if(!Fits(S.Bag,id,amount,16)) return false;
        S.Bag[id]=S.Bag.GetValueOrDefault(id)+amount; return true;
    }
    public bool Pay(Dictionary<string,int> costs,bool storage)
    {
        if(costs.Any(x=>Count(x.Key,storage)<x.Value)) return false;
        foreach(var c in costs)
        {
            int left=c.Value;
            void Take(Dictionary<string,int> inv)
            {
                int n=Math.Min(left,inv.GetValueOrDefault(c.Key)); left-=n;
                if(n>0) { inv[c.Key]-=n; if(inv[c.Key]==0) inv.Remove(c.Key); }
            }
            Take(S.Bag);
            if(storage) foreach(var chest in S.Chests.Where(NearBase)) Take(chest.Items);
        }
        return true;
    }
    public bool Unlocked(string recipe) => S.Tech.Any(id=>Technologies.TryGetValue(id,out var t)&&t.Recipes.Contains(recipe));
    public bool Unlock(string id)
    {
        var t=Technologies[id];
        if(S.Tech.Contains(id)||t.Memory>S.Memory||t.Requires.Any(x=>!S.Tech.Contains(x))) return false;
        if(!Pay(t.Cost,true)) return false;
        S.Tech.Add(id); return true;
    }
    public bool ExpandMemory()
    {
        if(S.Memory>=6 || !Pay(new() { ["ehternum"]=8+S.Memory*8 },true)) return false;
        S.Memory++; return true;
    }
    public float MaxEnergy => S.PowerUpgrade?200:100;
    public string Name(string id) => Items.TryGetValue(id,out var d)?d.Name:id;
    public bool Consume(string id)
    {
        if(S.Bag.GetValueOrDefault(id)<1) return false;
        string kind=Items[id].Kind;
        if(kind=="food" && S.Hunger>=100 || kind=="water" && S.Thirst>=100) return false;
        if(!new[]{"food","water","medicine","laser","suit","cell"}.Contains(kind)) return false;
        if(kind=="laser"&&S.LaserUpgrade || kind=="suit"&&S.Suit || kind=="cell"&&S.PowerUpgrade) return false;
        if(!Pay(new(){[id]=1},false)) return false;
        switch(kind)
        {
            case "food": S.Hunger=Math.Min(100,S.Hunger+38); break;
            case "water": S.Thirst=Math.Min(100,S.Thirst+45); break;
            case "medicine": S.Poison=Math.Max(0,S.Poison-45); S.Illness=Math.Max(0,S.Illness-40); S.Health=Math.Min(100,S.Health+12); break;
            case "laser": S.LaserUpgrade=true; break;
            case "suit": S.Suit=true; break;
            case "cell": S.PowerUpgrade=true; break;
        }
        return true;
    }
    public bool Transfer(ChestState chest,string id,bool intoChest)
    {
        var src=intoChest?S.Bag:chest.Items; var dst=intoChest?chest.Items:S.Bag;
        int amount=Math.Min(10,src.GetValueOrDefault(id));
        while(amount>0&&!Fits(dst,id,amount,intoChest?24:16)) amount--;
        if(amount==0) return false;
        src[id]-=amount; if(src[id]==0) src.Remove(id);
        dst[id]=dst.GetValueOrDefault(id)+amount; return true;
    }
    public void Load(string json)
    {
        var loaded=JsonSerializer.Deserialize<SaveState>(json,Json)??throw new Exception("Leerer Spielstand.");
        if(loaded.Version!=1) throw new Exception("Nicht unterstützte Spielstandversion.");
        if(loaded.Bag==null||loaded.Tech==null||loaded.Chests==null||loaded.Deposits==null) throw new Exception("Unvollständiger Spielstand.");
        if(!float.IsFinite(loaded.X)||!float.IsFinite(loaded.Y)) throw new Exception("Ungültige Position.");
        foreach(var bag in loaded.Chests.Select(c=>c.Items).Append(loaded.Bag))
            if(bag==null||bag.Any(x=>!Items.ContainsKey(x.Key)||x.Value<0)) throw new Exception("Ungültiges Inventar.");
        if(loaded.PendingRecipe!=""&&!Recipes.ContainsKey(loaded.PendingRecipe))throw new Exception("Gespeichertes Rezept fehlt.");
        if(loaded.Tech.Any(x=>!Technologies.ContainsKey(x))) throw new Exception("Unbekannte gespeicherte Technologie.");
        loaded.Memory=Math.Clamp(loaded.Memory,0,6); loaded.Tech.Add("supply");
        loaded.Health=Math.Clamp(loaded.Health,1,100); loaded.Hunger=Math.Clamp(loaded.Hunger,0,100); loaded.Thirst=Math.Clamp(loaded.Thirst,0,100);
        loaded.Energy=Math.Clamp(loaded.Energy,0,loaded.PowerUpgrade?200:100);
        S=loaded;
    }
}
