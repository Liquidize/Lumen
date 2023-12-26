using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Lumen.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Lumen.Registries
{
    public interface ILocationRegistry
    {
        public List<Location> GetLocations();
        public Location GetLocation(string name);

        public void LoadLocations();
    }

    public class LocationRegistry : ILocationRegistry
    {
        private enum FileActionType
        {
            Changed,
            Created,
            Renamed,
            Deleted
        }

        private class FileActionEntry
        {
            public string Path;
            public FileActionType Action;

            public FileActionEntry(string path, FileActionType type)
            {
                Path = path;
                Action = type; 
            }

        }

        public List<Location> JsonLocations { get; private set; } = new List<Location>();
        public List<Location> InternalLocations { get; private set; } = new List<Location>()
        {
         
        };
        public  List<Location> AllLocations => JsonLocations.Concat(InternalLocations).ToList();

        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();

        private readonly ConcurrentQueue<FileActionEntry> _fileActionsQueue =
            new ConcurrentQueue<FileActionEntry>();

        private Thread _fileSystemThread;


        public LocationRegistry()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "locations");

            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);

            _fileSystemWatcher = new FileSystemWatcher(path);
            _fileSystemWatcher.Filter = "*.json";
            _fileSystemWatcher.IncludeSubdirectories = true;

            _fileSystemWatcher.Created += OnJsonFileCreated;
            _fileSystemWatcher.Deleted += OnJsonFileDeleted;
            _fileSystemWatcher.Changed += OnJsonFileChanged;
            _fileSystemWatcher.Renamed += OnJsonFileRenamed;

            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;

            // TODO: To reduce the amount of times the events fire, create a queue like system that is executed upon on a background thread.
            _fileActionsQueue = new ConcurrentQueue<FileActionEntry>();

            _fileSystemThread = new Thread(FileSystemActionLoop);
            _fileSystemThread.IsBackground = true;
            _fileSystemThread.Priority = ThreadPriority.BelowNormal;
            _fileSystemThread.Start();

        }

        private void FileSystemActionLoop()
        {
            while (true)
            {
                while (_fileActionsQueue.Count > 0)
                {
                    if (_fileActionsQueue.TryDequeue(out FileActionEntry entry))
                    {
                        if (entry.Action == FileActionType.Changed || entry.Action == FileActionType.Created || entry.Action == FileActionType.Renamed)
                        {
                            var instance = LoadJsonLocation(entry.Path);
                            if (instance == null)
                            {
                                Log.Warning($"Could not refresh Location instance from JSON file with path {entry.Path}");
                                return;
                            }

                            if (TryRemoveLocation(instance))
                            {
                                JsonLocations.Add(instance);
                                instance.JsonPath = entry.Path;
                                instance.Initialize();
                                Log.Information($"Refreshed location {instance.Name} from path {entry.Path} new location count {JsonLocations.Count}");
                            }
                            else
                            {
                                Log.Warning($"Could not refresh location from JSON file, unable to remove the original location for path {entry.Path}");
                            }
                        } else if (entry.Action == FileActionType.Deleted)
                        {
                            var instance = JsonLocations.FirstOrDefault(x => x.JsonPath.ToLower() == entry.Path.ToLower());
                            if (instance != null)
                            {
                                JsonLocations.Remove(instance);
                                instance.Dispose();
                                Log.Information($"Removed location {instance.Name} due to file deletion on path {entry.Path}");
                            }
                        }
                    }
                }
                Thread.Sleep(1000); // Sleep to save cpu cycles since we don't really need to respond "right away"
            }
        }

        private void OnJsonFileRenamed(object sender, RenamedEventArgs e)
        {
            if (_fileActionsQueue.FirstOrDefault(x => x.Path == e.FullPath) == null)
            {
                var action = e.FullPath.EndsWith(".json") ? FileActionType.Renamed : FileActionType.Deleted;
                var path = e.FullPath.EndsWith(".json") ? e.FullPath : e.OldFullPath;
                _fileActionsQueue.Enqueue(new FileActionEntry(path, action));
            }

        }

        private void OnJsonFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_fileActionsQueue.FirstOrDefault(x => x.Path == e.FullPath) == null)
            {
                _fileActionsQueue.Enqueue(new FileActionEntry(e.FullPath, FileActionType.Changed));
            }

        }

        private void OnJsonFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (_fileActionsQueue.FirstOrDefault(x => x.Path == e.FullPath) == null)
            {
                _fileActionsQueue.Enqueue(new FileActionEntry(e.FullPath, FileActionType.Deleted));
            }
        }

        private void OnJsonFileCreated(object sender, FileSystemEventArgs e)
        {
            if (_fileActionsQueue.FirstOrDefault(x => x.Path == e.FullPath) == null)
            {
                _fileActionsQueue.Enqueue(new FileActionEntry(e.FullPath, FileActionType.Created));
            }
        }


        public Location LoadJsonLocation(string path)
        {
            if (File.Exists(path))
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },

                };

                try
                {
                    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read,
                               FileShare.ReadWrite))
                    {
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            // Read the JSON content from the file
                            string jsonContent = streamReader.ReadToEnd();

                            Location location = JsonConvert.DeserializeObject<Location>(jsonContent, settings);
                            if (location != null)
                            {
                                Log.Information($"Loaded location {location.Name} from file {path}");
                                return location;
                            }
                            else
                            {
                                Log.Warning($"Unable to parse Location from JSON File {path}");
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error when parsing Location from JSON file {path}");
                }
            }

            return null;
        }

        public void LoadLocations()
        {

            Log.Information("Loading Locations...");

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "locations");
            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);

            var jsonFiles = Directory.GetFiles(path, "*.json");
            var loaded = 0;
            foreach (var jsonFile in jsonFiles)
            {
                var location = LoadJsonLocation(jsonFile);
                if (location != null)
                {
                    location.JsonPath = jsonFile;
                    JsonLocations.Add(location);
                    location.Initialize();
                    loaded++;
                }
            }

            Log.Information($"Loaded {loaded} of {jsonFiles.Length} locations from JSON.");


        }


        private bool TryRemoveLocation(Location location)
        {
            // Nothing to remove so...
            if (location == null) return true;

            var jsonLocation = JsonLocations.FirstOrDefault(x => x.Name == location.Name);
            if (jsonLocation == null) return true;
            
           var removed =  JsonLocations.Remove(jsonLocation);
           jsonLocation.Dispose();
           return removed;

        }

        public List<Location> GetLocations()
        {
            return AllLocations;
        }

        public Location GetLocation(string name)
        {
            return AllLocations.FirstOrDefault(x => x.Name == name);
        }
    }
}
