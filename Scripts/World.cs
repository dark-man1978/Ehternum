using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ehternum;

public sealed class Deposit
{
    public int Id;
    public Vector2 Position;
    public string Resource="";
    public float Progress;
    public int Units=8;
}
public sealed class Creature
{
    public Vector2 Position,Home;
    public float Phase,Affection;
    public void Tick(float dt,Vector2 player)
    {
        Phase+=dt;Affection=Math.Max(0,Affection-dt);
        Vector2 target=Affection>0?player+new Vector2(48,28):Home+new Vector2(Mathf.Sin(Phase*.32f)*75,Mathf.Cos(Phase*.23f)*55);
        Position=Position.MoveToward(target,dt*22);
    }
}
// Original vector illustration: layered silhouettes, ink contours and muted mineral colours.
public partial class World : Node2D
{
    public Game Game=null!;
    public readonly List<Deposit> Deposits=new();
    public readonly List<Creature> Animals=new();
    private readonly List<(Vector2 P,int Type,float Size)> _plants=new();
    private readonly Color _ink=new("20373a"), _sand=new("afaa80"), _leaf=new("587b66");
    private Font _font=null!;
    public override void _Ready()
    {
        _font=ThemeDB.FallbackFont;
        var rng=new RandomNumberGenerator{Seed=198704};
        for(int i=0;i<410;i++)
        {
            var p=new Vector2(rng.RandfRange(-1800,1800),rng.RandfRange(-1400,1400));
            if(p.Length()<220||InWater(p)||InSpores(p))continue;
            _plants.Add((p,rng.RandiRange(0,3),rng.RandfRange(.7f,1.4f)));
        }
        // Starter deposits are within sight, all resource types remain renewable.
        AddDeposit(new Vector2(290,135),"universerium");AddDeposit(new Vector2(-190,190),"elekrerium");AddDeposit(new Vector2(110,-240),"ehternum");
        string[] types={"universerium","elekrerium","ehternum"};
        for(int i=0;i<65;i++)
        {
            var p=new Vector2(rng.RandfRange(-1670,1670),rng.RandfRange(-1250,1250));
            if(p.Length()<310||InWater(p))continue;AddDeposit(p,types[i%3]);
        }
        foreach(var home in new[]{new Vector2(380,240),new Vector2(-340,-80),new Vector2(780,360),new Vector2(-850,480),new Vector2(250,-800)})
            Animals.Add(new Creature{Home=home,Position=home,Phase=rng.RandfRange(0,20)});
    }
    private void AddDeposit(Vector2 p,string type)=>Deposits.Add(new(){Id=Deposits.Count,Position=p,Resource=type});
    public static bool InSpores(Vector2 p)=>((p-new Vector2(1000,-200))/new Vector2(400,330)).Length()<1;
    public static bool InWater(Vector2 p)=>((p-new Vector2(-1000,-370))/new Vector2(420,280)).Length()<1;
    public static bool InRubble(Vector2 p)=>p.DistanceTo(new Vector2(-100,390))<120;
    public override void _Draw()
    {
        if(Game.M==null)return;
        float t=Game.Clock;
        DrawRect(new Rect2(-2300,-1900,4600,3800),new Color("213c3d"));
        Oval(Vector2.Zero,new Vector2(1910,1490),new Color("657c68"));
        Oval(new Vector2(-400,180),new Vector2(1380,1100),new Color("76866b"));
        Oval(new Vector2(470,-400),new Vector2(900,650),new Color("7c8a70"));
        // Broad dry streambed leads from the wreck into the basin.
        for(int i=0;i<22;i++)
        {
            float y=-1400+i*135;float x=180+Mathf.Sin(i*.25f)*230;
            Oval(new Vector2(x,y),new Vector2(155,155),new Color("9c9c78"));
            Oval(new Vector2(x+20,y),new Vector2(84,150),new Color("aaa781"));
        }
        Oval(Vector2.Zero,new Vector2(250,185),_sand);
        Oval(new Vector2(-1000,-370),new Vector2(455,310),new Color("91a58b"));
        Oval(new Vector2(-1000,-370),new Vector2(420,280),new Color("426d74"));
        Oval(new Vector2(-1030,-390),new Vector2(350,220),new Color("3a606b"));
        for(int i=0;i<14;i++)
        {
            var p=new Vector2(-1260+(i%4)*150,-520+(i/4)*105);
            DrawArc(p,30+Mathf.Sin(t+i)*3,.1f,2.8f,16,new Color(.64f,.80f,.76f,.28f),1.5f,true);
        }
        Oval(new Vector2(1000,-200),new Vector2(400,330),new Color("7c805c"));
        for(int i=0;i<32;i++)
        {
            float a=i*2.39996f;float r=Mathf.Sqrt(i/32f);
            var p=new Vector2(1000+Mathf.Cos(a)*365*r,-200+Mathf.Sin(a)*285*r);
            DrawLine(p,p+new Vector2(0,-28),new Color("4a6358"),5,true);
            Oval(p+new Vector2(0,-30),new Vector2(18,10),new Color("c0b178"),true);
            DrawCircle(p+new Vector2(Mathf.Sin(t*.8f+i)*28,-50-Mathf.PosMod(t*8+i*7,35)),2,new Color(.85f,.82f,.49f,.55f));
        }
        foreach(var plant in _plants)DrawPlant(plant.P,plant.Type,plant.Size);
        // Wreck fragments are visible and dangerous only in this marked patch.
        Oval(new Vector2(-100,390),new Vector2(130,105),new Color("8d826b"));
        for(int i=0;i<18;i++)
        {var p=new Vector2(-190+(i%6)*36,330+(i/6)*42);Polygon(new[]{p,p+new Vector2(30,-12),p+new Vector2(20,18)},new Color("546469"));}
        foreach(var d in Deposits)DrawDeposit(d);
        DrawCapsule();
        if(Game.M.S.Garden)DrawGarden();
        if(Game.M.S.Beacon)DrawBeacon(t);
        foreach(var c in Game.M.S.Chests)DrawChest(new Vector2(c.X,c.Y));
        foreach(var a in Animals.OrderBy(a=>a.Position.Y))DrawAnimal(a,t);
        DrawPlayer(Game.Player,t);
        if(Game.Mining)
        {
            var start=Game.Player+new Vector2(0,-10)+Game.Facing*19;
            DrawLine(start,Game.BeamEnd,new Color(.94f,.80f,.48f,.18f),13,true);
            DrawLine(start,Game.BeamEnd,new Color("f9d687"),3,true);
            DrawLine(start,Game.BeamEnd,new Color("fff5d1"),1,true);
            for(int i=0;i<6;i++) {float a=t*13+i;DrawCircle(Game.BeamEnd+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(8+Mathf.Sin(t*19+i)*5),2,new Color("ffe3a0"));}
        }
        Label(new Vector2(-170,-150),"KAPSEL 07 · HEIMATHAFEN",new Color("e9e3c8"));
        Label(new Vector2(-1200,-710),"DIE STILLEN GEWÄSSER",new Color("c3d4cb"));
        Label(new Vector2(780,-570),"SPORENHAIN",new Color("e4d8a3"));
        // Terrain edge contour: no invisible sudden world boundary.
        DrawRect(new Rect2(-1815,-1415,3630,2830),new Color("314c46"),false,28);
    }
    private void Oval(Vector2 c,Vector2 size,Color color,bool ink=false)
    {
        var points=new Vector2[33];
        for(int i=0;i<32;i++) {float a=i*Mathf.Tau/32;points[i]=c+new Vector2(Mathf.Cos(a)*size.X,Mathf.Sin(a)*size.Y);}
        points[32]=points[0];DrawColoredPolygon(points.Take(32).ToArray(),color);
        if(ink)DrawPolyline(points,_ink,2,true);
    }
    private void Polygon(Vector2[] points,Color fill)
    {DrawColoredPolygon(points,fill);DrawPolyline(points.Append(points[0]).ToArray(),_ink,2,true);}
    private void Label(Vector2 p,string text,Color color,int size=16)=>DrawString(_font,p,text,HorizontalAlignment.Left,-1,size,color);
    private void DrawPlant(Vector2 p,int type,float size)
    {
        if(type==0)
        {
            DrawLine(p,p+new Vector2(0,-19*size),new Color("546d54"),2,true);
            DrawLine(p,p+new Vector2(-9,-14)*size,new Color("8d9c73"),2,true);
            DrawLine(p,p+new Vector2(9,-16)*size,new Color("a5ac7c"),2,true);
        }
        else if(type==1)
        {
            Oval(p+new Vector2(10,5),new Vector2(33,12)*size,new Color(0.1f,.2f,.2f,.18f));
            DrawLine(p,p+new Vector2(-4,-60)*size,_ink,7*size,true);
            Oval(p+new Vector2(-17,-47)*size,new Vector2(27,19)*size,new Color("47685b"),true);
            Oval(p+new Vector2(13,-62)*size,new Vector2(29,23)*size,_leaf,true);
            Oval(p+new Vector2(-7,-78)*size,new Vector2(25,20)*size,new Color("789174"),true);
            DrawLine(p+new Vector2(-7,-80)*size,p+new Vector2(9,-62)*size,new Color("93a382"),2,true);
        }
        else if(type==2)
        {
            Polygon(new[]{p+new Vector2(-16,4)*size,p+new Vector2(-12,-15)*size,p+new Vector2(7,-23)*size,p+new Vector2(22,-7)*size,p+new Vector2(15,9)*size},new Color("92947e"));
            DrawLine(p+new Vector2(-12,-15)*size,p+new Vector2(9,-7)*size,new Color("b5b49a"),2,true);
        }
        else
        {
            for(int i=0;i<4;i++)
            {var end=p+new Vector2((i-1.5f)*9,-18-Mathf.Sin(i)*10)*size;DrawLine(p,end,new Color("63744f"),2,true);Oval(end,new Vector2(6,4)*size,new Color("d3bb84"));}
        }
    }
    private void DrawDeposit(Deposit d)
    {
        bool depleted=Game.M.S.Deposits.GetValueOrDefault(d.Id)>=Game.M.S.Time;
        var p=d.Position;
        Oval(p+new Vector2(4,6),new Vector2(37,16),new Color(.12f,.2f,.20f,.3f));
        if(depleted){Oval(p,new Vector2(24,10),new Color("666f63"),true);return;}
        Color color=d.Resource switch {"ehternum"=>new Color("a596c2"),"elekrerium"=>new Color("d7af62"),_=>new Color("79adac")};
        for(int i=0;i<3;i++)
        {
            var q=p+new Vector2((i-1)*17,i==1?-10:0);
            Polygon(new[]{q+new Vector2(-12,7),q+new Vector2(-9,-19-i*5),q+new Vector2(2,-34-i*4),q+new Vector2(14,-14),q+new Vector2(12,8)},color.Darkened(i*.07f));
            DrawLine(q+new Vector2(2,-31-i*4),q+new Vector2(3,5),color.Lightened(.3f),2,true);
        }
        if(p.DistanceTo(Game.Player)<180)
        {
            Label(p+new Vector2(-45,-65),Game.M.Name(d.Resource),new Color("eee7d2"),16);
            if(d.Progress>0) {DrawRect(new Rect2(p+new Vector2(-26,16),new Vector2(52,5)),_ink);DrawRect(new Rect2(p+new Vector2(-26,16),new Vector2(52*d.Progress/1.1f,5)),color);}
        }
    }
    private void DrawCapsule()
    {
        Oval(new Vector2(14,25),new Vector2(108,63),new Color(.1f,.18f,.19f,.35f));
        Polygon(new[]{new Vector2(-87,35),new Vector2(-115,57),new Vector2(-69,50),new Vector2(-55,28)},new Color("65777a"));
        Polygon(new[]{new Vector2(67,26),new Vector2(118,52),new Vector2(91,58),new Vector2(52,42)},new Color("65777a"));
        Oval(Vector2.Zero,new Vector2(94,63),new Color("b7b8aa"),true);
        Oval(new Vector2(0,-13),new Vector2(86,56),new Color("d8d5be"),true);
        Oval(new Vector2(16,-26),new Vector2(40,27),new Color("284b57"),true);
        Oval(new Vector2(19,-32),new Vector2(30,15),new Color("557f88"));
        DrawArc(new Vector2(17,-30),29,3.4f,5.7f,18,new Color("98b6ad"),3,true);
        Polygon(new[]{new Vector2(-30,18),new Vector2(31,18),new Vector2(38,64),new Vector2(-37,64)},new Color("657d77"));
        DrawLine(new Vector2(-20,26),new Vector2(20,26),new Color("b7c5af"),3,true);
        Label(new Vector2(-72,-5),"07",_ink,23);
        DrawLine(new Vector2(-52,-54),new Vector2(-34,-31),new Color("716f64"),3,true);
        DrawLine(new Vector2(-34,-31),new Vector2(-43,-17),new Color("716f64"),2,true);
        DrawCircle(new Vector2(-47,25),4,new Color("9dc49c"));
        // Bio-bed and replicator service ports, readable visual cues.
        DrawRect(new Rect2(53,6,23,21),new Color("4e6f67"));
        DrawLine(new Vector2(64,10),new Vector2(64,23),new Color("c1d8b7"),3);DrawLine(new Vector2(58,16),new Vector2(70,16),new Color("c1d8b7"),3);
    }
    private void DrawChest(Vector2 p)
    {
        Oval(p+new Vector2(5,9),new Vector2(35,13),new Color(.1f,.2f,.2f,.25f));
        Polygon(new[]{p+new Vector2(-27,-18),p+new Vector2(26,-18),p+new Vector2(26,17),p+new Vector2(-27,17)},new Color("9b8664"));
        DrawRect(new Rect2(p+new Vector2(-26,-16),new Vector2(51,10)),new Color("c0a87c"));
        DrawLine(p+new Vector2(-16,-18),p+new Vector2(-16,17),_ink,3);DrawLine(p+new Vector2(15,-18),p+new Vector2(15,17),_ink,3);
        DrawRect(new Rect2(p+new Vector2(-5,-5),new Vector2(10,9)),new Color("cdd5b9"));
    }
    private void DrawGarden()
    {
        var p=new Vector2(-175,-95);
        Oval(p,new Vector2(64,43),new Color("c2b68e"),true);Oval(p,new Vector2(52,32),new Color("54654d"));
        for(int i=0;i<6;i++)DrawPlant(p+new Vector2(-32+i%3*31,-5+i/3*26),3,1);
        DrawArc(p+new Vector2(0,5),63,Mathf.Pi,Mathf.Tau,28,new Color(.8f,.9f,.85f,.45f),2,true);
    }
    private void DrawBeacon(float time)
    {
        var p=new Vector2(155,-120);
        DrawLine(p+new Vector2(-25,10),p+new Vector2(0,-58),_ink,5,true);DrawLine(p+new Vector2(25,10),p+new Vector2(0,-58),_ink,5,true);
        DrawLine(p,p+new Vector2(0,-80),new Color("b7c4b5"),7,true);
        Oval(p+new Vector2(0,-65),new Vector2(30,12),new Color("b9c9bf"),true);
        DrawCircle(p+new Vector2(0,-80),5,new Color("e6c88b"));
        float r=12+Mathf.PosMod(time*18,50);DrawArc(p+new Vector2(0,-80),r,-2.7f,-.4f,24,new Color(.9f,.8f,.55f,1-r/65),2,true);
    }
    private void DrawAnimal(Creature a,float t)
    {
        var p=a.Position;float bob=Mathf.Sin(t*3+a.Phase)*1.5f;
        Oval(p+new Vector2(2,10),new Vector2(25,8),new Color(.1f,.2f,.2f,.22f));
        for(int i=0;i<4;i++)DrawLine(p+new Vector2(-14+i*9,2),p+new Vector2(-14+i*9,13+Mathf.Sin(t*5+i)*2),new Color("445b54"),4,true);
        Oval(p+new Vector2(0,-4+bob),new Vector2(24,16),new Color("b1b995"),true);
        Oval(p+new Vector2(17,-12+bob),new Vector2(13,12),new Color("d0ceb0"),true);
        DrawLine(p+new Vector2(14,-21+bob),p+new Vector2(11,-35+bob),_ink,3,true);
        DrawLine(p+new Vector2(23,-21+bob),p+new Vector2(28,-31+bob),_ink,3,true);
        DrawCircle(p+new Vector2(23,-14+bob),2,_ink);
        DrawArc(p+new Vector2(22,-9+bob),4,0,1.8f,6,_ink,1,true);
        if(a.Affection>0)Label(p+new Vector2(-7,-47),"♥",new Color("e5bd99"),20);
    }
    private void DrawPlayer(Vector2 p,float t)
    {
        var suit=Game.M.S.Suit?new Color("c1c6a7"):new Color("dcbe87");
        Oval(p+new Vector2(0,10),new Vector2(17,7),new Color(.06f,.14f,.17f,.4f));
        DrawLine(p+new Vector2(-6,3),p+new Vector2(-8,14),_ink,7,true);DrawLine(p+new Vector2(6,3),p+new Vector2(8,14),_ink,7,true);
        Oval(p+new Vector2(0,-5),new Vector2(13,18),suit,true);
        DrawRect(new Rect2(p+new Vector2(-17,-16),new Vector2(8,22)),new Color("6b8080"));
        Oval(p+new Vector2(0,-25),new Vector2(13,13),new Color("e4dfc6"),true);
        Oval(p+new Vector2(2,-26),new Vector2(10,6),new Color("345966"));
        DrawLine(p+new Vector2(-4,-29),p+new Vector2(7,-29),new Color("91b3b8"),2,true);
        var arm=p+Game.Facing*18+new Vector2(0,-10);
        DrawLine(p+new Vector2(7,-10),arm,suit,7,true);
        DrawLine(arm,arm+Game.Facing*16,new Color("344e54"),7,true);
        DrawCircle(arm+Game.Facing*16,3,new Color("d6c399"));
    }
}
