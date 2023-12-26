using Lumen.Api.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Graphics;

namespace Lumen.Registries
{
    public interface ICanvasRegistry
    {
        public Canvas CreateCanvasInstance(string canvasName);
    }

    public class CanvasRegistry : ICanvasRegistry
    {
        private readonly Dictionary<string, Func<Canvas>> CanvaFactories = new Dictionary<string, Func<Canvas>>();

        public  void RegisterCanvas(string canvasName, Func<Canvas> factory)
        {
            CanvaFactories.TryAdd(canvasName, factory);
        }

        public void LoadCanvases()
        {
            var canvasTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => typeof(Canvas).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var canvasType in canvasTypes)
            {
                var instance = Activator.CreateInstance(canvasType) as Canvas;
                if (instance.Name != null)
                    RegisterCanvas(instance.Name, () => Activator.CreateInstance(canvasType) as Canvas);
            }
        }

        public Canvas CreateCanvasInstance(string canvasName)
        {
            if (CanvaFactories.TryGetValue(canvasName, out var canvas))
            {
                return canvas.Invoke();
            }
            else
            {
                return null;
            }
        }
    }
}
