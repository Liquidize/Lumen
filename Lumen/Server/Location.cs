﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Effects;
using Lumen.Api.Graphics;
using Lumen.Network;
using Lumen.Registries;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Lumen.Server
{
    [Serializable]
    public class Location : IDisposable
    {

        [JsonIgnore]
        protected DateTime StartTime { get; set; }

        [JsonIgnore]
        protected Thread _thread { get; set; }

        [JsonProperty("name")]
        public string Name { get; protected set; } = "[None]";

        [JsonProperty("controllers")]
        public virtual ControllerChannel[] Controllers { get;  set; }

        [JsonProperty("scheduledEffects")]
        public virtual ScheduledEffect[] ScheduledEffects { get; set; }


        [JsonProperty("framesPerSecond")]
        public uint FramesPerSecond { get; protected set; } = 21;

        [JsonProperty("canvasType")] public string CanvasType { get; protected set; } = "Canvas1D";

        [JsonIgnore]
        public Canvas Canvas { get; protected set; }

        [JsonProperty("width")]
        public uint Width { get; set; } = 144;

        [JsonProperty("height")]
        public uint Height { get; set; } = 1;

        [JsonProperty("isApiEnabled")]
        public bool IsApiEnabled { get; protected set; } = true;

        [JsonIgnore]
        public LedEffect? ActiveEffect { get; protected set; } = null;

        private LedEffect? _forcedEffect { get;  set; } = null;

        private readonly ConcurrentQueue<LedEffect?> effectQueue = new ConcurrentQueue<LedEffect?>();


        [JsonIgnore]
        public string JsonPath { get; set; } = string.Empty;


        public void Initialize()
        {
            StartTime = DateTime.UtcNow;

            Canvas = Lumen.CanvasRegistry.CreateCanvasInstance(CanvasType);
            if (Canvas == null)
            {
                // Maybe we should do something more if a canvas cant be created? The location is basically dead otherwise.
                Log.Error($"Canvas of type {CanvasType} could not be created for Location {Name}");
                return;
            }

            Canvas.Initialize(Width, Height);

            foreach (var controller in Controllers)
            {
                controller.SetLocation(this);
                controller.StartThread();
            }

            _thread = new Thread(DrawAndSendLoop);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.BelowNormal;
            _thread.Start();

        }

        public bool IsEffectQueued(string effectId)
        {
            return effectQueue.FirstOrDefault(x => x.Id == effectId) != null;
        }


        [JsonIgnore]
        public string CurrentEffectName
        {
            get { return ActiveEffect != null ? ActiveEffect.Name : "[None]"; }
        }

        private double SpareTime = 1000;
        private bool disposedValue;

        private void DrawAndSendLoop()
        {
            var lastSpareTimeReset = DateTime.UtcNow;
            var lastFrameTime = DateTime.UtcNow - TimeSpan.FromSeconds(1.0 / FramesPerSecond);
          while (!disposedValue)
            {
                var nextTime = lastFrameTime + TimeSpan.FromSeconds(1.0 / FramesPerSecond);
                var deltaTime = nextTime - lastFrameTime;
                lastFrameTime = nextTime;

                if (ActiveEffect == null || ActiveEffect.IsLifetimeOver())
                {
                    ActiveEffect = ProcessQueue(nextTime);
                    if (ActiveEffect != null)
                    {
                        ActiveEffect.SetStartTime(DateTime.UtcNow);
                        if (_forcedEffect != null) _forcedEffect = null;
                    }
                    else
                    {
                        continue;
                    }
                }

                // This should be an unreachable block, but safety first?
                if (Canvas == null)
                {
                    Log.Warning($"Canvas is null, unable to update and draw effect for Location {Name}. ");
                    continue;
                }



                // Update and  Render and send to controllers
                ActiveEffect.DrawFrame(deltaTime.TotalMilliseconds);

                foreach (var controller in Controllers)
                {
                    if (controller.IsReadyForData)
                    {
                        controller.CompressAndEnqueueData(Canvas.GetPixels(), nextTime);
                    }
                    else
                    {
                        controller.Response.Reset();
                    }
                }


                var delay = nextTime - DateTime.UtcNow;
                if (delay.TotalMilliseconds > 0)
                {
                    Thread.Sleep((int)delay.TotalMilliseconds);
                }
                else
                {
                    Log.Information(GetType().Name + " dropped Frame by " + delay.TotalMilliseconds);
                    Thread.Sleep(1);
                }


                var spare = delay.TotalMilliseconds <= 0 ? 0 : delay.TotalMilliseconds;
                SpareTime = Math.Min(SpareTime, (uint)spare);


                if ((DateTime.UtcNow - lastSpareTimeReset).TotalSeconds > 1)
                {
                    SpareTime = 1000;
                    lastSpareTimeReset = DateTime.UtcNow;
                }

            }
        }


        public void SetForcedEffect(LedEffect? effect)
        {
            if (ActiveEffect != null) 
                ActiveEffect.RequestEnd();
            _forcedEffect = effect;
        }

        public void EnqueueEffect(LedEffect? effect)
        {
            if (effect == null) return;
            effectQueue.Enqueue(effect);
        }

        private LedEffect? ProcessQueue(DateTime timeStamp)
        {
            if (_forcedEffect != null) return _forcedEffect;

            if (effectQueue.TryDequeue(out var queuedEffect))
            {
                return queuedEffect;
            }

            var scheduledEffected = ScheduledEffects.Where(scheduled =>
            {
                return scheduled.IsEffectScheduledToRunNow;
            });
            if (scheduledEffected.Any())
            {
                var nextEffect = scheduledEffected.First();
                var scheduledEffect = Lumen.EffectRegistry.CreateEffectInstance(nextEffect.EffectName, Canvas, nextEffect.Settings);
                if (scheduledEffect != null)
                {
                    scheduledEffect.SetId(nextEffect.Id);
                    return scheduledEffect;
                }
            }

            return null;
        }


        public ConcurrentQueue<LedEffect?> GetEffectQueue()
        {
            return effectQueue;
        }


        public void Serialize()
        {
            var serialierSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var content = JsonConvert.SerializeObject(this, typeof(Location), serialierSettings);
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "locations");
            if (Directory.Exists(path) != true) Directory.CreateDirectory(path);

            File.WriteAllText(path + $"/{Name}.json", content);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // We need to do this or the controllers remain when reloading the location
                    Controllers = null;
                    ScheduledEffects = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public LedEffect? GetEffect(string id)
        {
            return effectQueue.Concat(new []{ ActiveEffect }).FirstOrDefault(x => x != null && x.Id == id);
        }
    }
}
