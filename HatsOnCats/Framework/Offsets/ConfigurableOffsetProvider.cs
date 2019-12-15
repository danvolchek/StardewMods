using System;
using System.Collections.Generic;
using System.Linq;
using HatsOnCats.Framework.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace HatsOnCats.Framework.Offsets
{
    internal class ConfigurableOffsetProvider : IConfigurable, IOffsetProvider
    {
        public string Name { get; }

        private readonly string internalName;
        private readonly IDictionary<Frame, Offset> defaultConfiguration;
        private readonly IDictionary<Frame, Offset> configuration = new Dictionary<Frame, Offset>();

        public IDictionary<Frame, Offset> Configuration
        {
            get => this.configuration;
            set => this.SetConfiguration(value);
        }

        public ConfigurableOffsetProvider(string name, IDictionary<Frame, Offset> defaultConfiguration)
        {
            this.Name = name;
            this.internalName = name.ToLower();
            this.defaultConfiguration = defaultConfiguration;

            foreach (KeyValuePair<Frame, Offset> kvp in defaultConfiguration)
            {
                this.Configuration[kvp.Key] = kvp.Value;
            }
        }

        public void SetConfiguration(IDictionary<Frame, Offset> newConfiguration)
        {
            this.Configuration.Clear();

            foreach (KeyValuePair<Frame, Offset> offset in this.defaultConfiguration)
            {
                this.Configuration[offset.Key] = offset.Value;
            }

            foreach (KeyValuePair<Frame, Offset> offset in newConfiguration)
            {
                this.Configuration[offset.Key] = offset.Value;
            }

            this.ValidateConfig();
        }

        public bool GetOffset(Rectangle sourceRectangle, SpriteEffects effects, out Vector2 offset)
        {
            return this.GetOffset(new Frame(new Point(sourceRectangle.X, sourceRectangle.Y), effects), out offset);
        }

        public bool CanHandle(string spriteName)
        {
            return spriteName.Contains(this.internalName);
        }

        private void ValidateConfig()
        {
            foreach(KeyValuePair<Frame, Offset> kvp in this.Configuration.Where(kvp => kvp.Value.IsReference))
            {
                string messagePrefix = $"Config key {this.Name}:{kvp.Key} has an invalid reference";

                if (!this.Configuration.TryGetValue(kvp.Value.Reference.Frame, out Offset config))
                {
                    throw new Exception($"{messagePrefix}: Referenced key {kvp.Value.Reference.Frame} doesn't exist.");
                }

                if (config.IsReference)
                {
                    throw new Exception($"{messagePrefix}: References can only reference values, not other references.");
                }
            }
        }

        private bool GetOffset(Frame frame, out Vector2 offset)
        {
            if (this.Configuration.TryGetValue(frame, out Offset config))
            {
                offset = this.GetOffset(config);
                return true;
            }
            offset = Vector2.Zero;
            return false;
        }

        private Vector2 GetOffset(Offset config)
        {
            if (!config.IsReference)
            {
                return config.Value;
            }

            return this.Configuration[config.Reference.Frame].Value;
        }
    }
}
