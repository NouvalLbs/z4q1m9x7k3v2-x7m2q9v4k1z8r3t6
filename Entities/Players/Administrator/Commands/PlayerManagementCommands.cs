using ProjectSMP.Core;
using ProjectSMP.Extensions;
using ProjectSMP.Features.NameTag;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class PlayerManagementCommands : AdminCommandBase
    {
        [Command("aduty")]
        public static void ADuty(Player player)
        {
            if (player.Admin < 1)
            {
                player.SendClientMessage(Color.White, "{b9b9b9}Command '/aduty' tidak ada, gunakan '/help'.");
                return;
            }

            if (!player.AdminOnDuty)
            {
                player.Color = new Color(255, 0, 0, 0);
                player.AdminOnDuty = true;
                player.Name = player.Ucp;
                NameTagService.Refresh(player);
                Utilities.SendStaffMessage(-1, "{{FF6347}}{0}{{FFFFFF}} telah on duty admin dengan nama {1}", player.Username, player.Ucp);
            }
            else
            {
                player.Color = Color.White;
                player.AdminOnDuty = false;
                player.Name = player.Username;
                NameTagService.Refresh(player);
                Utilities.SendStaffMessage(-1, "{{FF6347}}{0}{{FFFFFF}} telah off duty admin.", player.Ucp);
            }
        }

        [Command("goto")]
        public static void GoTo(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            var pos = target.Position;
            player.SetInteriorSafe(target.Interior);
            player.SetVirtualWorldSafe(target.VirtualWorld);
            player.SetPositionSafe(pos.X + 1, pos.Y + 1, pos.Z);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah teleport ke {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah teleport ke lokasi kamu");
        }

        [Command("gethere")]
        public static void GetHere(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            var pos = player.Position;
            target.SetInteriorSafe(player.Interior);
            target.SetVirtualWorldSafe(player.VirtualWorld);
            target.SetPositionSafe(pos.X + 1, pos.Y + 1, pos.Z);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah menarik {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}} ke lokasi kamu!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah menarik kamu ke lokasi mereka");
        }

        [Command("freeze")]
        public static void Freeze(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            target.ToggleControllableSafe(false);
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah Freeze {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah Freeze pergerakan kamu");
        }

        [Command("unfreeze")]
        public static void Unfreeze(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            target.ToggleControllableSafe(true);
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah Unfreeze {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah Unfreeze pergerakan kamu");
        }

        [Command("slap")]
        public static void Slap(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target) || !CheckAdminRank(player, target)) return;

            if (target.InAnyVehicle) target.RemoveFromVehicle();

            var pos = target.Position;
            target.SetPositionSafe(pos.X, pos.Y, pos.Z + 10.0f);
            target.PlaySound(1130, Vector3.Zero);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah menampar {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah menampar kamu");
        }

        [Command("acuff")]
        public static void ACuff(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target) || !CheckAdminRank(player, target)) return;

            if (target.Cuffed)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut sudah dalam keadaan terborgol!");
                return;
            }

            target.Cuffed = true;
            target.SetSpecialActionSafe(SpecialAction.Cuffed);
            target.ToggleControllableSafe(false);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah memborgol {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah memborgol kamu");
        }

        [Command("auncuff")]
        public static void AUncuff(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            if (!target.Cuffed)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut tidak dalam keadaan terborgol!");
                return;
            }

            target.Cuffed = false;
            target.SetSpecialActionSafe(SpecialAction.None);
            target.ToggleControllableSafe(true);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah melepas borgol {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah melepas borgol kamu");
        }

        [Command("peject")]
        public static void PEject(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 2)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target) || !CheckAdminRank(player, target)) return;

            if (!target.InAnyVehicle)
            {
                player.SendClientMessage(Color.White, "{FF6347}<AdmCmd>{FFFFFF} Player tersebut tidak berada di dalam kendaraan!");
                return;
            }

            target.RemoveFromVehicle();
            var pos = target.Position;
            target.SetPositionSafe(pos.X, pos.Y, pos.Z + 1.0f);

            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah mengeluarkan {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}} dari kendaraan!");
            target.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengeluarkan kamu dari kendaraan");
        }

        [Command("astats")]
        public static void AStats(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 2)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            var gender = target.Gender == 0 ? "Male" : "Female";
            var phoneStatus = target.Phone.Off == 0 ? "{91ff00}Online{FFFFFF}" : "{FF0000}Offline{FFFFFF}";
            var charStatus = target.VerifiedChar == 1 ? "{91ff00}Verified{FFFFFF}" : "{FF0000}Unverified{FFFFFF}";
            var admin = Utilities.GetAdminString(target);
            var warn = Utilities.GetWarningString(target);

            var stats = $@"{{FFFF00}}IC Information:
            {{FFFFFF}}Gender: [{{b8d2ec}}{gender}{{FFFFFF}}] | Birthdate: [{{b8d2ec}}{target.BirthDate}{{FFFFFF}}] | Money: [{{00f000}}{Utilities.GroupDigits(target.CharMoney)}{{FFFFFF}}]
            {{FFFFFF}}Phone Status: [{phoneStatus}] | Phone Number: [{{ebeb00}}{target.Phone}{{FFFFFF}}] | Mask ID: [{{b8d2ec}}{target.MaskId}{{FFFFFF}}]

            {{FFFF00}}OOC Information:
            {{FFFFFF}}CitizenId: [{{77efc7}}{target.CitizenId}{{FFFFFF}}] | Level: [{{77efc7}}{target.Level}{{FFFFFF}}] | Paychecks: [{{b8d2ec}}{target.Paycheck}{{FFFFFF}}]
            {{FFFFFF}}Character Story: [{charStatus}] | Staff: [{admin}] | Warns: [{warn}]
            {{FFFFFF}}World: [{{ebeb00}}{target.VirtualWorld}{{FFFFFF}}] | Interior: [{{ebeb00}}{target.Interior}{{FFFFFF}}] | Health: [{{ab0000}}{target.Vitals.Health:F1}{{FFFFFF}}] | Armour: [{{9f9f9f}}{target.Vitals.Armour:F1}{{FFFFFF}}]";

            var title = $"{{FF6347}}Admin Stats: {{6fe0ba}}{target.Username} {{c8c8c8}}(ID:{target.Id} | UCP: {target.Ucp})";
            player.ShowMessage(title, stats).Show();
            player.SendClientMessage(Color.White, $"{{FF6347}}<AdmCmd>{{FFFFFF}} Kamu telah melihat statistik dari {{00FFFF}}{target.Username} (ID:{target.Id}){{FFFFFF}}!");
        }
    }
}