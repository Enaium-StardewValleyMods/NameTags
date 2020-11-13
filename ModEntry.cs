﻿using System;
using System.Linq;
using EnaiumToolKit.Framework.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using NameTags.Framework;
using NameTags.Framework.Gui;
using Netcode;
using StardewValley.Characters;
using StardewValley.Monsters;
using Rectangle = xTile.Dimensions.Rectangle;

namespace NameTags
{
    public class ModEntry : Mod
    {
        public static Config Config;
        private static ModEntry _instance;

        public ModEntry()
        {
            _instance = this;
        }

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<Config>();
            helper.Events.Input.ButtonPressed += OnButtonPress;
            helper.Events.Display.Rendered += OnRender;
        }

        private void OnRender(object sender, RenderedEventArgs e)
        {
            if (Game1.activeClickableMenu != null) return;
            try
            {
                foreach (var variable in GetCharacters())
                {
                    string tag = $"{Helper.Translation.Get("nameTags.name")}:{variable.displayName}";
                    if (variable is Monster monster)
                    {
                        if (monster.MaxHealth < monster.Health)
                        {
                            monster.MaxHealth = monster.Health;
                        }

                        tag +=
                            $" {Helper.Translation.Get("nameTags.hp")}:{monster.Health}/{monster.MaxHealth} {Helper.Translation.Get("nameTags.damage")}:{monster.damageToFarmer}";
                    }
                    else if (variable is Pet pet)
                    {
                        tag += $" {Helper.Translation.Get("nameTags.friendship")}:{pet.friendshipTowardFarmer / 200}";
                    }
                    else if (variable is Child child)
                    {
                        tag += $" {Helper.Translation.Get("nameTags.daysOld")}:{child.daysOld}";
                    }
                    else
                    {
                        if (Game1.player.friendshipData.TryGetValue(variable.name, out Friendship friendship))
                        {
                            tag +=
                                $" {Helper.Translation.Get("nameTags.friendship")}:{(friendship?.Points ?? 0) / NPC.friendshipPointsPerHeartLevel}";
                        }
                    }

                    Vector2 screenLoc = variable.Position - new Vector2(Game1.viewport.X, Game1.viewport.Y) -
                                        new Vector2(variable.Sprite.SpriteWidth, variable.Sprite.SpriteHeight);
                    FontUtils.Draw(e.SpriteBatch, tag, (int) screenLoc.X, (int) screenLoc.Y);
                }
            }
            catch (Exception exception)
            {
                ;
            }
        }

        private NetCollection<NPC> GetCharacters()
        {
            NetCollection<NPC> n = new NetCollection<NPC>();
            foreach (var variable in Game1.currentLocation.characters)
            {
                if (Config.RenderMonster && variable is Monster)
                {
                    n.Add(variable);
                }
                else if (variable is Horse horse)
                {
                }
                else if (Config.RenderPet && variable is Pet)
                {
                    n.Add(variable);
                }
                else if (variable is TrashBear)
                {
                }
                else if (variable is Junimo)
                {
                }
                else if (variable is JunimoHarvester)
                {
                }
                else if (Config.RenderChild && variable is Child)
                {
                    n.Add(variable);
                }
                else if (Config.RenderVillager && !(variable is Monster))
                {
                    n.Add(variable);
                }
            }

            return n;
        }

        private void OnButtonPress(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (!Context.IsPlayerFree)
                return;
            if (e.Button != Config.OpenSetting)
                return;
            Game1.activeClickableMenu = new NameTagsScreen();
        }

        public static ModEntry GetInstance()
        {
            return _instance;
        }
    }
}