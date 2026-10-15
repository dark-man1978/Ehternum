using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using FileAccess = Godot.FileAccess;

namespace Ehternum;

public partial class Game : Node2D
{
    public Model M = null!;
    public Vector2 Player => new(M.S.X,M.S.Y);
    public Vector2 Facing=Vector2.Down;
    public float Clock;
    public bool Mining;
    public Vector2 BeamEnd;
    private World _world=null!;
    private Camera2D _camera=null!;
    private Label _stats=null!, _hint=null!, _toast=null!, _objective=null!;
    private ProgressBar _jobBar=null!;
    private Control _overlay=null!;
    private VBoxContainer _list=null!;
    private string _menu="title", _message="";
    private float _messageTime, _autosave, _uiTick, _resumeDelay;
    private int _chest;
    private readonly RandomNumberGenerator _rng=new();
    private static string SavePath=>OS.GetCmdlineUserArgs().Contains("--smoke-test")?"user://ehternum-smoke-save.json":"user://ehternum-save.json";
    public override void _Ready()
    {
        try { M=new Model(Read("items"),Read("recipes"),Read("technologies")); }
        catch(Exception e) { GD.PushError(e.ToString()); GetTree().Quit(1); return; }
        _rng.Seed=1978;
        Bind("left",Key.A,Key.Left,JoyButton.DpadLeft);
        Bind("right",Key.D,Key.Right,JoyButton.DpadRight);
        Bind("up",Key.W,Key.Up,JoyButton.DpadUp);
        Bind("down",Key.S,Key.Down,JoyButton.DpadDown);
        Bind("interact",Key.E,Key.None,JoyButton.A);
        Bind("mine",Key.Space,Key.None,JoyButton.X);
        Bind("bag",Key.I,Key.Tab,JoyButton.Y);
        Bind("close",Key.Escape,Key.None,JoyButton.B);
        Bind("pause",Key.P,Key.None,JoyButton.Start);
        Bind("save",Key.F5); Bind("load",Key.F9); Bind("food",Key.Key1); Bind("water",Key.Key2);
        InputMap.ActionAddEvent("mine",new InputEventMouseButton{ButtonIndex=MouseButton.Left});
        InputMap.ActionAddEvent("mine",new InputEventJoypadMotion{Axis=JoyAxis.TriggerRight,AxisValue=1});
        _world=new World{Game=this}; AddChild(_world);
        _camera=new Camera2D { Position=Player,PositionSmoothingEnabled=true,PositionSmoothingSpeed=5 };
        AddChild(_camera);
        BuildHud(); ShowMenu("title");
        GetTree().AutoAcceptQuit=false;
        if(OS.GetCmdlineUserArgs().Contains("--smoke-test"))Callable.From(Smoke).CallDeferred();
    }
    private static string Read(string file)=>FileAccess.GetFileAsString($"res://Data/{file}.json");
    private static void Bind(string action,Key key,Key alternate=Key.None,JoyButton? joy=null)
    {
        if(!InputMap.HasAction(action)) InputMap.AddAction(action);
        InputMap.ActionAddEvent(action,new InputEventKey{PhysicalKeycode=key});
        if(alternate!=Key.None) InputMap.ActionAddEvent(action,new InputEventKey{PhysicalKeycode=alternate});
        if(joy.HasValue) InputMap.ActionAddEvent(action,new InputEventJoypadButton{ButtonIndex=joy.Value});
    }
    private void BuildHud()
    {
        var canvas=new CanvasLayer(); AddChild(canvas);
        var root=new Control(); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect); root.MouseFilter=Control.MouseFilterEnum.Ignore; canvas.AddChild(root);
        var theme=new Theme(); theme.DefaultFontSize=19;
        var normal=Style(new Color("263b3d")); var hover=Style(new Color("425a53")); var focus=Style(new Color("426964"));
        focus.BorderColor=new Color("e7c68d"); focus.SetBorderWidthAll(2);
        theme.SetStylebox("normal","Button",normal); theme.SetStylebox("hover","Button",hover);
        theme.SetStylebox("focus","Button",focus); theme.SetStylebox("pressed","Button",focus);
        theme.SetColor("font_color","Label",new Color("f0e8d8")); root.Theme=theme;
        var header=new PanelContainer { Position=new Vector2(20,18),CustomMinimumSize=new Vector2(680,100),MouseFilter=Control.MouseFilterEnum.Ignore };
        header.AddThemeStyleboxOverride("panel",Style(new Color(0.06f,0.12f,0.14f,.93f)));root.AddChild(header);
        var top=new VBoxContainer();header.AddChild(top);
        _stats=new Label();top.AddChild(_stats);
        _objective=new Label(); _objective.AddThemeColorOverride("font_color",new Color("e5c88e"));top.AddChild(_objective);
        _jobBar=new ProgressBar{CustomMinimumSize=new Vector2(0,10),ShowPercentage=false,Visible=false};top.AddChild(_jobBar);
        _hint=new Label{Position=new Vector2(24,740),MouseFilter=Control.MouseFilterEnum.Ignore};
        _hint.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomLeft);_hint.Position=new Vector2(24,-58);root.AddChild(_hint);
        _toast=new Label{Position=new Vector2(24,140),MouseFilter=Control.MouseFilterEnum.Ignore};root.AddChild(_toast);
        _overlay=new Control();_overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);root.AddChild(_overlay);
        var shade=new ColorRect{Color=new Color(0.025f,.055f,.07f,.8f)};shade.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);_overlay.AddChild(shade);
        var panel=new PanelContainer();panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        panel.AnchorLeft=.08f;panel.AnchorTop=.10f;panel.AnchorRight=.92f;panel.AnchorBottom=.90f;
        panel.AddThemeStyleboxOverride("panel",Style(new Color("142a2e")));_overlay.AddChild(panel);
        var scroll=new ScrollContainer{HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled,FollowFocus=true};panel.AddChild(scroll);
        _list=new VBoxContainer{SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};_list.AddThemeConstantOverride("separation",10);scroll.AddChild(_list);
    }
    private static StyleBoxFlat Style(Color color)
    {
        var s=new StyleBoxFlat{BgColor=color,CornerRadiusTopLeft=8,CornerRadiusTopRight=8,CornerRadiusBottomLeft=8,CornerRadiusBottomRight=8,
            ContentMarginLeft=18,ContentMarginRight=18,ContentMarginTop=12,ContentMarginBottom=12};return s;
    }
    private void Text(string text,int size=19)
    {
        var l=new Label{Text=text,AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};
        l.AddThemeFontSizeOverride("font_size",size);_list.AddChild(l);
    }
    private Button Button(string text,Action callback,bool enabled=true)
    {
        var b=new Button{Text=text,Disabled=!enabled,CustomMinimumSize=new Vector2(0,46),Alignment=HorizontalAlignment.Left};
        b.Pressed+=callback;_list.AddChild(b);return b;
    }
    private void ClearMenu()
    {
        foreach(Node child in _list.GetChildren()) { _list.RemoveChild(child);child.QueueFree(); }
    }
    private void ShowMenu(string menu)
    {
        _menu=menu;_overlay.Visible=menu!="";Mining=false;ClearMenu();
        if(menu=="")_resumeDelay=.18f;
        if(menu=="")return;
        switch(menu)
        {
            case "title":
                Text("EHTERNUM",48);Text("EINE NEUE HEIMAT",24);
                Text("Deine Kapsel ist gestrandet. Die Sicherungen sind intakt – doch der Speicher ist zerstört.\nEntdecke eine fremde, friedliche Welt. Stelle dein Wissen wieder her. Entscheide, wo du bleiben möchtest.");
                Button("Neue Expedition",()=>{if(FileAccess.FileExists(SavePath))ShowMenu("confirm");else NewGame();});
                Button("Expedition fortsetzen",()=>{if(Load())ShowMenu("");},FileAccess.FileExists(SavePath));
                Button("Steuerung & Spielregeln",()=>ShowMenu("help"));Button("Beenden",()=>GetTree().Quit());break;
            case "confirm":
                Text("Neue Expedition?",30);Text("Beim nächsten Speichern wird dein bisheriger Spielstand ersetzt.");
                Button("Neue Expedition beginnen",NewGame);Button("Zurück",()=>ShowMenu("title"));break;
            case "help":
                Text("Deine ersten Schritte",30);
                Text("1 · Suche farbige Kristalle und halte den Laser gedrückt.\n2 · E / A an der Kapsel öffnet Replikator, Speicher und Bio-Bett.\n3 · Elekrerium lädt die Basis. Ehternum erweitert ihren Speicher.\n4 · Sicherungen wiederherstellen, Gegenstände replizieren und im Inventar benutzen.\n5 · Habitatgarten bauen und Notrufsender aktivieren.");
                Text("PC: WASD / Pfeile bewegen · Maus zielen · linke Maustaste / Leertaste abbauen\nE interagieren · I Inventar · 1 essen · 2 trinken · P Pause · F5 speichern · F9 laden\nSteam Deck: linker Stick bewegen · rechter Stick zielen · RT / X abbauen\nA interagieren / bestätigen · Y Inventar · B zurück · Start Pause\nMenüs: Steuerkreuz / Pfeile, A / Enter. Die Welt pausiert in Menüs; ein laufender Replikator arbeitet weiter.");
                Text("Gelbgrüne Sporen im Osten vergiften; kalte Gewässer im Westen machen krank. Ein Schutzanzug reduziert beide Gefahren. Tiere sind freundlich. Bei Bewusstlosigkeit rettet dich die Kapsel ohne Ressourcenverlust. Solar-Notversorgung verhindert einen Energiestillstand.");
                Button("Zurück",()=>ShowMenu("title"));break;
            case "pause":
                Text("Expedition pausiert",32);Button("Weiterspielen",()=>ShowMenu(""));
                Button("Speichern",()=>{if(Save())Notify("Expedition gespeichert.");});
                Button("Letzten Spielstand laden",()=>{if(Load())ShowMenu("");},FileAccess.FileExists(SavePath));
                Button("Speichern & zum Titel",()=>{Save();ShowMenu("title");});break;
            case "base":
                Text("Kapsel · Heimathafen",32);Text($"Energie {M.S.Energy:0}/{M.MaxEnergy:0} · Speicherstufe {M.S.Memory} · Solar-Notladung aktiv");
                Button("Replikator",()=>ShowMenu("craft"));Button("Komprimierte Sicherungen / Technologiebaum",()=>ShowMenu("tech"));
                Button("Bio-Bett · 18 Energie",Heal,M.S.Energy>=18);
                Button("Elekrerium einspeisen · +25 Energie",Fuel,M.Count("elekrerium",true)>0&&M.S.Energy<M.MaxEnergy);
                if(M.S.Beacon) Button(M.S.Rescued?"Rettung bestätigt · Entscheidung ansehen":"Notrufsender · Signalstatus",()=>ShowMenu("rescue"));
                Button("Inventar & Baugegenstände",()=>ShowMenu("bag"));Button("Zurück zur Welt",()=>ShowMenu(""));break;
            case "craft":
                Text("Replikator",32);Text("Verwendet Rucksack und Kisten im Basisradius (360 m). Herstellung läuft auch bei geschlossenem Menü.");
                Text(M.S.PendingRecipe==""?"Bereit.":$"Produktion: {M.Name(M.Recipes[M.S.PendingRecipe].Output)} · {M.S.JobRemaining:0.0} s");
                foreach(var r in M.Recipes.Values.Where(r=>M.Unlocked(r.Id)))
                {
                    var recipe=r;
                    Button($"{r.Name}  ×{r.Amount}    |    {Cost(r.Cost)} · {r.Energy:0} E · {r.Seconds:0}s",()=>Craft(recipe),M.S.PendingRecipe==""&&M.S.Energy>=r.Energy&&r.Cost.All(c=>M.Count(c.Key,true)>=c.Value));
                }
                Button("Zurück",()=>ShowMenu("base"));break;
            case "tech":
                Text("Gedächtnis der Kapsel",32);Text($"Speicherstufe {M.S.Memory} · Bereits entpackte Baupläne bleiben dauerhaft verfügbar.");
                Button($"Speicher erweitern · {8+M.S.Memory*8} Ehternum",()=>{if(M.ExpandMemory())Notify("Speicher erweitert.");else Notify("Nicht genug Ehternum.");ShowMenu("tech");},M.S.Memory<6);
                foreach(var t in M.Technologies.Values)
                {
                    var tech=t;bool unlocked=M.S.Tech.Contains(t.Id);
                    Text($"{(unlocked?"✓":"○")} {t.Name} · Stufe {t.Memory}\nVoraussetzungen: {(t.Requires.Count==0?"keine":string.Join(", ",t.Requires.Select(id=>M.Technologies[id].Name)))}\nBaupläne: {string.Join(", ",t.Recipes.Select(id=>M.Recipes[id].Name))}");
                    if(!unlocked)Button($"Sicherung entpacken · {Cost(t.Cost)}",()=>{if(M.Unlock(tech.Id))Notify("Baupläne wiederhergestellt.");else Notify("Speicher, Voraussetzungen oder Rohstoffe fehlen.");ShowMenu("tech");});
                }
                Button("Zurück",()=>ShowMenu("base"));break;
            case "bag":
                Text($"Rucksack · {M.Slots(M.S.Bag)}/16 Stapelplätze",30);
                Text($"Ausrüstung: Laser {(M.S.LaserUpgrade?"II":"I")} · {(M.S.Suit?"Schutzanzug":"Standardanzug")}\nVerletzung {M.S.Injury:0} · Krankheit {M.S.Illness:0} · Vergiftung {M.S.Poison:0}");
                foreach(var pair in M.S.Bag.ToArray())
                {
                    string id=pair.Key; string kind=M.Items[id].Kind;
                    if(new[]{"food","water","medicine","laser","cell","suit"}.Contains(kind)) Button($"{M.Name(id)} ×{pair.Value} · Benutzen / Ausrüsten",()=>{Notify(M.Consume(id)?"Verwendet.":"Wird gerade nicht benötigt.");ShowMenu("bag");});
                    else if(new[]{"chest","garden","beacon"}.Contains(kind)) Button($"{M.Name(id)} ×{pair.Value} · Platzieren",()=>Place(id));
                    else Text($"{M.Name(id)} ×{pair.Value}");
                }
                Button("Zurück",()=>ShowMenu(""));break;
            case "chest":
                var chest=M.S.Chests[_chest];
                Text($"Lagerkiste · {M.Slots(chest.Items)}/24 Stapelplätze",30);
                Text(Model.NearBase(chest)?"Mit dem Replikator verbunden.":"Außenlager · außerhalb des Basisradius.");
                Text("Einlagern · bis zu 10 Einheiten pro Auswahl");
                foreach(var x in M.S.Bag.ToArray()) { var id=x.Key;Button($"→ {M.Name(id)} ×{x.Value}",()=>{M.Transfer(chest,id,true);ShowMenu("chest");}); }
                Text("Entnehmen");
                foreach(var x in chest.Items.ToArray()) {var id=x.Key;Button($"← {M.Name(id)} ×{x.Value}",()=>{M.Transfer(chest,id,false);ShowMenu("chest");});}
                Button("Zurück",()=>ShowMenu(""));break;
            case "rescue":
                Text("Ein Zeichen zwischen den Sternen",32);
                Text(M.S.Rescued?"Die Antwort ist da. Ein Rettungsschiff hat deine Position bestätigt. Du hast überlebt – und diesem Ort Leben gegeben.":$"Notrufsender aktiv. Signalaufbau: {M.S.Signal:0}/60 Sekunden.\nDer Sender braucht 0,25 Energie pro Sekunde. Lass die Zeit in der Welt weiterlaufen.");
                if(M.S.Rescued) {Button("Hierbleiben und die neue Heimat ausbauen",()=>ShowMenu(""));Button("Rettung annehmen · Epilog",()=>ShowMenu("ending"));}
                Button("Zurück",()=>ShowMenu(""));break;
            case "ending":
                Text("Du bist nicht mehr gestrandet.",34);Text("Als das Rettungsschiff landet, wartet ein neugieriges Tier neben deiner Kapsel.\nDu hast eine Wahl – und einen Ort, an den du zurückkehren kannst.\n\nExpedition abgeschlossen. Dein Spielstand bleibt erhalten.");
                Button("Frei weiterspielen",()=>ShowMenu(""));Button("Speichern & zum Titel",()=>{Save();ShowMenu("title");});break;
        }
        var first=_list.GetChildren().OfType<Button>().FirstOrDefault(b=>!b.Disabled);
        if(first!=null)Callable.From(()=>{if(GodotObject.IsInstanceValid(first)&&first.IsInsideTree())first.GrabFocus();}).CallDeferred();
    }
    private string Cost(Dictionary<string,int> costs)=>costs.Count==0?"kostenlos":string.Join(" · ",costs.Select(c=>$"{c.Value} {M.Name(c.Key)}"));
    private void NewGame()
    {
        M.S=new SaveState();foreach(var d in _world.Deposits){d.Units=8;d.Progress=0;}_autosave=0;_camera.Position=Player;ShowMenu("");
        Notify("Du lebst. Die Kapsel wartet hinter dir. E / A öffnet ihre Systeme.");
    }
    private void Craft(RecipeDef r)
    {
        if(M.S.PendingRecipe!=""||M.S.Energy<r.Energy)return;
        if(!M.Fits(M.S.Bag,r.Output,r.Amount,16)){Notify("Rucksack voll. Erst Platz schaffen.");return;}
        if(!M.Pay(r.Cost,true)){Notify("Rohstoffe fehlen.");return;}
        M.S.Energy-=r.Energy;M.S.PendingRecipe=r.Id;M.S.JobRemaining=r.Seconds;ShowMenu("craft");
    }
    private void FinishJob()
    {
        var r=M.Recipes[M.S.PendingRecipe];
        if(!M.Add(r.Output,r.Amount))return; // Completed product waits safely until bag space is available.
        M.S.PendingRecipe="";M.S.JobRemaining=0;Notify($"Repliziert: {r.Name}");
        if(_menu=="craft")ShowMenu("craft");
    }
    private void Fuel()
    {
        if(M.S.Energy>=M.MaxEnergy)return;
        if(M.Pay(new(){["elekrerium"]=1},true)) M.S.Energy=Math.Min(M.MaxEnergy,M.S.Energy+25);
        ShowMenu("base");
    }
    private void Heal()
    {
        if(M.S.Energy<18)return;
        M.S.Energy-=18;M.S.Health=100;M.S.Poison=0;M.S.Illness=0;M.S.Injury=0;
        Notify("Bio-Bett: Behandlung abgeschlossen.");ShowMenu("base");
    }
    private void Place(string id)
    {
        string kind=M.Items[id].Kind;
        if(kind!="chest" && Player.Length()>300){Notify("Dieses Gerät gehört zur Kapsel. Gehe näher heran.");return;}
        if(kind=="garden"&&M.S.Garden||kind=="beacon"&&M.S.Beacon){Notify("Bereits aufgebaut.");return;}
        var pos=Player+Facing*58;
        if(kind=="chest"&&(pos.Length()<100||M.S.Chests.Any(c=>new Vector2(c.X,c.Y).DistanceTo(pos)<65))) {Notify("Zu wenig Platz. Gehe etwas weiter.");return;}
        if(!M.Pay(new(){[id]=1},false))return;
        if(kind=="chest")M.S.Chests.Add(new(){X=pos.X,Y=pos.Y});
        else if(kind=="garden")M.S.Garden=true;
        else if(kind=="beacon")M.S.Beacon=true;
        Notify("Aufgebaut.");ShowMenu("");Save();
    }
    public void Notify(string text){_message=text;_messageTime=6;}
    public override void _Process(double delta)
    {
        if(M==null)return;
        float dt=Math.Min((float)delta,.1f);Clock+=dt;_resumeDelay=Math.Max(0,_resumeDelay-dt);
        if(_messageTime>0)_messageTime-=dt;
        if(_menu!="title"&&_menu!="confirm"&&_menu!="help" && M.S.PendingRecipe!="")
        {
            M.S.JobRemaining=Math.Max(0,M.S.JobRemaining-dt);if(M.S.JobRemaining<=0)FinishJob();
        }
        if(Input.IsActionJustPressed("pause")&&_menu!="title"&&_menu!="help"&&_menu!="confirm")ShowMenu(_menu=="pause"?"":"pause");
        if(Input.IsActionJustPressed("close")&&_menu!="title")ShowMenu(_menu==""?"pause":"");
        if(_menu==""&&_resumeDelay<=0)
        {
            if(Input.IsActionJustPressed("bag"))ShowMenu("bag");
            if(Input.IsActionJustPressed("save")&&Save())Notify("Gespeichert.");
            if(Input.IsActionJustPressed("load"))Load();
            if(Input.IsActionJustPressed("food"))M.Consume("food");
            if(Input.IsActionJustPressed("water"))M.Consume("water");
            if(_menu=="")Simulate(dt);
        }
        _camera.Position=Player;_world.QueueRedraw();
        _uiTick+=dt;if(_uiTick>.12f){_uiTick=0;UpdateHud();}
    }
    private void Simulate(float dt)
    {
        var stick=new Vector2(Input.GetJoyAxis(0,JoyAxis.LeftX),Input.GetJoyAxis(0,JoyAxis.LeftY));
        if(stick.Length()<.2f)stick=Vector2.Zero;
        var move=Input.GetVector("left","right","up","down")+stick;
        if(move.Length()>1)move=move.Normalized();
        var aim=new Vector2(Input.GetJoyAxis(0,JoyAxis.RightX),Input.GetJoyAxis(0,JoyAxis.RightY));
        if(aim.Length()>.25f)Facing=aim.Normalized();else if(move.Length()>.1f)Facing=move.Normalized();
        if(Input.IsMouseButtonPressed(MouseButton.Left))Facing=(GetGlobalMousePosition()-Player).Normalized();
        float speed=155*(1-M.S.Injury*.003f);
        var next=Player+move*speed*dt;
        // Capsule hull and crates have simple circular collision bounds.
        if(next.Length()<83)next=next.Normalized()*83;
        foreach(var c in M.S.Chests)
        {var diff=next-new Vector2(c.X,c.Y);if(diff.Length()<34)next=new Vector2(c.X,c.Y)+(diff.Length()<.1f?Vector2.Down:diff.Normalized())*34;}
        M.S.X=Math.Clamp(next.X,-1800,1800);M.S.Y=Math.Clamp(next.Y,-1400,1400);
        Mining=false;
        if(Input.IsActionPressed("mine"))
        {
            var target=_world.Deposits.Where(d=>d.Position.DistanceTo(Player)<145 && M.S.Deposits.GetValueOrDefault(d.Id)<M.S.Time)
                .OrderBy(d=>d.Position.DistanceTo(Player)+(1-Facing.Dot((d.Position-Player).Normalized()))*80).FirstOrDefault();
            if(target!=null)
            {
                Mining=true;BeamEnd=target.Position;target.Progress+=dt*(M.S.LaserUpgrade?2:1);
                if(target.Progress>=1.1f)
                {
                    target.Progress=0;
                    if(M.Add(target.Resource,1)) {target.Units--;if(target.Units<=0){target.Units=8;M.S.Deposits[target.Id]=M.S.Time+75;} M.S.DepositUnits[target.Id]=target.Units; }
                    else Notify("Rucksack voll. Lagere Ressourcen ein.");
                }
            }
        }
        if(Input.IsActionJustPressed("interact"))
        {
            int i=M.S.Chests.FindIndex(c=>new Vector2(c.X,c.Y).DistanceTo(Player)<85);
            var animal=_world.Animals.FirstOrDefault(a=>a.Position.DistanceTo(Player)<90);
            if(i>=0){_chest=i;ShowMenu("chest");}
            else if(Player.Length()<220)ShowMenu("base");
            else if(animal!=null){animal.Affection=5;Notify("Das Tier schmiegt sich neugierig an deinen Anzug.");}
            else Notify("Nähere dich der Kapsel, einer Kiste oder einem Tier.");
        }
        M.S.Time+=dt;M.S.Energy=Math.Min(M.MaxEnergy,M.S.Energy+dt*.12f);
        M.S.Hunger=Math.Max(0,M.S.Hunger-dt*.065f);M.S.Thirst=Math.Max(0,M.S.Thirst-dt*.095f);
        float protection=M.S.Suit?.18f:1;
        if(World.InSpores(Player))M.S.Poison=Math.Min(100,M.S.Poison+dt*2.2f*protection);
        else M.S.Poison=Math.Max(0,M.S.Poison-dt*.06f);
        if(World.InWater(Player))M.S.Illness=Math.Min(100,M.S.Illness+dt*1.4f*protection);
        if(move.Length()>.1f&&World.InRubble(Player))M.S.Injury=Math.Min(100,M.S.Injury+dt*.65f*protection);
        float damage=(M.S.Hunger==0?.8f:0)+(M.S.Thirst==0?1.2f:0)+M.S.Poison*.008f+M.S.Illness*.003f;
        M.S.Health=Math.Max(0,M.S.Health-damage*dt);
        if(M.S.Garden&&Player.Length()<260){M.S.Hunger=Math.Min(100,M.S.Hunger+dt*.45f);M.S.Thirst=Math.Min(100,M.S.Thirst+dt*.5f);}
        if(M.S.Beacon&&!M.S.Rescued&&M.S.Energy>.25f*dt)
        {M.S.Energy-=.25f*dt;M.S.Signal+=dt;if(M.S.Signal>=60){M.S.Rescued=true;Notify("Ein Schiff hat geantwortet! Kehre zum Sender an der Kapsel zurück.");Save();}}
        if(M.S.Health<=0)
        {
            M.S.X=150;M.S.Y=90;M.S.Health=65;M.S.Hunger=45;M.S.Thirst=45;M.S.Poison=0;M.S.Illness=0;M.S.Injury=0;
            Notify("Die Kapsel hat dich geborgen. Dein Inventar ist erhalten.");
        }
        foreach(var a in _world.Animals)a.Tick(dt,Player);
        _autosave+=dt;if(_autosave>45){_autosave=0;Save();}
    }
    private void UpdateHud()
    {
        _stats.Text=$"GESUNDHEIT {M.S.Health:0}    NAHRUNG {M.S.Hunger:0}    WASSER {M.S.Thirst:0}\nENERGIE {M.S.Energy:0}/{M.MaxEnergy:0}    SPEICHER {M.S.Memory}    RUCKSACK {M.Slots(M.S.Bag)}/16";
        _objective.Text=M.S.Rescued?"Rettung erreicht · Baue deine Heimat weiter aus":M.S.Beacon?$"Notrufsignal {M.S.Signal:0}/60 s":M.S.Garden?"Nächstes Ziel: Notrufsender aufbauen":"Ziel: Habitatgarten & Notrufsender wiederherstellen";
        string nearby=Player.Length()<220?"E / A · Kapsel öffnen":"E / A · Interagieren";
        _hint.Text=$"{nearby}    I / Y · Inventar    Leertaste / RT · Laser\n{(World.InSpores(Player)?"WARNUNG: Giftsporen":World.InWater(Player)?"WARNUNG: Kältezonen":World.InRubble(Player)?"Vorsicht: scharfkantige Trümmer":"P / Start · Pause & Speichern")}";
        _toast.Text=_messageTime>0?_message:"";
        _jobBar.Visible=M.S.PendingRecipe!="";
        if(_jobBar.Visible)_jobBar.Value=100*(1-M.S.JobRemaining/M.Recipes[M.S.PendingRecipe].Seconds);
    }
    private bool Save()
    {
        try
        {
            string temp=SavePath+".tmp";using(var f=FileAccess.Open(temp,FileAccess.ModeFlags.Write))
            {if(f==null)throw new Exception(FileAccess.GetOpenError().ToString());f.StoreString(JsonSerializer.Serialize(M.S,Model.Json));}
            var err=DirAccess.RenameAbsolute(ProjectSettings.GlobalizePath(temp),ProjectSettings.GlobalizePath(SavePath));
            if(err!=Error.Ok)throw new Exception(err.ToString());return true;
        }
        catch(Exception e){Notify("Speichern fehlgeschlagen: "+e.Message);GD.PushError(e.Message);return false;}
    }
    private bool Load()
    {
        try
        {
            if(!FileAccess.FileExists(SavePath))throw new Exception("Noch kein Spielstand vorhanden.");
            M.Load(FileAccess.GetFileAsString(SavePath));
            if(M.S.PendingRecipe!=""&&!M.Recipes.ContainsKey(M.S.PendingRecipe))throw new Exception("Rezept im Spielstand fehlt.");
            foreach(var d in _world.Deposits){d.Progress=0;d.Units=M.S.DepositUnits.GetValueOrDefault(d.Id,8);}
            _camera.Position=Player;Notify("Expedition geladen.");return true;
        }
        catch(Exception e){Notify("Laden fehlgeschlagen: "+e.Message);GD.PushError(e.Message);return false;}
    }
    public override void _Notification(int what)
    {
        if(what==NotificationWMCloseRequest){if(_menu!="title"&&_menu!="confirm"&&_menu!="help")Save();GetTree().Quit();}
    }
}
