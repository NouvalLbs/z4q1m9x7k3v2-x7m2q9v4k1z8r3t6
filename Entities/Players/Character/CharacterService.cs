#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Feature.CinematicCamera;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Plugins.Streamer;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectSMP.Entities.Players.Character
{
    public class CharPosition { public float X, Y, Z, A; public int Interior, World; }
    public class CharVitals { public float MaxHealth = 100, Health = 100, Armour, Hunger = 100, Energy = 100; }
    public class CharPlaytime { public int Hours, Minutes, Seconds; }
    public class CharBackpack { public int Enabled, Slots = 32, MaxWeight = 60000; }
    public class CharPhone { public int Number, Off, Credit; }
    public class CharJailInfo { public int Jailed, Time; public string Reason = ""; }
    public class CharBanInfo { public int Banned, Time, Expire; public string Reason = "", Admin = ""; }

    public class CharCondition
    {
        public int DyingTime, RespawnTime, Hospital, Injured;
        public int Cough, CoughTime;
        public int Migrain, MigrainTime, MigrainUsed;
        public int Fever, FeverTime, FeverUsed;
        public int DrugTime, DrugUsed;
    }

    public class CharSettings
    {
        public Language Language { get; set; } = Language.ID;
        public int EnterExit = 4;
        public int DynamicObjectsPriority = 1;
        public int HBEMode;
        public bool ShowHealth;
        public bool ShowArmour;
        public bool ShowHunger = true;
        public bool ShowThirst = true;
        public bool ShowStress = true;
        public bool ShowTime = true;
        public bool ToggleJoinLog;
        public bool ToggleNews = true;
        public bool ToggleQuiz;
        public bool ToggleAdvertise = true;
        public bool ToggleUppercase = true;
        public bool ToggleStreamerMode;
        public bool ToggleChatOOC;
        public bool ToggleFamilyChat;
        public bool ToggleWTChat = true;
        public bool ToggleFacRadioChat;
        public bool TogglePrivateMessage = true;
        public bool ToggleAdminCmdLog = true;
        public bool ToggleAutoLowChat;
        public bool ToggleAutoHandbrake;
        public bool ToggleAutoChatAnimation;
        public bool ToggleSeatbeltHelmet;
        public bool ToggleAutoMaskLoggedIn;
    }

    public class CharJob
    {
        public string JobName = "";
        public string RegisterDate = "";
    }

    internal sealed class CharCreationData
    {
        public string Name { get; set; } = "";
        public string BirthDate { get; set; } = "-";
        public int Gender { get; set; }
        public int Height { get; set; } = 150;
        public string Hair { get; set; } = "Hitam";
        public string Eye { get; set; } = "";
        public int Skin { get; set; } = 2;
        public int WhichSpawn { get; set; }
    }

    internal sealed class RawCharRow
    {
        public string citizen_id { get; set; } = "";
        public string ucp { get; set; } = "";
        public string ip { get; set; } = "";
        public string reg_date { get; set; } = "";
        public string last_login { get; set; } = "";
        public int verified_char { get; set; }
        public string username { get; set; } = "";
        public int skin { get; set; }
        public int gender { get; set; }
        public string birth_date { get; set; } = "";
        public int height { get; set; } = 150;
        public string hair { get; set; } = "";
        public string eye { get; set; } = "";
        public int level { get; set; } = 1;
        public int level_points { get; set; }
        public int level_points_exp { get; set; }
        public int money { get; set; }
        public int admin { get; set; }
        public int mask_id { get; set; }
        public int warn { get; set; }
        public int paycheck { get; set; }
        public string? position { get; set; }
        public string? vitals { get; set; }
        public string? playtime { get; set; }
        public string? backpack { get; set; }
        public string? phone { get; set; }
        public string? jail_info { get; set; }
        public string? ban_info { get; set; }
        public string? condition { get; set; }
        public string? settings { get; set; }
        public string? jobs { get; set; }
    }

    internal sealed class CharListItem
    {
        public string citizen_id { get; set; } = "";
        public string username { get; set; } = "";
        public int level { get; set; }
        public string last_login { get; set; } = "";
    }

    internal static class CharacterService
    {
        private const string Table = "players";
        private const int MaxChars = 25;

        private static readonly (int Skin, string Hair)[] MaleSkins =
        {
            (1,"Hitam"),(2,"Hitam"),(3,"Hitam"),(4,"Hitam"),(5,"Hitam"),
            (6,"Hitam"),(7,"Hitam"),(14,"Hitam"),(29,"Hitam"),(100,"Hitam"),(299,"Hitam")
        };

        private static readonly (int Skin, string Hair)[] FemaleSkins =
        {
            (9,"Hitam"),(10,"Hitam"),(11,"Hitam"),(12,"Hitam"),(13,"Hitam"),
            (31,"Hitam"),(38,"Hitam"),(39,"Hitam"),(40,"Hitam"),(41,"Hitam")
        };

        private static readonly Regex _rpName = new(@"^[A-Z][a-z]+_[A-Z][a-z]+$", RegexOptions.Compiled);
        private static readonly JsonSerializerOptions _jOpts = new() { PropertyNameCaseInsensitive = true, IncludeFields = true };
        private static readonly Dictionary<int, List<CharListItem>> _lists = new();
        private static readonly Dictionary<int, CharCreationData> _creations = new();

        private static Language Lang(Player p) => p.IsCharLoaded ? p.Settings.Language : Language.ID;
        private static string L(Player p, string s, string k) => LocalizationManager.Get(Lang(p), s, k);
        private static string L(Player p, string s, string k, params object[] a) => LocalizationManager.Get(Lang(p), s, k, a);

        public static async void CheckPlayerCharAsync(Player player)
        {
            var rows = await DatabaseManager.QueryAsync<CharListItem>(
                $"SELECT citizen_id, username, level, last_login FROM `{Table}` WHERE ucp = @Ucp LIMIT {MaxChars}",
                new { Ucp = player.Name });

            if (player.IsDisposed) return;

            var list = new List<CharListItem>(rows);
            _lists[player.Id] = list;
            ShowCharListDialog(player, list);
        }

        public static void HandleSpawn(Player player)
        {
            if (!player.IsCharLoaded) return;

            player.ToggleControllable(true);
            player.Score = player.Level;

            for (var i = 0; i < 50; i++) player.SendClientMessage(Color.White, "");
            player.SendClientMessage(Color.White, L(player, "CHAR", "WELCOME_1"));
            player.SendClientMessage(Color.White, L(player, "CHAR", "WELCOME_2", player.Username));
            player.SendClientMessage(Color.White, L(player, "CHAR", "WELCOME_3"));
            player.SendClientMessage(Color.White, L(player, "CHAR", "WELCOME_LAST_LOGIN", player.LastLogin));
        }

        public static async Task SaveAsync(Player player)
        {
            if (!player.IsCharLoaded) return;

            player.Vitals.Health = player.Health;
            player.Vitals.Armour = player.Armour;

            var pos = new CharPosition
            {
                X = player.Position.X,
                Y = player.Position.Y,
                Z = player.Position.Z,
                A = player.Angle,
                Interior = player.Interior,
                World = player.VirtualWorld
            };

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET " +
                "level=@Level, level_points=@LevelPoints, level_points_exp=@LevelPointsExp, " +
                "money=@Money, admin=@Admin, mask_id=@MaskId, warn=@Warn, paycheck=@Paycheck, " +
                "ip=@Ip, last_login=CURRENT_TIMESTAMP(), " +
                "position=@Pos, vitals=@Vitals, playtime=@Playtime, " +
                "backpack=@Backpack, phone=@Phone, jail_info=@JailInfo, ban_info=@BanInfo, " +
                "`condition`=@Condition, `settings`=@Settings, jobs=@Jobs " +
                "WHERE citizen_id=@CitizenId",
                new
                {
                    player.Level,
                    LevelPoints = player.LevelPoints,
                    LevelPointsExp = player.LevelPointsExp,
                    Money = player.CharMoney,
                    player.Admin,
                    player.MaskId,
                    player.Warn,
                    player.Paycheck,
                    Ip = player.IP,
                    Pos = Ser(pos),
                    Vitals = Ser(player.Vitals),
                    Playtime = Ser(player.Playtime),
                    Backpack = Ser(player.Backpack),
                    Phone = Ser(player.Phone),
                    JailInfo = Ser(player.JailInfo),
                    BanInfo = Ser(player.BanInfo),
                    Condition = Ser(player.Condition),
                    Settings = Ser(player.Settings),
                    Jobs = Ser(player.Jobs),
                    CitizenId = player.CitizenId
                });
        }

        public static void Cleanup(Player player)
        {
            _lists.Remove(player.Id);
            _creations.Remove(player.Id);
            player.IsCharLoaded = false;
        }

        private static void ShowCharListDialog(Player player, List<CharListItem> list)
        {
            var rows = new List<string[]>();
            foreach (var c in list)
                rows.Add(new[] { $"{{ffffff}}{c.username}", c.level.ToString(), c.last_login });
            if (list.Count < MaxChars)
                rows.Add(new[] { L(player, "CHAR", "LIST_CREATE_BTN"), "", "" });

            DialogManager.ShowTabList(player,
                L(player, "CHAR", "LIST_TITLE"),
                new[] { L(player, "CHAR", "LIST_COL_NAME"), L(player, "CHAR", "LIST_COL_LEVEL"), L(player, "CHAR", "LIST_COL_LOGIN") },
                rows.ToArray(),
                L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_QUIT"),
                e =>
                {
                    if (e.DialogButton != DialogButton.Left) { player.Kick(); return; }
                    if (!_lists.TryGetValue(player.Id, out var l)) return;
                    if (e.ListItem < l.Count) LoadExistingCharAsync(player, l[e.ListItem].citizen_id);
                    else ShowCreateNameDialog(player);
                });
        }

        private static void ShowCreateNameDialog(Player player, bool taken = false)
        {
            var body = taken
                ? L(player, "CHAR", "CREATE_NAME_TAKEN_MSG")
                : L(player, "CHAR", "CREATE_NAME_MSG");

            DialogManager.ShowInput(player, L(player, "CHAR", "CREATE_NAME_TITLE"), body,
                btnLeft: L(player, "GENERAL", "BTN_CREATE"), btnRight: L(player, "GENERAL", "BTN_EXIT"),
                onResponse: e =>
                {
                    if (e.DialogButton != DialogButton.Left) { player.Kick(); return; }
                    HandleCreateNameAsync(player, e.InputText);
                });
        }

        private static async void HandleCreateNameAsync(Player player, string name)
        {
            if (name.Length < 1 || name.Length > 24 || !_rpName.IsMatch(name))
            {
                ShowCreateNameDialog(player);
                return;
            }

            var taken = await DatabaseManager.ExistsAsync(
                $"SELECT COUNT(*) FROM `{Table}` WHERE username = @Name", new { Name = name });

            if (player.IsDisposed) return;
            if (taken) { ShowCreateNameDialog(player, taken: true); return; }

            _creations[player.Id] = new CharCreationData { Name = name };
            EnterCreationScene(player);
            ShowSettingsDialog(player);
        }

        private static void EnterCreationScene(Player player)
        {
            CinematicCameraService.Stop(player);
            player.Interior = 14;
            player.VirtualWorld = player.Id;

            var pos = new Vector3(255.306320f, -41.813072f, 1002.023437f);
            player.Position = pos;
            player.Angle = 257.241699f;
            player.CameraPosition = pos;
            player.SetCameraLookAt(pos, CameraCut.Cut);

            StreamerNatives.Instance.Streamer_UpdateEx(player.Id, pos.X, pos.Y, pos.Z);
            StreamerNatives.Instance.Streamer_Update(player.Id);
        }

        private static void ShowSettingsDialog(Player player)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;

            DialogManager.ShowTabList(player,
                L(player, "CHAR", "SETTINGS_TITLE"),
                new[] { L(player, "CHAR", "SETTINGS_COL_DATA"), L(player, "CHAR", "SETTINGS_COL_VALUE") },
                new[]
                {
                    new[] { L(player, "CHAR", "SETTINGS_ROW_NAME"),      $"{{ffffff}}{c.Name}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_BIRTHDATE"), $"{{ffffff}}{c.BirthDate}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_GENDER"),    $"{{ffffff}}{FmtGender(player, c.Gender)}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_HEIGHT"),    $"{{ffffff}}{c.Height} cm" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_HAIR"),      $"{{ffffff}}{c.Hair}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_EYE"),       $"{{ffffff}}{c.Eye}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_CREATE"),    "" }
                },
                L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_CANCEL"),
                e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        _lists.TryGetValue(player.Id, out var l);
                        ShowCharListDialog(player, l ?? new());
                        return;
                    }
                    HandleSettingItem(player, e.ListItem);
                });
        }

        private static void HandleSettingItem(Player player, int item)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;
            switch (item)
            {
                case 0:
                    player.SendClientMessage(Color.White, L(player, "CHAR", "SETTINGS_ERR_NAME"));
                    ShowSettingsDialog(player);
                    break;
                case 1: ShowBirthDateDialog(player); break;
                case 2: ShowGenderDialog(player); break;
                case 3: ShowHeightDialog(player); break;
                case 4:
                    player.SendClientMessage(Color.White, L(player, "CHAR", "SETTINGS_ERR_HAIR"));
                    ShowSettingsDialog(player);
                    break;
                case 5: ShowEyeColorDialog(player); break;
                case 6:
                    if (c.BirthDate is "-" or { Length: <= 1 })
                    { player.SendClientMessage(Color.White, L(player, "CHAR", "SETTINGS_ERR_BIRTHDATE")); ShowSettingsDialog(player); return; }
                    if (string.IsNullOrWhiteSpace(c.Eye))
                    { player.SendClientMessage(Color.White, L(player, "CHAR", "SETTINGS_ERR_EYE")); ShowSettingsDialog(player); return; }
                    ShowSpawnSelectorDialog(player);
                    break;
            }
        }

        private static void ShowBirthDateDialog(Player player, string? errKey = null)
        {
            var body = L(player, "CHAR", "BIRTHDATE_MSG");
            if (errKey != null) body += $"\n{{ff0000}}(!) " + L(player, "CHAR", errKey);

            DialogManager.ShowInput(player, L(player, "CHAR", "BIRTHDATE_TITLE"), body,
                btnLeft: L(player, "GENERAL", "BTN_SELECT"), btnRight: L(player, "GENERAL", "BTN_CANCEL"),
                onResponse: e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSettingsDialog(player); return; }
                    HandleBirthDate(player, e.InputText);
                });
        }

        private static void HandleBirthDate(Player player, string input)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;
            var parts = input.Split('/');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var d) ||
                !int.TryParse(parts[1], out var m) || !int.TryParse(parts[2], out var y))
            { ShowBirthDateDialog(player, "BIRTHDATE_ERR_INVALID"); return; }

            int[] mDays = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            if (y < 1900 || y > DateTime.Now.Year) { ShowBirthDateDialog(player, "BIRTHDATE_ERR_YEAR"); return; }
            if (m < 1 || m > 12) { ShowBirthDateDialog(player, "BIRTHDATE_ERR_MONTH"); return; }
            if (d < 1 || d > mDays[m - 1]) { ShowBirthDateDialog(player, "BIRTHDATE_ERR_DAY"); return; }

            c.BirthDate = input;
            ShowSettingsDialog(player);
        }

        private static void ShowGenderDialog(Player player)
        {
            DialogManager.ShowList(player,
                L(player, "CHAR", "GENDER_TITLE"),
                new[] { L(player, "CHAR", "GENDER_MALE"), L(player, "CHAR", "GENDER_FEMALE") },
                L(player, "GENERAL", "BTN_INPUT"), L(player, "GENERAL", "BTN_CANCEL"),
                e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSettingsDialog(player); return; }
                    if (!_creations.TryGetValue(player.Id, out var c)) return;
                    c.Gender = e.ListItem;
                    ShowSkinDialog(player);
                });
        }

        private static void ShowSkinDialog(Player player)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;
            var skinList = c.Gender == 1 ? FemaleSkins : MaleSkins;
            var items = new List<PreviewModelItem>();
            foreach (var s in skinList)
                items.Add(new PreviewModelItem { ModelId = s.Skin, Text = $"ID: {s.Skin}" });

            PreviewModelDialog.Show(player, 0, FmtGender(player, c.Gender), items,
                L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_CANCEL"),
                args =>
                {
                    if (!args.Accepted) { ShowGenderDialog(player); return; }
                    if (!_creations.TryGetValue(player.Id, out var cc)) return;
                    var sk = (cc.Gender == 1 ? FemaleSkins : MaleSkins)[args.ListItem];
                    cc.Skin = sk.Skin;
                    cc.Hair = sk.Hair;
                    player.Skin = cc.Skin;
                    ShowSettingsDialog(player);
                });
        }

        private static void ShowHeightDialog(Player player, string? errKey = null)
        {
            var body = L(player, "CHAR", "HEIGHT_MSG");
            if (errKey != null) body += $"\n{{ff0000}}(!) " + L(player, "CHAR", errKey);

            DialogManager.ShowInput(player, L(player, "CHAR", "HEIGHT_TITLE"), body,
                btnLeft: L(player, "GENERAL", "BTN_INPUT"), btnRight: L(player, "GENERAL", "BTN_CANCEL"),
                onResponse: e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSettingsDialog(player); return; }
                    HandleHeight(player, e.InputText);
                });
        }

        private static void HandleHeight(Player player, string input)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;
            if (!int.TryParse(input, out var h)) { ShowHeightDialog(player, "HEIGHT_ERR_NAN"); return; }
            if (h < 130 || h > 195) { ShowHeightDialog(player, "HEIGHT_ERR_RANGE"); return; }
            c.Height = h;
            ShowSettingsDialog(player);
        }

        private static void ShowEyeColorDialog(Player player)
        {
            DialogManager.ShowList(player,
                L(player, "CHAR", "EYE_TITLE"),
                new[] { L(player, "CHAR", "EYE_BLACK"), L(player, "CHAR", "EYE_BROWN"), L(player, "CHAR", "EYE_BLUE"), L(player, "CHAR", "EYE_GRAY") },
                L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_CANCEL"),
                e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSettingsDialog(player); return; }
                    if (!_creations.TryGetValue(player.Id, out var c)) return;
                    c.Eye = e.ListItem switch { 0 => "Hitam", 1 => "Coklat", 2 => "Biru", _ => "Abu-abu Muda" };
                    ShowSettingsDialog(player);
                });
        }

        private static void ShowSpawnSelectorDialog(Player player)
        {
            DialogManager.ShowList(player,
                L(player, "CHAR", "SPAWN_TITLE"),
                new[] { L(player, "CHAR", "SPAWN_AIRPORT"), L(player, "CHAR", "SPAWN_STATION") },
                L(player, "CHAR", "SPAWN_BTN"), "",
                e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSpawnSelectorDialog(player); return; }
                    if (!_creations.TryGetValue(player.Id, out var c)) return;
                    c.WhichSpawn = e.ListItem;
                    RegisterNewCharAsync(player);
                });
        }

        private static async void RegisterNewCharAsync(Player player)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;

            string cid;
            do { cid = GenCitizenId(); }
            while (await DatabaseManager.ExistsAsync(
                $"SELECT COUNT(*) FROM `{Table}` WHERE citizen_id = @Id", new { Id = cid }));

            if (player.IsDisposed) return;

            var pos = c.WhichSpawn == 0
                ? new CharPosition { X = 1685.602172f, Y = -2239.236572f, Z = 13.546875f, A = 182.176986f }
                : new CharPosition { X = 1773.370117f, Y = -1936.799072f, Z = 13.552585f, A = 322.835693f };

            var maskId = new Random().Next(111111, 988888);

            await DatabaseManager.ExecuteAsync(
                $"INSERT INTO `{Table}` " +
                "(citizen_id,ucp,ip,username,skin,gender,birth_date,height,hair,eye,mask_id," +
                "position,vitals,playtime,backpack,phone,jail_info,ban_info,`condition`,`settings`,jobs) " +
                "VALUES (@Cid,@Ucp,@Ip,@Username,@Skin,@Gender,@BirthDate,@Height,@Hair,@Eye,@MaskId," +
                "@Pos,@Vitals,@Playtime,@Backpack,@Phone,@JailInfo,@BanInfo,@Condition,@Settings,@Jobs)",
                new
                {
                    Cid = cid,
                    Ucp = player.Name,
                    Ip = player.IP,
                    Username = c.Name,
                    Skin = c.Skin,
                    Gender = c.Gender,
                    BirthDate = c.BirthDate,
                    Height = c.Height,
                    Hair = c.Hair,
                    Eye = c.Eye,
                    MaskId = maskId,
                    Pos = Ser(pos),
                    Vitals = Ser(new CharVitals()),
                    Playtime = Ser(new CharPlaytime()),
                    Backpack = Ser(new CharBackpack()),
                    Phone = Ser(new CharPhone()),
                    JailInfo = Ser(new CharJailInfo()),
                    BanInfo = Ser(new CharBanInfo()),
                    Condition = Ser(new CharCondition()),
                    Settings = Ser(new CharSettings()),
                    Jobs = Ser(new List<CharJob>())
                });

            if (player.IsDisposed) return;

            ApplyToPlayer(player, new RawCharRow
            {
                citizen_id = cid,
                ucp = player.Name,
                ip = player.IP,
                username = c.Name,
                skin = c.Skin,
                gender = c.Gender,
                birth_date = c.BirthDate,
                height = c.Height,
                hair = c.Hair,
                eye = c.Eye,
                mask_id = maskId,
                position = Ser(pos),
                level = 1,
                last_login = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            _creations.Remove(player.Id);
            SpawnCharacter(player);
        }

        private static async void LoadExistingCharAsync(Player player, string citizenId)
        {
            var raw = await DatabaseManager.QueryFirstAsync<RawCharRow>(
                $"SELECT * FROM `{Table}` WHERE citizen_id = @Id LIMIT 1", new { Id = citizenId });

            if (player.IsDisposed) return;

            if (raw is null)
            {
                _lists.TryGetValue(player.Id, out var l);
                ShowCharListDialog(player, l ?? new());
                return;
            }

            ApplyToPlayer(player, raw);

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET ip=@Ip, last_login=CURRENT_TIMESTAMP() WHERE citizen_id=@Id",
                new { Ip = player.IP, Id = raw.citizen_id });

            if (player.IsDisposed) return;
            SpawnCharacter(player);
        }
        private static void SpawnCharacter(Player player)
        {
            CinematicCameraService.Stop(player);
            player.ToggleSpectating(false);
            player.Interior = player.CharSpawnPos.Interior;
            player.VirtualWorld = player.CharSpawnPos.World;
            player.SetSpawnInfo(0, player.CharSkin,
                new Vector3(player.CharSpawnPos.X, player.CharSpawnPos.Y, player.CharSpawnPos.Z),
                player.CharSpawnPos.A);
            player.Spawn();
        }
        public static void RespawnCharacter(Player player)
        {
            if (!player.IsCharLoaded) return;
            var pos = player.CharSpawnPos;
            player.Interior = pos.Interior;
            player.VirtualWorld = pos.World;
            player.SetSpawnInfo(0, player.CharSkin,
                new Vector3(pos.X, pos.Y, pos.Z), pos.A);
            player.ToggleSpectating(false);
        }

        private static void ApplyToPlayer(Player player, RawCharRow r)
        {
            player.CitizenId = r.citizen_id;
            player.Ucp = r.ucp;
            player.RegDate = r.reg_date;
            player.LastLogin = r.last_login;
            player.VerifiedChar = r.verified_char;
            player.Username = r.username;
            player.CharSkin = r.skin;
            player.Gender = r.gender;
            player.BirthDate = r.birth_date;
            player.Height = r.height;
            player.Hair = r.hair;
            player.Eye = r.eye;
            player.Level = r.level;
            player.LevelPoints = r.level_points;
            player.LevelPointsExp = r.level_points_exp;
            player.CharMoney = r.money;
            player.Admin = r.admin;
            player.MaskId = r.mask_id;
            player.Warn = r.warn;
            player.Paycheck = r.paycheck;
            player.Vitals = Des<CharVitals>(r.vitals) ?? new();
            player.Playtime = Des<CharPlaytime>(r.playtime) ?? new();
            player.Backpack = Des<CharBackpack>(r.backpack) ?? new();
            player.Phone = Des<CharPhone>(r.phone) ?? new();
            player.JailInfo = Des<CharJailInfo>(r.jail_info) ?? new();
            player.BanInfo = Des<CharBanInfo>(r.ban_info) ?? new();
            player.CharSpawnPos = Des<CharPosition>(r.position) ?? new();
            player.Condition = Des<CharCondition>(r.condition) ?? new();
            player.Settings = Des<CharSettings>(r.settings) ?? new();
            player.Jobs = Des<List<CharJob>>(r.jobs) ?? new();
            player.IsCharLoaded = true;
        }

        private static string FmtGender(Player p, int g) => g == 0 ? L(p, "CHAR", "GENDER_MALE") : L(p, "CHAR", "GENDER_FEMALE");
        private static string Ser<T>(T obj) => JsonSerializer.Serialize(obj, _jOpts);

        private static string GenCitizenId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rng = new Random();
            var r = new char[8];
            for (var i = 0; i < 8; i++) r[i] = chars[rng.Next(chars.Length)];
            return new string(r);
        }

        private static T? Des<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json, _jOpts); }
            catch { return null; }
        }
    }
}