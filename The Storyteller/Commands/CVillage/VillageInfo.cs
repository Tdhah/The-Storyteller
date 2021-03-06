﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using The_Storyteller.Entities;
using The_Storyteller.Entities.Tools;
using The_Storyteller.Models.MGameObject;
using The_Storyteller.Models.MVillage;

namespace The_Storyteller.Commands.CVillage
{
    /// <summary>
    /// Group de commandes, affiche les infos sur le village
    /// Par la suite, détails des villageois, buildings, etc ...
    /// </summary>
    [Group("villageinfo", CanInvokeWithoutSubcommand = true)]
    class VillageInfo
    {
        private readonly Dependencies dep;

        public VillageInfo(Dependencies d)
        {
            dep = d;
        }

        public async Task ExecuteGroupAsync(CommandContext ctx, params string[] name)
        {
            //Vérification de base character + guild
            if (!dep.Entities.Characters.IsPresent(ctx.User.Id)
                || (!ctx.Channel.IsPrivate) && !dep.Entities.Guilds.IsPresent(ctx.Guild.Id))
            {
                return;
            }

            var character = dep.Entities.Characters.GetCharacterByDiscordId(ctx.User.Id);
            var villageName = character.VillageName;
            var detailled = true;
               
            
            if (name.Length > 0)
            {
                villageName = string.Join(" ", name);
                detailled = false;

            }

            Village village = dep.Entities.Villages.GetVillageByName(villageName);

            if (village == null)
            {
                var embed = dep.Embed.CreateBasicEmbed(ctx.User, dep.Dialog.GetString("errorNotPartOfVillage"));
                await ctx.RespondAsync(embed: embed);
                return;
            }

            var  embedVillage = GetVillageInfo(village, detailled);

            DiscordDmChannel channel;
            if (!ctx.Channel.IsPrivate)
            {
                channel = await ctx.Member.CreateDmChannelAsync();
                await ctx.RespondAsync($"{ctx.Member.Mention} private message sent !");
            }
            else
                channel = (DiscordDmChannel)ctx.Channel;

            if (detailled)
            {
                await channel.SendMessageAsync(embed: embedVillage);
                await ctx.RespondAsync($"{ctx.Member.Mention} private message sent !");
            }
            else
            {
                await ctx.Channel.SendMessageAsync(embed: embedVillage);
            }
            
        }

        [Command("building")]
        public async Task VillageInfoBuildingCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("building info");
        }

        public DiscordEmbedBuilder GetVillageInfo(Village v, bool detailled)
        {
            var king = dep.Entities.Characters.GetCharacterByDiscordId(v.KingId);
            var inventory = dep.Entities.Inventories.GetInventoryById(v.Id);

            List<string> inventoryList = new List<string>();
            inventoryList.Add("Treasury: " + inventory.GetMoney());
            inventoryList.Add("----------------");
            foreach (GameObject go in inventory.GetItems())
            {
                inventoryList.Add($"{go.Name} -  Quantity: {go.Quantity}");
            }

            List<string> charactersList = new List<string>();
            foreach (ulong cId in v.GetInhabitants())
            {
                var c = dep.Entities.Characters.GetCharacterByDiscordId(cId);
                charactersList.Add($"{c.Name} - Profession: {c.Profession}");
            }

            List<string> buildingList = new List<string>();
            foreach (Building building in v.GetBuildings())
            {
                var c = dep.Entities.Characters.GetCharacterByDiscordId(building.ProprietaryId);
                buildingList.Add($"{building.Name} - Level: {building.Level} - Proprietary: {c.Name}");
            }

            List<CustomEmbedField> attributes = new List<CustomEmbedField>
            {
                //1 Infos général du personnage
                new CustomEmbedField()
                {
                    Name = "General informations",
                    Attributes = new List<string>
                    {
                        "Name: " + v.Name,
                        "Region: " + v.RegionName,
                        "King: " + king.Name,
                        "Access: " + v.VillagePermission
                    }
                },
                new CustomEmbedField()
                {
                    Name = "Villagers",
                    Attributes = charactersList
                }
            };

            if (detailled)
            {
                attributes.Add(new CustomEmbedField()
                {
                    Name = "Buildings",
                    Attributes = buildingList
                });

                attributes.Add(new CustomEmbedField()
                {
                    Name = "Inventory",
                    Attributes = inventoryList
                });
            }

            string title = "Village Informations";

            DiscordEmbedBuilder embed = dep.Embed.CreateDetailledEmbed(title, attributes, inline: true);

            return embed;
        }
    }
}
