using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MixtapeGui.Models;

namespace MixtapeGui.Services
{
    public class MusicFilePOCO
    {
        public string SourceFile;
        public double CanvasX;
        public double CanvasY;
        public MusicFilePOCO()
        {
            SourceFile = "";
            CanvasX = CanvasY = 0;
        }
        public MusicFilePOCO(string sf, double x, double y)
        {
            SourceFile = sf;
            CanvasX = x;
            CanvasY = y;
        }
    }

    public class ProjectPOCO
    {
        public List<MusicFilePOCO> MusicFiles;
        public List<List<MusicFilePOCO>> Connections;  // The inner list is actually a 2-tuple but json.net won't deserialize a Tuple

        public ProjectPOCO()
        {
            MusicFiles = new List<MusicFilePOCO>();
            Connections = new List<List<MusicFilePOCO>>();
        }

        public ProjectPOCO(List<MusicFilePOCO> files, List<List<MusicFilePOCO>> connections)
        {
            MusicFiles = files;
            Connections = connections;
        }
    }


    public class IOService {
        public static void SaveProject(Project project)
        {
            // Create the list of POCOs corresponding to the MusicFiles
            var musicFilePocos = new List<MusicFilePOCO>();
            var musicFileToPocoMapping = new Dictionary<MusicFile, MusicFilePOCO>();
            foreach (var mf in project.MusicFiles)
            {
                var poco = new MusicFilePOCO(mf.SourceFile, mf.CanvasX, mf.CanvasY);
                musicFilePocos.Add(poco);
                musicFileToPocoMapping[mf] = poco;
            }
            // Create the list of connections
            var connections = new List<List<MusicFilePOCO>>();
            foreach (var mfFrom in project.MusicFiles)
            {
                var mfTo = mfFrom.NextMusicFile;
                if (mfTo == null)
                {
                    continue;
                }
                var pocoFrom = musicFileToPocoMapping[mfFrom];
                var pocoTo = musicFileToPocoMapping[mfTo];
                var connection = new List<MusicFilePOCO>(){pocoFrom, pocoTo};
                connections.Add(connection);
            }
            // Create the containing project POCO
            var projectPOCO = new ProjectPOCO(musicFilePocos, connections);

            // And now save the project POCO to json
            var options = new JsonSerializerOptions {
                IncludeFields = true,
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
            };

            string jsonString = JsonSerializer.Serialize(projectPOCO, options);
            File.WriteAllText(project.ProjectFilename, jsonString);
        }

        public static Project LoadProject(string filename)
        {
            // Load the POCOs from json
            string jsonString = File.ReadAllText(filename);
            var options = new JsonSerializerOptions {
                IncludeFields = true,
                ReferenceHandler = ReferenceHandler.Preserve,
            };
            ProjectPOCO projectPOCO;
            try {
                projectPOCO = JsonSerializer.Deserialize<ProjectPOCO>(jsonString, options);
            }
            catch (System.InvalidOperationException e)
            {
                System.Diagnostics.Trace.WriteLine($"{e}");
                throw;
            }
            // Reconstruct the MusicFiles from the POCOs
            Project project = new Project();
            project.ProjectFilename = filename;
            var pocoToMusicFileMapping = new Dictionary<MusicFilePOCO, MusicFile>();
            foreach (var mfPOCO in projectPOCO.MusicFiles)
            {
                var mf = new MusicFile(project, mfPOCO.SourceFile);
                mf.CanvasX = mfPOCO.CanvasX;
                mf.CanvasY = mfPOCO.CanvasY;
                project.AddMusicFile(mf);
                pocoToMusicFileMapping[mfPOCO] = mf;
            }
            // Reconstruct the connections
            foreach (var connection in projectPOCO.Connections)
            {
                if (connection.Count != 2)
                {
                    System.Console.WriteLine("Corrupted JSON - malformed connection list");
                    continue;
                }
                var pocoFrom = connection[0];
                var pocoTo = connection[1];
                var mfFrom = pocoToMusicFileMapping[pocoFrom];
                var mfTo = pocoToMusicFileMapping[pocoTo];
                mfFrom.NextMusicFile = mfTo;
                if (mfTo != null)
                {
                    // This should always be true
                    mfTo.PrevMusicFile = mfFrom;
                }
            }
            return project;
        }
    }
}
