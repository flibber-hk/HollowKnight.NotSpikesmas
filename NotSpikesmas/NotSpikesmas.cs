using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Items;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using System;
using UnityEngine;

namespace NotSpikesmas
{
    [Serializable]
    public class EmbeddedSprite : ISprite
    {
        private static readonly SpriteManager _embeddedSpriteManager = new(typeof(EmbeddedSprite).Assembly, "NotSpikesmas.Resources.");

        public string key;
        public EmbeddedSprite(string key)
        {
            this.key = key;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Sprite Value => _embeddedSpriteManager.GetSprite(key);
        public ISprite Clone() => (ISprite)MemberwiseClone();
    }


    public class NotSpikesmas : Mod, IGlobalSettings<Settings>
    {
        internal static NotSpikesmas Instance;

        public const string NotSpikesmasItemName = "Not_Spikesmas";

        public NotSpikesmas() : base(null)
        {
            Instance = this;
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");

            DefineItem();
            if (ModHooks.GetMod("Randomizer 4") is Mod)
            {
                RandoInterop.Hook();
            }
        }

        private void DefineItem()
        {
            AbstractItem notSpikesmasItem = new CustomSkillItem()
            {
                boolName = nameof(NotSpikesmasModule.hasNotSpikesmas),
                moduleName = "NotSpikesmas.NotSpikesmasModule, NotSpikesmas",
                name = NotSpikesmasItemName,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Not Spikesmas"),
                    shopDesc = new BoxedString("Nobody knows where this came from."),
                    sprite = new EmbeddedSprite("NotSpikesmasIcon"),
                }
            };

            InteropTag tag = notSpikesmasItem.AddTag<InteropTag>();
            tag.Message = "RandoSupplementalMetadata";
            tag.Properties["ModSource"] = Instance.GetName();
            tag.Properties["PoolGroup"] = "Skills";

            Finder.DefineCustomItem(notSpikesmasItem);
        }

        public static Settings GS { get; set; } = new();

        public void OnLoadGlobal(Settings s) => GS = s;

        public Settings OnSaveGlobal() => GS;
    }
}