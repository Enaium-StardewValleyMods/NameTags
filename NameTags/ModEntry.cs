﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using NameTags.Framework;
using NameTags.Framework.Gui;
using Netcode;
using StardewValley.Characters;
using StardewValley.Monsters;

namespace NameTags;

public class ModEntry : Mod
{
    public static Config Config;
    private static ModEntry _instance;

    public ModEntry()
    {
        _instance = this;
    }

    public static void ConfigReload()
    {
        GetInstance().Helper.WriteConfig(Config);
        Config = GetInstance().Helper.ReadConfig<Config>();
    }

    public override void Entry(IModHelper helper)
    {
        Config = helper.ReadConfig<Config>();
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        helper.Events.Display.Rendered += OnRender;
    }

    private void OnRender(object? sender, RenderedEventArgs e)
    {
        if (Game1.activeClickableMenu != null) return;
        try
        {
            foreach (var variable in GetCharacters())
            {
                var tag = $"{variable.displayName}";
                switch (variable)
                {
                    case Monster monster:
                    {
                        if (monster.MaxHealth < monster.Health)
                        {
                            monster.MaxHealth = monster.Health;
                        }

                        tag +=
                            $" {Helper.Translation.Get("nameTags.hp")}:{monster.Health}/{monster.MaxHealth} {Helper.Translation.Get("nameTags.damage")}:{monster.DamageToFarmer}";
                        break;
                    }
                    case Pet pet:
                        tag +=
                            $" {Helper.Translation.Get("nameTags.friendship")}:{pet.friendshipTowardFarmer.Get() / 200}";
                        break;
                    case Horse:
                        break;
                    case Child child:
                        tag += $" {Helper.Translation.Get("nameTags.daysOld")}:{child.daysOld}";
                        break;
                    case Junimo junimo:
                        tag += $" {Helper.Translation.Get("nameTags.friendly")}:{junimo.friendly}";
                        break;
                    default:
                    {
                        if (Game1.player.friendshipData.TryGetValue(variable.Name, out var friendship))
                        {
                            tag +=
                                $" {Helper.Translation.Get("nameTags.friendship")}:{(friendship?.Points ?? 0) / NPC.friendshipPointsPerHeartLevel}";
                        }

                        break;
                    }
                }

                var position = variable.Position - new Vector2(Game1.viewport.X, Game1.viewport.Y) -
                               new Vector2(variable.Sprite.SpriteWidth, variable.Sprite.SpriteHeight);
                var v = Game1.dialogueFont.MeasureString(tag);
                var x = (int)position.X;
                var y = (int)position.Y - variable.Sprite.SpriteHeight * 2;
                e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(x, y, (int)v.X, (int)v.Y), Config.BackgroundColor);
                e.SpriteBatch.DrawString(Game1.dialogueFont, tag, new Vector2(x, y), Config.Color);
                if (Config.TargetLine)
                {
                    var playerPosition = Game1.GlobalToLocal(Game1.viewport, Game1.player.Position);
                    var targetPosition = variable.getLocalPosition(Game1.viewport);
                    Utility.drawLineWithScreenCoordinates(
                        (int)playerPosition.X + Game1.tileSize / 2,
                        (int)playerPosition.Y + Game1.tileSize / 2,
                        (int)targetPosition.X + Game1.tileSize / 2,
                        (int)targetPosition.Y + Game1.tileSize / 2
                        , e.SpriteBatch, Config.Color, 0.1f);
                }
            }
        }
        catch (Exception exception)
        {
            ;
        }
    }

    private IEnumerable<NPC> GetCharacters()
    {
        var n = new NetCollection<NPC>();
        foreach (var variable in Game1.currentLocation.characters)
        {
            if (Config.RenderMonster && variable is Monster)
            {
                n.Add(variable);
            }
            else if (Config.RenderPet && variable is Pet)
            {
                n.Add(variable);
            }
            else if (Config.RenderHorse && variable is Horse)
            {
                n.Add(variable);
            }
            else if (variable is TrashBear)
            {
            }
            else if (Config.RenderJunimo && variable is Junimo)
            {
                n.Add(variable);
            }
            else if (variable is JunimoHarvester)
            {
            }
            else if (Config.RenderChild && variable is Child)
            {
                n.Add(variable);
            }
            else if (Config.RenderVillager && !(variable is Monster) && !(variable is Pet) &&
                     !(variable is Horse) && !(variable is Junimo) && !(variable is Child))
            {
                n.Add(variable);
            }
        }

        return n;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        if (!Context.IsPlayerFree)
            return;
        if (!Config.OpenSetting.JustPressed())
            return;
        Game1.activeClickableMenu = new NameTagsScreen();
    }

    public static ModEntry GetInstance()
    {
        return _instance;
    }
}