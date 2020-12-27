using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlaylistEditor.Models
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
        public string ProjectDirectory;
        public List<MusicFilePOCO> MusicFiles;
        public ProjectPOCO()
        {
            ProjectDirectory = "";
            MusicFiles = new List<MusicFilePOCO>();
        }

        public ProjectPOCO(string dir, List<MusicFilePOCO> files)
        {
            ProjectDirectory = dir;
            MusicFiles = files;
        }
    }


    public class ModelIO {
        public static void SaveProject(Project project)
        {
            var musicFilePocos = new List<MusicFilePOCO>();
            foreach (var mf in project.MusicFiles)
            {
                musicFilePocos.Add(new MusicFilePOCO(mf.SourceFile, mf.CanvasX, mf.CanvasY));
            }
            var projectPOCO = new ProjectPOCO(project.ProjectDirectory, musicFilePocos);

            var options = new JsonSerializerOptions {
                IncludeFields = true,
                WriteIndented = true,
            };

            string jsonString = JsonSerializer.Serialize(projectPOCO, options);
            File.WriteAllText(project.contentsFile, jsonString);
        }

        public static Project LoadProject(string projectDirectory)
        {
            string filename = Path.Join(projectDirectory, Project.ProjectContentsFileLeaf);
            string jsonString = File.ReadAllText(filename);
            var options = new JsonSerializerOptions {
                IncludeFields = true,
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
            Project project = new Project();
            project.ProjectDirectory = projectPOCO.ProjectDirectory;
            foreach (var mfPOCO in projectPOCO.MusicFiles)
            {
                var mf = new MusicFile(project, mfPOCO.SourceFile);
                mf.CanvasX = mfPOCO.CanvasX;
                mf.CanvasY = mfPOCO.CanvasY;
                project.AddMusicFile(mf);
            }
            return project;
        }
    }
}
